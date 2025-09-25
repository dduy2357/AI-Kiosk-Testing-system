using API.Attributes;
using API.Services;
using API.Services.Interfaces;
using API.ViewModels;
using API.ViewModels.Token;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AuthenPermission]
    public class DisabledKeyController : Authentication.Authentication
    {
        private readonly IDisabledKeyService _disabledKeyService;
        public DisabledKeyController(IDisabledKeyService disabledKeyService, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _disabledKeyService = disabledKeyService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll([FromQuery] DisabledKeySearchVM search)
        {
            var (message, rooms) = await _disabledKeyService.GetAll(search);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Lấy danh sách keyboard thành công.", data = rooms });
        }

        [HttpGet("GetOne/{keyId}")]
        public async Task<IActionResult> GetOne(string keyId)
        {
            var (message, key) = await _disabledKeyService.GetOne(keyId);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Lấy keyboard thành công.", data = key });
        }

        [HttpPost("ChangeActivate")]
        public async Task<IActionResult> ChangeActivate([FromBody] List<string> keyIds)
        {
            var (message, keys) = await _disabledKeyService.ChangeActivate(keyIds, UserToken.UserID);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Thay đổi trạng thái hoạt động của keyboard thành công.", data = keys });
        }

        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete([FromBody] List<string> keyIds)
        {
            var (message, keys) = await _disabledKeyService.DoDelete(keyIds, UserToken.UserID);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Xóa keyboard(s) thành công.", data = keys });
        }

        [HttpPost("CreateUpdate")]
        public async Task<IActionResult> CreateUpdate([FromBody] CreateUpdateDisabledKeyVM input)
        {
            var message = await _disabledKeyService.CreateUpdate(input, UserToken.UserID);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Tạo hoặc cập nhật keyboard thành công." });
        }
    }
}
