using API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigDesktopController : ControllerBase
    {
        private readonly IConfigDesktopService _iConfigDesktop;

        public ConfigDesktopController(IConfigDesktopService iConfigDesktop)
        {
            _iConfigDesktop = iConfigDesktop;
        }

        [HttpGet("get-configs")]
        public async Task<IActionResult> GetConfigurations()
        {
            var (message, configs) = await _iConfigDesktop.GetConfigurations();
            if (message.Length > 0) return BadRequest(new { success = false, message });
            return Ok(new { success = true, message = "Get desktop configuration successfully.", data = configs });
        }
    }
}
