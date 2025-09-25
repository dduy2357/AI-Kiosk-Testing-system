using API.Attributes;
using API.Services.Interfaces;
using API.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AuthenPermission]
    public class MonitorController : Authentication.Authentication
    {
        private readonly IMonitoringService _monitoringService;

        public MonitorController(IMonitoringService monitoringService, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _monitoringService = monitoringService;
        }

        [HttpGet("exam-overview")]
        public async Task<IActionResult> GetExamOverview([FromQuery] MonitorExamSearchVM search)
        {
            var (message, result) = await _monitoringService.GetExamOverview(search, UserToken.UserID!);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Get exam overview list successfully.", data = result });
        }

        [HttpGet("exam-monitor-detail")]
        public async Task<IActionResult> GetExamMonitorDetail([FromQuery] MonitorExamDetailSearchVM search)
        {
            var (message, result) = await _monitoringService.GetExamMonitorDetail(search, UserToken.UserID!);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Get exam monitor detail successfully.", data = result });
        }

        [HttpPost("add-student-extra-time")]
        public async Task<IActionResult> AddStudentExtraTime([FromBody] StudentExamExtraTime time)
        {
            var message = await _monitoringService.AddStudentExtraTime(time, UserToken.UserID!);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Add student extra time successfully." });
        }

        [HttpPost("add-exam-extra-time")]
        public async Task<IActionResult> AddExamExtraTime([FromBody] ExamExtraTime time)
        {
            var message = await _monitoringService.AddExamExtraTime(time, UserToken.UserID!);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Add exam extra time successfully." });
        }

        [HttpPost("finish-exam")]
        public async Task<IActionResult> FinishExam([FromBody] FinishExam finish)
        {
            var message = await _monitoringService.FinishExam(finish, UserToken.UserID!);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Finish exam successfully." });
        }

        [HttpPost("finish-student-exam")]
        public async Task<IActionResult> FinishStudentExam([FromBody] FinishStudentExam finish)
        {
            var message = await _monitoringService.FinishStudentExam(finish, UserToken.UserID!);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Finish student exam successfully." });
        }

        [HttpPost("re-assign-students")]
        public async Task<IActionResult> ReAssignExam([FromBody] ReAssignExam assignExam)
        {
            var (message, result) = await _monitoringService.ReAssignExam(assignExam, UserToken.UserID!);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Re-assign exam successfully.", data = result });
        }
        [HttpPost("re-assign-student")]
        public async Task<IActionResult> ReAssignStudent([FromBody] ReAssignStudent assignStudent)
        {
            var message = await _monitoringService.ReAssignStudent(assignStudent, UserToken.UserID!);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Re-assign student successfully." });
        }
    }
}
