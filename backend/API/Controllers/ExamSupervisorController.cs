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
    public class ExamSupervisorController : Authentication.Authentication
    {
        private readonly IExamSupervisorService _examSupervisorService;
        public ExamSupervisorController(IExamSupervisorService examSupervisorService, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _examSupervisorService = examSupervisorService;
        }

        [HttpGet("get-supervisors")]
        public async Task<IActionResult> GetSupervisors([FromQuery] SearchRequestVM search)
        {
            var (message, data) = await _examSupervisorService.GetSupervisors(search);
            if (message.Length > 0) return BadRequest(new { success = false, message, data = new List<object>() });
            return Ok(new { success = true, message = "Get supervisors successfully.", data });
        }

        [HttpGet("get-exams")]
        public async Task<IActionResult> GetExams([FromQuery] SearchRequestVM search)
        {
            var (message, data) = await _examSupervisorService.GetExams(search, UserToken.UserID);
            if (message.Length > 0) return BadRequest(new { success = false, message, data = new List<object>() });
            return Ok(new { success = true, message = "Get exams supervisor successfully.", data });
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll([FromQuery] string examId)
        {
            var (message, data) = await _examSupervisorService.GetAll(examId, UserToken.UserID);
            if (message.Length > 0) return BadRequest(new { success = false, message, data = new List<object>() });
            return Ok(new { success = true, message = "Get supervisors list successfully.", data });
    }

        [HttpPost("assign-supervisor")]
        public async Task<IActionResult> CreateUpdate([FromBody] EditExamSupervisorVM input)
        {
            var (message, data) = await _examSupervisorService.AssignSupervisor(input, UserToken.UserID);
            if (message.Length > 0) return BadRequest(new { success = false, message });
            return Ok(new { success = true, message = "Assign supervisors successfully.", data });
        }

        [HttpPost("Remove")]
        public async Task<IActionResult> Remove([FromBody] EditExamSupervisorVM supervisorIds)
        {
            var (message, data) = await _examSupervisorService.Remove(supervisorIds, UserToken.UserID);
            if (message.Length > 0)  return BadRequest(new { success = false, message });
            return Ok(new { success = true, message = "Remove supervisors successfully.", data });
        }
    }
}
