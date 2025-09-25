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
    public class ClassesController : Authentication.Authentication
    {
        private readonly IClassesService _classesService;
        public ClassesController(IClassesService classesService, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _classesService = classesService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllClasses([FromQuery] SearchClassVM search)
        {
            var (message, classes) = await _classesService.GetAllClasses(search);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Lấy classes thành công.", data = classes });
        }

        [HttpGet("get-by-id/{classId}")]
        public async Task<IActionResult> GetClassById(string classId)
        {
            var (message, classVM) = await _classesService.GetClassById(classId);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Lấy class thành công.", data = classVM });
        }

        [HttpPost("deactivate-class/{classId}")]
        public async Task<IActionResult> DeactivateClass(string classId)
        {
            var message = await _classesService.DoDeactivateClass(classId, UserToken.UserID);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Deactivated class successfully." });
        }

        [HttpPost("create-update-class")]
        public async Task<IActionResult> CreateUpdateClass([FromBody] CreateUpdateClassVM input)
        {
            var message = await _classesService.DoCreateUpdateClass(input, UserToken.UserID);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Tạo hoặc cập nhật class thành công." });
        }
    }
}
