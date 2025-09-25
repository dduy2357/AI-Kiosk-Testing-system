using API.Attributes;
using API.Services.Interfaces;
using API.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AuthenPermission]
    public class NotificationController : Authentication.Authentication
    {
        private readonly INotificationService _notificationService;
        public NotificationController(INotificationService notificationService, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _notificationService = notificationService;
        }

        [HttpGet("GetAlert")]
        public async Task<IActionResult> GetAlert([FromQuery] NotifySearchVM search)
        {
            var (message, data) = await _notificationService.GetAlert(search, UserToken.UserID!);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Get notifications successfully.", data });
        }

        [HttpGet("GetDetail/{id}")]
        public async Task<IActionResult> GetOne(string id)
        {
            var (message, data) = await _notificationService.GetOne(id);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Get notification successfully.", data });
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] NotificationCreateVM notificationVM)
        {
            var message = await _notificationService.Create(notificationVM, UserToken.UserID!);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Notification created successfully." });
        }

        [HttpPost("MarkAsRead")]
        public async Task<IActionResult> MarkAsRead([FromQuery] string id)
        {
            var message = await _notificationService.MarkAsRead(id);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Notification marked as read successfully." });
        }

        [HttpPost("MarkAllAsRead")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var message = await _notificationService.MarkAllAsRead(UserToken.UserID!);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "All notifications marked as read successfully." });
        }

        [HttpGet("GetTotalUnread")]
        public async Task<IActionResult> GetTotalUnread()
        {
            var (message, total) = await _notificationService.GetTotalUnread(UserToken.UserID!);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Get total unread notifications successfully.", data = total });
        }
        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete([FromQuery] List<string> ids)
        {
            var message = await _notificationService.Delete(ids);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Notification deleted successfully." });
        }
    }
}
