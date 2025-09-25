using API.Attributes;
using API.Services;
using API.Services.Interfaces;
using API.ViewModels;
using API.ViewModels.Token;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AuthenPermission]
    public class UserController : Authentication.Authentication
    {
        private readonly IUserService _iService;
        public UserController(IUserService iService, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _iService = iService;
        }

        [HttpGet("get-list")]
        public async Task<IActionResult> GetList([FromQuery] SearchUserVM input)
        {
            var (message, list) = await _iService.GetList(input, UserToken.UserID);
            if (message.Length > 0) return BadRequest(new { success = false, message, data = new List<object>() });
            return Ok(new { success = true, message = "Successfully retrieved user list.", data = list });
        }

        [HttpPost("toggle-active")]
        public async Task<IActionResult> DoToggleActive([FromBody] string userId)
        {
            string message = await _iService.DoToggleActive(UserToken.UserID, userId);
            if (message.Length > 0) return BadRequest(new { success = false, message });
            return Ok(new { success = true, message = "Toggle active completed successfully." });
        }

        [HttpGet("get-by-id")]
        public async Task<IActionResult> GetAccount(string userId)
        {
            var (msg, user) = await _iService.GetById(userId);
            if (msg.Length > 0) return BadRequest(new { success = false, message = msg, data = new List<object>() });
            return Ok(new { success = true, message = "Successfully retrieved account information.", data = user });
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] CreateUserVM input)
        {
            string message = await _iService.Create(input, UserToken.UserID);
            if (message.Length > 0) return BadRequest(new { success = false, message });
            return Ok(new { success = true, message = "Account created successfully." });
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update([FromForm] UpdateUserVM input)
        {
            string message = await _iService.Update(input, UserToken.UserID);
            if (message.Length > 0) return BadRequest(new { success = false, message });
            return Ok(new { success = true, message = "Account updated successfully." });
        }

        [HttpGet("check-email-existed")]
        public async Task<IActionResult> CheckEmailExisted(string? email)
        {
            string message = await _iService.CheckEmailExisted(email);
            if (message.Length > 0) return BadRequest(new { success = false, message });
            return Ok(new { success = true, message = "Email is valid." });
        }

        [HttpGet("check-phone-existed")]
        public async Task<IActionResult> CheckPhoneExisted(string? phone, string? userId = null)
        {
            string message = await _iService.CheckPhoneExisted(phone, userId);
            if (message.Length > 0) return BadRequest(new { success = false, message });
            return Ok(new { success = true, message = "Phone number is valid." });
        }

        [HttpGet("check-user-code-existed")]
        public async Task<IActionResult> CheckUserCodeExisted(string? code, string? userId = null)
        {
            string message = await _iService.CheckUserCodeExisted(code, userId);
            if (message.Length > 0) return BadRequest(new { success = false, message });
            return Ok(new { success = true, message = "User code is valid." });
        }

        [HttpPost("export-data")]
        public async Task<IActionResult> ExportData()
        {
            var (message, fileStream) = await _iService.ExportData(UserToken.UserID);
            if (message.Length > 0) return BadRequest(new { success = false, message });
            var fileName = $"UserExport_{DateTime.UtcNow :yyyyMMddHHmmss}.xlsx";
            return File(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost("check-import-data")]
        public IActionResult ImportData(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "Invalid file." });

            var msg = _iService.CheckImportData(file, out List<ErrorImport> result);
            if (msg.Length > 0) return BadRequest(new { success = false, message = msg });

            return Ok(new { success = true, message = "Data imported successfully.", data = result });
        }

        [HttpPost("add-list-user")]
        public async Task<IActionResult> AddListUser([FromBody] List<AddListUserVM> users)
        {
            if (users == null || !users.Any())
                return BadRequest(new { success = false, message = "Invalid user list." });

            var (message, result) = await _iService.AddListUser(users, UserToken.UserID);
            if (message.Length > 0) return BadRequest(new { success = false, message });
            return Ok(new { success = true, message = "User list added successfully.", data = result });
        }

        [HttpPost("change-avatar")]
        public async Task<IActionResult> ChangeAvatar([FromForm] ChangeAvatarVM avatarVM)
        {
            if (avatarVM == null || avatarVM.Avatar == null)
                return BadRequest(new { success = false, message = "Invalid avatar data." });

            string message = await _iService.ChangeAvatar(avatarVM, UserToken.UserID);
            if (message.Length > 0) return BadRequest(new { success = false, message });
            return Ok(new { success = true, message = "Avatar changed successfully." });
        }
    }
}
