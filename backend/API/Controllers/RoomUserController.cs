using API.Attributes;
using API.Services.Interfaces;
using API.ViewModels;
using API.ViewModels.Token;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AuthenPermission]
    public class RoomUserController : Authentication.Authentication
    {
        private readonly IRoomUserService _roomUserService;
        public RoomUserController(IRoomUserService roomUserService, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _roomUserService = roomUserService;
        }

        [HttpGet("GetAllRoomUsers")]
        public async Task<IActionResult> GetAllRoomUsers([FromQuery] SearchUserRoomExamVM search)
        {
            var (message, roomUsers) = await _roomUserService.GetUsersNotInRoom(search);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Get user list successfully.", data = roomUsers });
        }

        [HttpGet("GetUsersInRoom")]
        public async Task<IActionResult> GetRoomUsers([FromQuery] SearchRoomUserVM search)
        {
            var (message, result) = await _roomUserService.GetUsersInRoom(search);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Lấy danh sách người dùng trong room thành công.", data = result });
        }

        [HttpPost("AddUserToRoom")]
        public async Task<IActionResult> AddUserToRoom([Required] string roomId, [Required] List<string> userIdsOrCodes)
        {
            var (message, AddedUserIds, InvalidUserIds, DuplicatedUserIds) = await _roomUserService.AddStudentsToRoom(roomId, userIdsOrCodes, UserToken.UserID!);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new
            {
                success = true,
                message = "Add/import students to room successful.",
                data = new
                {
                    addedInputs = AddedUserIds,
                    invalidInputs = InvalidUserIds,
                    duplicatedInputs = DuplicatedUserIds
                }
            });
        }

        [HttpDelete("RemoveUsersFromRoom")]
        public async Task<IActionResult> RemoveUsersFromRoom(string roomId, List<string> userIds)
        {
            var message = await _roomUserService.RemoveUsersFromRoom(roomId, userIds, UserToken.UserID!);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Xóa người dùng khỏi room thành công." });
        }

        [HttpPost("AssignTeacher")]
        public async Task<IActionResult> AssignTeacherToRoom(string roomId, string userId)
        {
            var message = await _roomUserService.AssignTeacherToRoom(roomId, userId, UserToken.UserID!);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Gán giáo viên vào room thành công." });
        }

        [HttpPost("Update")]
        public async Task<IActionResult> Update([FromForm] UpdateRoomUserVM input)
        {
            var message = await _roomUserService.UpdateRoomUser(input, UserToken.UserID!);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Tạo hoặc cập nhật người dùng trong room thành công." });
        }

        [HttpGet("Export")]
        public async Task<IActionResult> ExportRoomUsers(string? roomId)
        {
            var (message, stream) = await _roomUserService.Export(roomId);
            if (message.Length > 0) return BadRequest(new { success = false, message });
            if (stream == null)  return NotFound(new { success = false, message = "No data to export." });
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "RoomUsers.xlsx");
        }

        [HttpPost("Import")]
        public async Task<IActionResult> ImportRoomUsers(IFormFile fileData, [Required] string roomId)
        {
            var (message, result) = await _roomUserService.Import(fileData, roomId, UserToken.UserID!);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Import users to room successful.", data = result });
        }

        [HttpPost("change-active-student")]
        public async Task<IActionResult> ToggleActive([FromBody] ToggleActiveRoomUserVM input)
        {
            var message = await _roomUserService.ToggleActive(input, UserToken.UserID!);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Toggle active status of room users successful." });
        }
    }
}
