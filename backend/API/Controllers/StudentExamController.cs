using API.Attributes;
using API.Models;
using API.Services.Interfaces;
using API.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AuthenPermission]
    public class StudentExamController : Authentication.Authentication
    {
        private readonly IStudentExamService _iService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public StudentExamController(IStudentExamService iService, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _iService = iService;
        }

        [HttpGet("list-exams")]
        public async Task<IActionResult> GetListExams()
        {
            var (message, data) = await _iService.GetList(UserToken.UserID!);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Get list exams successfully.", data });
        }

        [HttpGet("history-exams")]
        public async Task<IActionResult> GetHistoryExams([FromQuery] SearchStudentExamVM search)
        {
            var (message, data) = await _iService.GetHistoryExams(search, UserToken.UserID!);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Get history exams successfully.", data });
        }

        [HttpGet("history-exam-detail/{studentExamId}")]
        public async Task<IActionResult> GetExamDetail(string studentExamId)
        {
            var (message, data) = await _iService.GetHistoryExamDetail(studentExamId, UserToken.UserID!);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Get exam detail successfully.", data });
        }

        [HttpPost("access-exam")]
        public async Task<IActionResult> AccessExam([FromBody] StudentExamRequest otp)
        {
            var (message, result) = await _iService.AccessExam(otp, UserToken.UserID!, _httpContextAccessor.HttpContext!);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Access exam successfully.", data = result });
        }

        [HttpGet("exam-detail-by-id/{examId}")]
        public async Task<IActionResult> GetExamDetailById(string examId)
        {
            var (message, data) = await _iService.GetExamDetail(examId);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Get exam detail successfully.", data });
        }

        [HttpPost("submit-exam")]
        public async Task<IActionResult> SubmitExam([FromBody] SubmitExamRequest request)
        {
            var message = await _iService.SubmitExam(request, UserToken.UserID!, _httpContextAccessor.HttpContext!);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Submit exam successfully." });
        }

        [HttpPost("save-answer-temporary")]
        public async Task<IActionResult> SaveAnswerTemporary([FromBody] SubmitExamRequest input)
        {
            var message = await _iService.SaveAnswerTemporary(input, UserToken.UserID);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Save answer temporarily successfully." });
        }

        [HttpGet("get-saved-answers/{examId}")]
        public async Task<IActionResult> GetSavedAnswers(string examId)
        {
            var (message, data) = await _iService.GetSavedAnswers(examId, UserToken.UserID!);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Get saved answers successfully.", data });
        }

        [HttpGet("essay-exam/{studentExamId}/{examId}")]
        public async Task<IActionResult> GetEssayExam([Required] string studentExamId, [Required] string examId)
        {
            var (message, data) = await _iService.GetEssayExam(studentExamId, examId, UserToken.UserID!);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Get essay exam successfully.", data });
        }

        [HttpPost("mark-essay")]
        public async Task<IActionResult> MarkEssay([FromBody] MarkEssayRequest input)
        {
            var message = await _iService.MarkEssay(input, UserToken.UserID!);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Mark essay successfully." });
        }
    }
}
