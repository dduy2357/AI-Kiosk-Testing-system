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
    public class RoomController : Authentication.Authentication
    {
        private readonly IRoomService _roomService;
        public RoomController(IRoomService roomService, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _roomService = roomService ?? throw new ArgumentNullException(nameof(roomService));
        }

        [HttpGet("GetAllRooms")]
        public async Task<IActionResult> GetAllRooms([FromQuery] SearchRoomVM search)
        {
            var (message, rooms) = await _roomService.GetAllRooms(search);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Lấy rooms thành công.", data = rooms });
        }

        [HttpGet("GetRoomById/{roomId}")]
        public async Task<IActionResult> GetRoomById(string roomId)
        {
            var (message, roomVM) = await _roomService.GetRoomByIdAsync(roomId);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Lấy room thành công.", data = roomVM });
        }

        [HttpPost("ChangeActivateRoom/{roomId}")]
        public async Task<IActionResult> ChangeActivateRoom(string roomId)
        {
            var message = await _roomService.ChangeActivateRoom(roomId, UserToken.UserID);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Thay đổi trạng thái hoạt động của phòng thành công." });
        }

        [HttpDelete("RemoveRoom/{roomId}")]
        public async Task<IActionResult> RemoveRoom(string roomId)
        {
            var message = await _roomService.DoRemoveRoom(roomId, UserToken.UserID);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Xóa phòng thành công." });
        }

        [HttpPost("CreateUpdateRoom")]
        public async Task<IActionResult> CreateUpdateRoom([FromBody] CreateUpdateRoomVM roomVM)
        {
            var message = await _roomService.CreateUpdateRoomVM(roomVM, UserToken.UserID);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Tạo hoặc cập nhật phòng thành công." });
        }
    }
}
