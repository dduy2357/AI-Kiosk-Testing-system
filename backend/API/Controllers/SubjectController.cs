using API.Attributes;
using API.Services;
using API.Services.Interfaces;
using API.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AuthenPermission]
    public class SubjectController : Authentication.Authentication
    {
        private readonly ISubjectService _subjectService;
        public SubjectController(ISubjectService subjectService, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _subjectService = subjectService;
        }

        [HttpGet("GetAllSubjects")]
        public async Task<IActionResult> GetAllSubjects([FromQuery] SearchSubjectVM search)
        {
            var (message, subjects) = await _subjectService.GetAllSubjects(search);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Lấy subjects thành công.", data = subjects });
        }

        [HttpGet("GetSubjectById/{subjectId}")]
        public async Task<IActionResult> GetSubjectById(string subjectId)
        {
            var (message, subjectVM) = await _subjectService.GetSubjectById(subjectId);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Lấy subject thành công.", data = subjectVM });
        }

        [HttpPost("ChangeActivateSubject/{subjectId}")]
        public async Task<IActionResult> ChangeActivateSubject(string subjectId)
        {
            var message = await _subjectService.ChangeActivateSubject(subjectId, UserToken.UserID);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Thay đổi trạng thái subject thành công." });
        }

        [HttpPost("CreateUpdateSubject")]
        public async Task<IActionResult> CreateUpdateSubject([FromBody] CreateUpdateSubjectVM subject)
        {
            var message = await _subjectService.CreateUpdateSubject(subject, UserToken.UserID);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Tạo hoặc cập nhật subject thành công." });
        }
    }
}
