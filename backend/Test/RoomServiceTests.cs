using API.Commons;
using API.Models;
using API.Services;
using API.ViewModels;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace API.Tests
{
    public class RoomServiceTests : IDisposable
    {
        private readonly Sep490Context _context;
        private readonly Mock<ILog> _mockLogger;
        private readonly RoomService _service;

        public RoomServiceTests()
        {
            var options = new DbContextOptionsBuilder<Sep490Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new Sep490Context(options);
            _mockLogger = new Mock<ILog>();
            _service = new RoomService(_context, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllRooms_RoomsFound_ReturnsNoMessage()
        {
            var sub = new Subject { SubjectName = "1", SubjectCode = "ROOM01", Credits = 2, SubjectId = "1" };
            var classes = new Class { CreatedBy = "testuser", ClassCode = "C001", ClassId = "1" };
            var room = new Room
            {
                RoomId = "1",
                RoomCode = "ROOM01",
                ClassId = "1",
                SubjectId = "1",
                Capacity = 30,
                IsActive = true
            };
            _context.Classes.Add(classes);
            _context.Subjects.Add(sub);
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            var search = new SearchRoomVM { CurrentPage = 1, PageSize = 10 };
            var (message, result) = await _service.GetAllRooms(search);
            Assert.Equal("", message);
            Assert.NotNull(result);
        }
        [Fact]
        public async Task GetAllRooms_RoomsNoFoundPage2_ReturnsMessage()
        {
            var sub = new Subject { SubjectName = "1", SubjectCode = "ROOM01", Credits = 2, SubjectId = "1" };
            var classes = new Class { CreatedBy = "testuser", ClassCode = "C001", ClassId = "1" };
            var room = new Room
            {
                RoomId = "1",
                RoomCode = "ROOM01",
                ClassId = "1",
                SubjectId = "1",
                Capacity = 30,
                IsActive = true
            };
            _context.Classes.Add(classes);
            _context.Subjects.Add(sub);
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            var search = new SearchRoomVM { CurrentPage = 2, PageSize = 10 };
            var (message, result) = await _service.GetAllRooms(search);
            Assert.Equal("No rooms found.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllRooms_SearchNoFound_ReturnsMessage()
        {
            var room = new Subject { SubjectName = "1", SubjectCode = "ROOM01", Credits = 2, SubjectId = "1" };
            _context.Subjects.Add(room);
            await _context.SaveChangesAsync();

            var search = new SearchRoomVM { CurrentPage = 1, PageSize = 10, TextSearch = "nonexistent" };
            var (message, result) = await _service.GetAllRooms(search);
            Assert.Equal("No rooms found.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllRooms_NoRoomsFound_ReturnsErrorMessage()
        {
            var search = new SearchRoomVM { CurrentPage = 1, PageSize = 10 };
            var (message, result) = await _service.GetAllRooms(search);
            Assert.Equal("No rooms found.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetRoomByIdAsync_NullId_ReturnsErrorMessage()
        {
            var (message, result) = await _service.GetRoomByIdAsync(null);
            Assert.Equal("Room Id not found!", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetRoomByIdAsync_NotFound_ReturnsErrorMessage()
        {
            var (message, result) = await _service.GetRoomByIdAsync("1");
            Assert.Equal("Room not found.", message);
            Assert.Null(result);
        }

        //[Fact]
        //public async Task GetRoomByIdAsync_GetExist_ReturnsNoMessage()
        //{
        //    var subject = new Subject { SubjectName = "1", SubjectCode = "ROOM01", Credits = 2, SubjectId = "1" };
        //    _context.Subjects.Add(subject);
        //    var classEntity = new Class { CreatedBy = "testuser", ClassCode = "C001", ClassId = "1" };
        //    _context.Classes.Add(classEntity);
        //    var room = new Room
        //    {
        //        RoomId = "1",
        //        RoomCode = "ROOM01",
        //        ClassId = "1",
        //        SubjectId = "1",
        //        Capacity = 30,
        //        IsActive = true
        //    };
        //    _context.Rooms.Add(room);
        //    await _context.SaveChangesAsync();
        //    var (message, result) = await _service.GetRoomByIdAsync("1");
        //    Assert.Equal(".", message);
        //    Assert.NotNull(result);
        //}

        //[Fact]
        //public async Task RemoveRoomByIdAsync_Exist_ReturnsNoMessage()
        //{
        //    var subject = new Subject { SubjectName = "1", SubjectCode = "ROOM01", Credits = 2, SubjectId = "1" };
        //    _context.Subjects.Add(subject);
        //    var classEntity = new Class { CreatedBy = "testuser", ClassCode = "C001", ClassId = "1" };
        //    _context.Classes.Add(classEntity);
        //    var room = new Room
        //    {
        //        RoomId = "1",
        //        RoomCode = "ROOM01",
        //        ClassId = "1",
        //        SubjectId = "1",
        //        Capacity = 30,
        //        IsActive = true
        //    };
        //    _context.Rooms.Add(room);
        //    await _context.SaveChangesAsync();
        //    var message = await _service.DoRemoveRoom("1", "token");
        //    Assert.Equal(".", message);
        //}

        [Fact]
        public async Task ChangeActivateRoom_NullId_ReturnsErrorMessage()
        {
            var message = await _service.ChangeActivateRoom(null, "token");
            Assert.Equal("Room Id cannot be null or empty.", message);
        }

        [Fact]
        public async Task ChangeActivateRoom_NotFound_ReturnsErrorMessage()
        {
            var message = await _service.ChangeActivateRoom("1", "token");
            Assert.Equal("Room not found.", message);
        }

        [Fact]
        public async Task DoRemoveRoom_NullId_ReturnsErrorMessage()
        {
            var message = await _service.DoRemoveRoom(null, "token");
            Assert.Equal("Room Id cannot be null or empty.", message);
        }

        [Fact]
        public async Task DoRemoveRoom_NotFound_ReturnsErrorMessage()
        {
            var message = await _service.DoRemoveRoom("1", "token");
            Assert.Equal("Room not found.", message);
        }

        [Fact]
        public async Task CreateUpdateRoomVM_NullInput_ReturnsErrorMessage()
        {
            var message = await _service.CreateUpdateRoomVM(null, "token");
            Assert.Equal("Room data cannot be null.", message);
        }

        [Fact]
        public async Task CreateUpdateRoomVM_MissingRequiredFields_ReturnsValidationError()
        {
            var room = new Subject { SubjectName = "1", SubjectCode = "ROOM01", Credits = 2, SubjectId = "1" };
            _context.Subjects.Add(room);
            await _context.SaveChangesAsync();
            var input = new CreateUpdateRoomVM
            {
                RoomCode = null,
                ClassId = null,
                SubjectId = "1",
                Capacity = 30
            };
            var message = await _service.CreateUpdateRoomVM(input, "token");
            Assert.Contains("Please select a class valid", message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CreateUpdateRoomVM_Successfully_NoMessageReturn()
        {
            var room = new Subject { SubjectName = "1", SubjectCode = "ROOM01", Credits = 2, SubjectId = "1" };
            _context.Subjects.Add(room);
            var classEntity = new Class { CreatedBy = "testuser", ClassCode = "C001", ClassId = "1" };
            _context.Classes.Add(classEntity);
            await _context.SaveChangesAsync();
            var input = new CreateUpdateRoomVM
            {
                RoomCode = "ROOM01",
                ClassId = "1",
                SubjectId = "1",
                Capacity = 101
            };
            var message = await _service.CreateUpdateRoomVM(input, "token");
            Assert.Contains("", message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CreateUpdateRoomVM_DuplicateRoomCode_ReturnsErrorMessage()
        {
            var room = new Room { RoomId = "1", RoomCode = "ROOM01", ClassId = "1", SubjectId = "1" };
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            var input = new CreateUpdateRoomVM
            {
                RoomCode = "ROOM01",
                ClassId = "1",
                SubjectId = "1",
                Capacity = 30
            };
            var message = await _service.CreateUpdateRoomVM(input, "token");
            Assert.Contains("already in use", message, StringComparison.OrdinalIgnoreCase);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}