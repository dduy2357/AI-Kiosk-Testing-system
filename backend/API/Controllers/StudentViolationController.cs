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
    public class StudentViolationController : Authentication.Authentication
    {
        private readonly IStudentViolationService _studentViolationService;
        public StudentViolationController(IStudentViolationService studentViolationService, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _studentViolationService = studentViolationService;
        }

        [HttpGet("get-list")]
        public async Task<IActionResult> GetList([FromQuery] SearchStudentViolation search)
        {
            var (message, result) = await _studentViolationService.GetAll(search);
            if (!string.IsNullOrEmpty(message))
            {
                return NotFound(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Get list student violations successfully.", data = result});
        }

        [HttpGet("get-one/{id}")]
        public async Task<IActionResult> GetOne(string id)
        {
            var (message, data) = await _studentViolationService.GetById(id);
            if (!string.IsNullOrEmpty(message))
            {
                return NotFound(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Get student violation successfully.", data });
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] SendStudentViolationVM send)
        {
            var message = await _studentViolationService.Create(send, UserToken.UserID!);
            if (!string.IsNullOrEmpty(message))
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Student violation created successfully." });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var message = await _studentViolationService.Delete(id, UserToken.UserID!);
            if (!string.IsNullOrEmpty(message))
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Student violation deleted successfully." });
        }
    }
}
