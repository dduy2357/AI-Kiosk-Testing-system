using API.Attributes;
using API.Services.Interfaces;
using API.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AuthenPermission]
    public class ConfigUrlController : Authentication.Authentication
    {
        private readonly IConfigUrlService _iConfigUrl;
        public ConfigUrlController(IConfigUrlService iConfigUrl, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _iConfigUrl = iConfigUrl;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var (message, configUrls) = await _iConfigUrl.GetAll();
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Get list of successful URL configurations.", data = configUrls });
        }

        [HttpGet("GetOne/{urlId}")]
        public async Task<IActionResult> GetOne(string urlId)
        {
            var (message, configUrl) = await _iConfigUrl.GetOne(urlId);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Get URL configuration successfully.", data = configUrl });
        }

        [HttpPost("CreateUpdate")]
        public async Task<IActionResult> CreateUpdate([FromBody] CreateUpdateConfigUrlVM input)
        {
            var (message, data) = await _iConfigUrl.CreateUpdate(input, UserToken.UserID);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "URL configuration created or updated successfully.", data });
        }

        [HttpPost("ChangeActivate")]
        public async Task<IActionResult> ChangeActivate([FromBody] string id)
        {
            var message = await _iConfigUrl.ToggleConfigUrl(id, UserToken.UserID);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Toggle active URL configuration successfully." });
        }

        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete([FromBody] List<string> configIds)
        {
            var (message, configs) = await _iConfigUrl.DoRemove(configIds, UserToken.UserID);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Deleted configuration URL(s) successfully.", data = configs });
        }
    }
}
