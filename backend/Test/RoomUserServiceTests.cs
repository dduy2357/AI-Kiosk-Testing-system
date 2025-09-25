using API.Commons;
using API.Helper;
using API.Models;
using API.Services;
using API.ViewModels;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace API.Tests
{
    public class RoomUserServiceTests : IDisposable
    {
        private readonly Sep490Context _context;
        private readonly Mock<ILog> _mockLogger;
        private readonly RoomUserService _service;

        public RoomUserServiceTests()
        {
            var options = new DbContextOptionsBuilder<Sep490Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new Sep490Context(options);
            _mockLogger = new Mock<ILog>();
            _mockLogger.Setup(x => x.WriteActivity(It.IsAny<AddUserLogVM>())).ReturnsAsync("");
            _service = new RoomUserService(_context, _mockLogger.Object);
        }

        [Fact]
        public async Task GetListUser_NoUsersFound_ReturnsErrorMessage()
        {
            _context.Rooms.Add(new Room { RoomId = "r1", RoomCode = "Room 1", ClassId = "1", SubjectId = "1" });
            var search = new SearchUserRoomExamVM { CurrentPage = 1, PageSize = 10, RoomId = "r1" };
            var (message, result) = await _service.GetUsersNotInRoom(search);
            Assert.Equal("No users found.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUsersInRoom_NullRoomId_ReturnsErrorMessage()
        {
            var search = new SearchRoomUserVM { RoomId = null, CurrentPage = 1, PageSize = 10 };
            var (message, result) = await _service.GetUsersInRoom(search);
            Assert.Equal("RoomId is required.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task RemoveUsersFromRoom_NullRoomIdOrUserIds_ReturnsErrorMessage()
        {
            var message = await _service.RemoveUsersFromRoom(null, null, "token");
            Assert.Equal("RoomId and list of UserIds cannot be null or empty.", message);
        }

        [Fact]
        public async Task AddStudentsToRoom_NullRoomIdOrUserCodes_ReturnsErrorMessage()
        {
            var (message, _, _, _) = await _service.AddStudentsToRoom(null, null, "token");
            Assert.Equal("Room ID and User Codes/IDs cannot be null or empty.", message);
        }

        [Fact]
        public async Task GetListUser_WithUsers_ReturnsList()
        {
            _context.Majors.Add(new Major { Id = "1", Code = "Major 1", Name ="1" });
            _context.Users.Add(new User { UserId = "1", FullName = "User 1", UserCode = "U1", Email = "u1@email.com" });
            _context.Roles.Add(new Role { Id = 1, Name = "Admin" });
            _context.UserRoles.Add(new UserRole { UserId = "1", RoleId = 1 });
            _context.Rooms.Add(new Room { RoomId = "r1", RoomCode = "Room 1", ClassId = "1", SubjectId = "1" });
            await _context.SaveChangesAsync();
            var search = new SearchUserRoomExamVM { CurrentPage = 1, PageSize = 10, RoomId = "r1" };
            var (message, result) = await _service.GetUsersNotInRoom(search);
            Assert.Equal("", message);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetUsersInRoom_ValidRoomId_ReturnsUsers()
        {
            var room = new Room {RoomId = "r1", RoomCode = "Room 1", ClassId = "1", SubjectId = "1" };
            var user = new User { UserId = "1", FullName = "User 1", UserCode = "U1", Email = "u1@email.com" };
            _context.Rooms.Add(room);
            _context.Users.Add(user);
            _context.RoomUsers.Add(new RoomUser { RoomId = "r1", UserId = $"1", RoomUserId = "1" });
            await _context.SaveChangesAsync();
            var search = new SearchRoomUserVM { RoomId = "r1", CurrentPage = 1, PageSize = 10 };
            var (message, result) = await _service.GetUsersInRoom(search);
            Assert.Equal("", message);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task RemoveUsersFromRoom_Valid_ReturnsSuccess()
        {
            var room = new Room {RoomId = "r1", RoomCode = "Room 1", ClassId = "1", SubjectId = "1" };
            var user = new User { UserId = "1", FullName = "User 1", UserCode = "U1", Email = "u1@email.com" };
            _context.Rooms.Add(room);
            _context.Users.Add(user);
            _context.RoomUsers.Add(new RoomUser { RoomId = "r1", UserId = $"1", RoomUserId = "1" });
            _context.Exams.Add(new Exam { ExamId = "1", CreateUser = "1", RoomId = "r1", CreatedAt = DateTime.Now });
            await _context.SaveChangesAsync();
            var message = await _service.RemoveUsersFromRoom("r1", new List<string> { "1" }, "token");
            Assert.Equal("", message);
        }

        [Fact]
        public async Task RemoveUsersFromRoom_NotFound_ReturnsError()
        {
            var message = await _service.RemoveUsersFromRoom("r1", new List<string> { "notfound" }, "token");
            Assert.Contains("No matching users found in the room.", message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AddStudentsToRoom_Valid_ReturnsSuccess()
        {
            var room = new Room {RoomId = "r1", RoomCode = "Room 1", ClassId = "1", SubjectId = "1" };
            var user = new User { UserId = "1", FullName = "User 1", UserCode = "U1", Email = "u1@email.com" };
            _context.Rooms.Add(room);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            var (message, _, _, _) = await _service.AddStudentsToRoom("r1", new List<string> { "U1" }, "token");
            Assert.Equal("", message);
        }

        [Fact]
        public async Task AddStudentsToRoom_UserNotFound_ReturnsError()
        {
            var room = new Room {RoomId = "r1", RoomCode = "Room 1", ClassId = "1", SubjectId = "1" };
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            var (message, _, _, _) = await _service.AddStudentsToRoom("r1", new List<string> { "U2" }, "token");
            Assert.Contains("No new users to add.", message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetUsersInRoom_Pagination_WorksCorrectly()
        {
            var room = new Room {RoomId = "r1", RoomCode = "Room 1", ClassId = "1", SubjectId = "1" };
            _context.Rooms.Add(room);
            for (int i = 0; i < 12; i++)
            {
                var user = new User { UserId = $"u{i}", FullName = $"User {i}", UserCode = $"U{i}", Email = $"u{i}@email.com" };
                _context.Users.Add(user);
                _context.RoomUsers.Add(new RoomUser { RoomId = "r1", UserId = $"u{i}", RoomUserId = $"1{i}" });
            }
            await _context.SaveChangesAsync();
            var search = new SearchRoomUserVM { RoomId = "r1", CurrentPage = 2, PageSize = 10 };
            var (message, result) = await _service.GetUsersInRoom(search);
            Assert.Equal("", message);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task AddStudentsToRoom_SpecialCharacters_ReturnsSuccess()
        {
            var room = new Room { RoomId = "r1", RoomCode = "Room 1", ClassId = "1", SubjectId = "1" };
            var user = new User { UserId = "1", FullName = "User 1", UserCode = "!@#$%^&*()_+", Email = "u1@email.com" };
            _context.Rooms.Add(room);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            var (message, _, _, _) = await _service.AddStudentsToRoom("r1", new List<string> { "!@#$%^&*()_+" }, "token");
            Assert.Equal("", message);
        }

        [Fact]
        public async Task AddStudentsToRoom_LongUserCode_ReturnsSuccess()
        {
            var room = new Room { RoomId = "r1", RoomCode = "Room 1", ClassId = "1", SubjectId = "1" };
            var longCode = new string('a', 1000);
            var user = new User { UserId = "1", FullName = "User 1", UserCode = longCode, Email = "u1@email.com" };
            _context.Rooms.Add(room);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            var (message, _, _, _) = await _service.AddStudentsToRoom("r1", new List<string> { longCode }, "token");
            Assert.Equal("", message);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
} 