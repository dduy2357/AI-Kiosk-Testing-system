using API.Attributes;
using API.Helper;
using API.Services;
using API.Services.Interfaces;
using API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AuthenPermission]
    public class ExamController : Authentication.Authentication
    {
        private readonly IExamService _examService;


        public ExamController(IExamService examService, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _examService = examService;
        }

        [HttpPost("{examId}/status")]
        public async Task<IActionResult> ChangeStatus(string examId, [FromBody] ChangeStatusRequest request)
        {
            if (string.IsNullOrEmpty(UserToken.UserID))
                return Unauthorized(new { success = false, message = "User not authenticated." });

            var result = await _examService.ChangeExamStatusAsync(examId, request.NewStatus, UserToken.UserID);
            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        [HttpGet("student-exam-detail")]
        public async Task<IActionResult> GetStudentExamDetail([FromQuery] string studentExamId)
        {
            if (string.IsNullOrEmpty(UserToken.UserID))
                return Unauthorized(new { success = false, message = "User not authenticated." });
            var result = await _examService.GetStudentExamDetailAsync(studentExamId, UserToken.UserID);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message, data = new List<object>() });

            return Ok(new
            {
                success = true,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpGet("exams/{examId}/export-results")]
        public async Task<IActionResult> ExportStudentExamResults(string examId)
        {
            if (string.IsNullOrEmpty(UserToken.UserID))
                return Unauthorized(new { success = false, message = "User not authenticated." });

            var (message, fileStream) = await _examService.ExportStudentExamResultReport(examId, UserToken.UserID);
            if (fileStream == null)
                return BadRequest(new { success = false, message, data = new List<object>() });

            fileStream.Position = 0;
            return File(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "StudentExamResults.xlsx");
        }


        [HttpGet("guidelines/{examId}")]
        public async Task<IActionResult> GetExamGuideLines(string examId)
        {
            if (string.IsNullOrEmpty(UserToken.UserID))
                return Unauthorized(new { success = false, message = "User not authenticated." });

            var (success, message, guideLines) = await _examService.GetExamGuideLinesAsync(examId, UserToken.UserID);
            if (!success)
                return BadRequest(new { success = false, Message = message, data = new List<object>() });

            return Ok(new
            {
                success,
                GuideLines = guideLines
            });
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateExam([FromBody] UpdateExamRequest request)
        {
            if (string.IsNullOrEmpty(UserToken.UserID))
                return Unauthorized(new { success = false, message = "User not authenticated." });

            var (success, message) = await _examService.UpdateExamAsync(request, UserToken.UserID);
            if (!success)
                return BadRequest(new { success = false, Message = message });

            return Ok(new { success = true, Message = message });
        }

        [HttpGet("{examId}/ResultReport")]
        public async Task<IActionResult> GetExamResultReport(string examId)
        {
            if (string.IsNullOrEmpty(UserToken.UserID))
                return Unauthorized(new { success = false, message = "User not authenticated." });

            var result = await _examService.GetExamResultReportAsync(examId, UserToken.UserID);
            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(result.Data);
        }

        [HttpGet("{ExamId}/detail")]
        public async Task<IActionResult> GetExamDetail(string ExamId)
        {
            if (string.IsNullOrEmpty(UserToken.UserID))
                return Unauthorized(new { success = false, message = "User not authenticated." });

            var (success, message, data) = await _examService.GetExamDetailAsync(ExamId, UserToken.UserID);

            if (!success)
                return BadRequest(new { success = false, message, data = new List<object>() });

            return Ok(new
            {
                success = true,
                message,
                data
            });
        }


        [HttpGet("list")]
        public async Task<IActionResult> GetExamList([FromQuery] ExamListRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid request body." });

            if (string.IsNullOrEmpty(UserToken.UserID)) 
                return Unauthorized(new { message = "User not authenticated." });

            var (success, message, result) = await _examService.GetExamListAsync(request, UserToken.UserID);

            if (!success)
                return BadRequest(new { message, data = new List<object>() });

            return Ok(new { message, data = result });
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddExam([FromBody] AddExamRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid request body, required field(s) are missing." });

            if (string.IsNullOrEmpty(UserToken.UserID))
                return Unauthorized(new { message = "User not authenticated." });

            var (success, message, questions) = await _examService.AddExamAsync(request, UserToken.UserID);
            //var (success, message, questions) = await _examService.AddExamBuilderPatternAsync(request, UserToken.UserID);

            if (success)
            {
                return Ok(new
                {
                    success,
                    message,
                    questions 
                });
            }

            return BadRequest(new { success, message, questions = new List<object>() });
        }

        [HttpPost("assign-otp")]
        public async Task<IActionResult> AssignOTP([FromBody] CreateExamOtpVM input)
        {
            var (message, examOtp) = await _examService.AssignOTP(input, UserToken.UserID);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Assign OTP successfully.", data = examOtp });
        }

        //**
        //[HttpPost("create")]
        //public async Task<IActionResult> CreateExam([FromBody] AddExamRequest request)
        //{
        //    string message = await _examService.Handle(request, UserToken.UserID);
        //    if (message.Length > 0)
        //    {
        //        return BadRequest(new { success = false, message });
        //    }
        //    return Ok(new { success = true, message = "User created successfully." });
        //}
    }
}
