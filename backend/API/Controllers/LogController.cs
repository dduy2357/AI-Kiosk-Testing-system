using API.Attributes;
using API.Commons;
using API.ViewModels;
using API.ViewModels.Token;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AuthenPermission]
    public class LogController : Authentication.Authentication
    {
        private readonly ILog _log;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public LogController(ILog log, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _log = log;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("write-user-activity")]
        public async Task<IActionResult> WriteUserActivity([FromBody] AddUserLogVM log)
        {
            string message = await _log.WriteActivity(log);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "User activity logged successfully." });
        }

        [HttpPost("write-exam-activity")]
        public async Task<IActionResult> WriteExamActivity([FromForm] AddExamLogVM log)
        {
            string message = await _log.WriteActivity(log);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Exam activity logged successfully." });
        }

        [HttpGet("get-user-log-list")]
        public async Task<IActionResult> GetUserLogList([FromQuery] UserLogFilterVM filter)
        {
            var (message, result) = await _log.GetListUserLog(filter);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "User log list retrieved successfully.", data = result });
        }
        [HttpGet("get-exam-log-list")]
        public async Task<IActionResult> GetExamLogList([FromQuery] ExamLogFilterVM filter)
        {
            var (message, result) = await _log.GetListExamLog(filter);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Exam log list retrieved successfully.", data = result });
        }

        [HttpGet("get-user-log")]
        public async Task<IActionResult> GetUserLogById(string logId)
        {
            var (msg, log) = await _log.GetLogUserById(logId);
            if (msg.Length > 0)
            {
                return NotFound(new { success = false, message = msg, data = new List<object>() });
            }
            return Ok(new { success = true, message = "User log retrieved successfully.", data = log });
        }

        [HttpGet("get-exam-log")]
        public async Task<IActionResult> GetExamLogById(string logId)
        {
            var (msg, log) = await _log.GetLogExamById(logId);
            if (msg.Length > 0)
            {
                return NotFound(new { success = false, message = msg, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Exam log retrieved successfully.", data = log });
        }

        [HttpDelete("delete-user-log")]
        public async Task<IActionResult> DeleteUserLog([FromBody, Required] List<string> logIds)
        {
            string message = await _log.DeleteUserLog(logIds);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "User log deleted successfully." });
        }

        [HttpDelete("delete-exam-log")]
        public async Task<IActionResult> DeleteExamLog([FromBody, Required] List<string> logIds)
        {
            string message = await _log.DeleteExamLog(logIds);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Exam log deleted successfully." });
        }

        [HttpPost("export-log")]
        public async Task<IActionResult> ExportUserActivity([Required] List<string> logIds)
        {
            var (message, fileStream) = await _log.ExportLog(UserToken.UserID, logIds);
            if (message.Length > 0) return BadRequest(new { success = false, message });

            var fileName = $"UserExport_{DateTime.UtcNow :yyyyMMddHHmmss}.xlsx";
            return File(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost("export-exam-log")]
        public async Task<IActionResult> ExportExamActivity([Required] string studentExamId)
        {
            var (message, fileStream) = await _log.ExportExamLog(UserToken.UserID, studentExamId);
            if (message.Length > 0) return BadRequest(new { success = false, message });

            var fileName = $"ExamExport_{DateTime.UtcNow :yyyyMMddHHmmss}.xlsx";
            return File(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
