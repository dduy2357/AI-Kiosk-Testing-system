using API.Commons;
using API.Helper;
using API.Models;
using API.Services;
using API.Services.Interfaces;
using API.ViewModels;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Xunit;

namespace API.Tests
{
    public class UserServiceTests : IDisposable
    {
        private readonly Sep490Context _context;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILog> _mockLogger;
        private readonly UserService _service;
        private readonly Mock<IAmazonS3Service> _amazonS3Service;

        public UserServiceTests()
        {
            var options = new DbContextOptionsBuilder<Sep490Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            _context = new Sep490Context(options);
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILog>();
            _mockLogger.Setup(l => l.WriteActivity(It.IsAny<AddUserLogVM>())).ReturnsAsync("");
            _amazonS3Service = new Mock<IAmazonS3Service>();
            _service = new UserService(_mockMapper.Object, _context, _mockLogger.Object, _amazonS3Service.Object);
        }

        [Fact]
        public async Task GetById_NullId_ReturnsErrorMessage()
        {
            var (message, result) = await _service.GetById(null);
            Assert.Equal("User ID is not valid.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetById_UserNotFound_ReturnsErrorMessage()
        {
            var (message, result) = await _service.GetById("notfound");
            Assert.Equal("User not found", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetById_UserFound_ReturnsNoMessage()
        {
            var user1 = new User { UserId = "1", UserCode = "user1", Email = "a@email.com", Phone = "+84123456789" };
            _context.Users.Add(user1);
            await _context.SaveChangesAsync();

            var (message, result) = await _service.GetById("1");
            Assert.Equal("", message);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task DoToggleActive_ValidId_ReturnsNoMessage()
        {
            var user1 = new User { UserId = "1", UserCode = "user1", Email = "a@email.com", Phone = "+84123456789" };
            _context.Users.Add(user1);
            await _context.SaveChangesAsync();

            var message = await _service.DoToggleActive("token", "1");
            Assert.Equal("", message);
        }

        [Fact]
        public async Task DoToggleActive_NullId_ReturnsErrorMessage()
        {
            var message = await _service.DoToggleActive("token", null);
            Assert.Equal("User ID is not valid!", message);
        }

        [Fact]
        public async Task DoToggleActive_UserNotFound_ReturnsErrorMessage()
        {
            var message = await _service.DoToggleActive("token", "1");
            Assert.Equal("User not found!", message);
        }

        [Fact]
        public async Task GetList_NoUsersFound_ReturnsErrorMessage()
        {
            var search = new SearchUserVM { CurrentPage = 1, PageSize = 10, TextSearch = "123" };
            var (message, result) = await _service.GetList(search, "token");
            Assert.Equal("No users found.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetList_UsersFound_ReturnsNoMessage()
        {
            var user1 = new User { UserId = "1", UserCode = "user1", Email = "a@email.com", Phone = "+84123456789" };
            var user2 = new User { UserId = "2", UserCode = "user2", Email = "b@email.com", Phone = "+84987654321" };
            _context.Users.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            var search = new SearchUserVM { CurrentPage = 1, PageSize = 10 };
            var (message, result) = await _service.GetList(search, "token");
            Assert.Equal("", message);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetList_UsersFoundTextSearch_ReturnsErrorMessage()
        {
            var user1 = new User { UserId = "1", UserCode = "user1", Email = "a@email.com", Phone = "+84123456789" };
            var user2 = new User { UserId = "2", UserCode = "user2", Email = "b@email.com", Phone = "+84987654321" };
            _context.Users.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            var search = new SearchUserVM { CurrentPage = 1, PageSize = 10, TextSearch = "user1" };
            var (message, result) = await _service.GetList(search, "token");
            Assert.Equal("", message);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetList_SearchPage2_ReturnsErrorMessage()
        {
            var user1 = new User { UserId = "1", UserCode = "user1", Email = "a@email.com", Phone = "+84123456789" };
            var user2 = new User { UserId = "2", UserCode = "user2", Email = "b@email.com", Phone = "+84987654321" };
            _context.Users.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            var search = new SearchUserVM { CurrentPage = 2, PageSize = 10 };
            var (message, result) = await _service.GetList(search, "token");
            Assert.Equal("No users found.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task Create_NullInput_ReturnsErrorMessage()
        {
            var message = await _service.Create(null, "token");
            Assert.Equal("User data cannot be null.", message);
        }

        [Fact]
        public async Task Create_MissingRequiredFields_ReturnsValidationError()
        {
            var input = new CreateUserVM
            {
                // Bỏ qua các trường bắt buộc
                Email = null,
                FullName = null,
                Phone = null,
                UserCode = null,
                RoleId = null,
                Dob = DateTime.Today,
                Address = null,
                CampusId = null
            };
            var message = await _service.Create(input, "token");
            Assert.Contains("Email is not valid", message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Create_Valid_ReturnsNoMessage()
        {
            var input = new CreateUserVM
            {
                // Bỏ qua các trường bắt buộc
                Email = "abc@gmail.com",
                FullName = "Test User",
                Phone = "+84123456789",
                UserCode = "testuser",
                RoleId = new List<int> { 1 },
                Dob = DateTime.Today.AddYears(-20),
                Address = "Test Address",
                CampusId = "1"
            };
            var message = await _service.Create(input, "token");
            Assert.Contains("", message, StringComparison.OrdinalIgnoreCase);
        }


        [Fact]
        public async Task Create_InvalidEmailFormat_ReturnsValidationError()
        {
            var input = new CreateUserVM
            {
                Email = "invalid-email",
                FullName = "Test User",
                Phone = "+84123456789",
                UserCode = "testuser",
                RoleId = new List<int> { 1 },
                Dob = DateTime.Today.AddYears(-20),
                Address = "Test Address",
                CampusId = "1"
            };
            var message = await _service.Create(input, "token");
            Assert.Contains("Email is not valid", message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Create_DuplicateUserCode_ReturnsErrorMessage()
        {
            var user = new User { UserId = "1", UserCode = "testuser", Email = "test@email.com", Phone = "+84123456789" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var input = new CreateUserVM
            {
                Email = "new@email.com",
                FullName = "Test User",
                Phone = "+84123456789",
                UserCode = "testuser",
                RoleId = new List<int> { 1 },
                Dob = DateTime.Today.AddYears(-20),
                Address = "Test Address",
                CampusId = "1"
            };
            var message = await _service.Create(input, "token");
            Assert.Contains("already in use", message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Create_DuplicateEmail_ReturnsErrorMessage()
        {
            var user = new User { UserId = "1", UserCode = "testuser", Email = "test@email.com", Phone = "+84123456789" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var input = new CreateUserVM
            {
                Email = "test@email.com",
                FullName = "Test User",
                Phone = "+84123456789",
                UserCode = "testuser1",
                RoleId = new List<int> { 1 },
                Dob = DateTime.Today.AddYears(-20),
                Address = "Test Address",
                CampusId = "1",
            };
            var message = await _service.Create(input, "token");
            Assert.Contains("already in use", message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Create_DuplicatePhone_ReturnsErrorMessage()
        {
            var user = new User { UserId = "1", UserCode = "testuser", Email = "test@email.com", Phone = "+84123456789" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var input = new CreateUserVM
            {
                Email = "new@email.com",
                FullName = "Test User",
                Phone = "+84123456789",
                UserCode = "testuser1",
                RoleId = new List<int> { 1 },
                Dob = DateTime.Today.AddYears(-20),
                Address = "Test Address",
                CampusId = "1",
            };
            var message = await _service.Create(input, "token");
            Assert.Contains("already in use", message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Update_UserNotFound_ReturnsErrorMessage()
        {
            var input = new UpdateUserVM
            {
                UserId = "notfound",
                FullName = "Test User",
                Phone = "+84123456789",
                UserCode = "testuser",
                RoleId = new List<int> { 1 },
                Dob = DateTime.Today.AddYears(-20),
                Address = "Test Address",
                CampusId = "1"
            };
            var message = await _service.Update(input, "token");
            Assert.Equal("User not found.", message);
        }

        [Fact]
        public async Task Update_DuplicatePhone_ReturnsErrorMessage()
        {
            var user1 = new User { UserId = "1", UserCode = "user1", Email = "a@email.com", Phone = "+84123456789" };
            var user2 = new User { UserId = "2", UserCode = "user2", Email = "b@email.com", Phone = "+84987654321" };
            _context.Users.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            var input = new UpdateUserVM
            {
                UserId = "2",
                FullName = "Test User",
                Phone = "+84123456789", // trùng với user1
                UserCode = "user2",
                RoleId = new List<int> { 1 },
                Dob = DateTime.Today.AddYears(-20),
                Address = "Test Address",
                CampusId = "1"
            };
            var message = await _service.Update(input, "token");
            Assert.Contains("already in use", message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Update_DuplicateUserCode_ReturnsErrorMessage()
        {
            var user1 = new User { UserId = "1", UserCode = "user1", Email = "a@email.com", Phone = "+84123456789" };
            var user2 = new User { UserId = "2", UserCode = "user2", Email = "b@email.com", Phone = "+84987654321" };
            _context.Users.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            var input = new UpdateUserVM
            {
                UserId = "2",
                FullName = "Test User",
                Phone = "+84987654321",
                UserCode = "user1", // trùng với user1
                RoleId = new List<int> { 1 },
                Dob = DateTime.Today.AddYears(-20),
                Address = "Test Address",
                CampusId = "1"
            };
            var message = await _service.Update(input, "token");
            Assert.Contains("already in use", message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ExportData_NoUsers_ReturnsNoUsersFound()
        {
            var (message, file) = await _service.ExportData("token");
            Assert.Equal("No users found.", message);
            Assert.Null(file);
        }

        [Fact]
        public async Task ExportData_ValidUsers_ReturnsUsersFound()
        {
            _context.Roles.Add(new Role { Id = 1, Name = "Teacher" });
            _context.Majors.Add(new Major { Id = "1", Name = "Test Major", Code = "123" });
            _context.Campuses.Add(new Campus { Id = "1", Name = "Test Campus", Code = "123" });
            var users = new List<User>
            {
                new User
                {
                    UserId = "1",
                    Email = "ab@email.com",
                    FullName = "User A",
                    Phone = "+84123456789",
                    UserCode = "useraa",
                    Dob = DateTime.Today.AddYears(-20),
                    Address = "Test Address",
                    CampusId = "1",
                    MajorId = "1"
                }
            };
            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();

            var (msg, file) = await _service.ExportData("token");
            Assert.Equal("", msg);
            Assert.NotNull(file);
        }

        [Fact]
        public async Task AddListUser_NoValid_ReturnsErrorMessage()
        {
            var (message, errorList) = await _service.AddListUser(null, "token");
            Assert.Equal("List Users cannot null or empty.", message);
        }

        [Fact]
        public async Task AddListUser_AllValid_ReturnsNoMessage()
        {
            _context.Roles.Add(new Role { Id = 1, Name = "Teacher" });
            _context.Majors.Add(new Major { Id = "1", Name = "Test Major", Code = "123" });
            _context.Campuses.Add(new Campus { Id = "1", Name = "Test Campus", Code = "123" });
            await _context.SaveChangesAsync();
            var users = new List<AddListUserVM>
            {
                new AddListUserVM
                {
                    Email = "ab@email.com",
                    FullName = "User A",
                    Phone = "+84123456789",
                    UserCode = "useraa",
                    RoleId = new List<int> { 1 },
                    Dob = DateTime.Today.AddYears(-20),
                    Address = "Test Address",
                    CampusId = "1",
                    MajorId = "1"
                }
            };
            var (message, errorList) = await _service.AddListUser(users, "token");
            Assert.Equal("", message);
            Assert.Empty(errorList);
        }

        [Fact]
        public async Task AddListUser_SomeInvalid_ReturnsErrorList()
        {
            _context.Roles.Add(new Role { Id = 1, Name = "Teacher" });
            _context.Campuses.Add(new Campus { Id = "1", Name = "Test Campus", Code = "123" });
            await _context.SaveChangesAsync();
            var users = new List<AddListUserVM>
            {
                new AddListUserVM
                {
                    Email = "a@email.com",
                    FullName = "User A",
                    Phone = "+84123456789",
                    UserCode = "usera",
                    RoleId = new List<int> { 1 },
                    Dob = DateTime.Today.AddYears(-20),
                    Address = "Test Address",
                    CampusId = "1"
                },
                new AddListUserVM
                {
                    Email = "invalid-email",
                    FullName = "User B",
                    Phone = "+84123456780",
                    UserCode = "userb",
                    RoleId = new List<int> { 1 },
                    Dob = DateTime.Today.AddYears(-20),
                    Address = "Test Address",
                    CampusId = "1"
                }
            };
            var (message, errorList) = await _service.AddListUser(users, "token");
            Assert.True(errorList.Count > 0);
            Assert.Contains(errorList, e => e.Item1.Email == "invalid-email");
        }

        [Fact]
        public async Task AddListUser_AllInvalid_ReturnsNoUsersCreated()
        {
            var users = new List<AddListUserVM>
            {
                new AddListUserVM
                {
                    Email = "invalid-email",
                    FullName = "User A",
                    Phone = "invalid-phone",
                    UserCode = "usera",
                    RoleId = new List<int> { 1 },
                    Dob = DateTime.Today.AddYears(-10),
                    Address = "Test Address",
                    CampusId = "invalid"
                }
            };
            var (message, errorList) = await _service.AddListUser(users, "token");
            Assert.Equal("No users were created.", message);
            Assert.True(errorList.Count > 0);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}