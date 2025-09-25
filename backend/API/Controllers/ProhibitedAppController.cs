using API.Attributes;
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
    public class ProhibitedAppController : Authentication.Authentication
    {
        private readonly IProhibitedAppService _prohibitedAppService;
        public ProhibitedAppController(IProhibitedAppService prohibitedAppService, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _prohibitedAppService = prohibitedAppService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll([FromQuery] ProhibitedAppSearchVM search)
        {
            var (message, result) = await _prohibitedAppService.GetAll(search);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Lấy danh sách ứng dụng cấm thành công.", data = result });
        }

        [HttpGet("GetOne/{appId}")]
        public async Task<IActionResult> GetOne(string appId)
        {
            var (message, app) = await _prohibitedAppService.GetOne(appId);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Lấy ứng dụng cấm thành công.", data = app });
        }

        [HttpPost("CreateUpdate")]
        public async Task<IActionResult> CreateUpdate([FromForm] CreateUpdateProhibitedAppVM input)
        {
            string usertoken = GetUserId();
            var message = await _prohibitedAppService.CreateUpdate(input, usertoken);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Cập nhật ứng dụng cấm thành công." });
        }

        [HttpPost("ChangeActivate")]
        public async Task<IActionResult> ChangeActivate([FromBody] List<string> appIds)
        {
            string usertoken = GetUserId();
            var (message, apps) = await _prohibitedAppService.ChangeActivate(appIds, usertoken);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Thay đổi trạng thái hoạt động của ứng dụng cấm thành công.", data = apps });
        }

        [HttpDelete("DoRemove")]
        public async Task<IActionResult> DoRemove([FromBody] List<string> appIds)
        {
            var (message, removedApps) = await _prohibitedAppService.DoRemove(appIds, UserToken.UserID);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Xóa ứng dụng cấm thành công.", data = removedApps });
        }
    }
}
