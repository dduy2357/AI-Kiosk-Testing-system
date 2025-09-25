using API.Commons;
using API.Hubs;
using API.Models;
using API.Services;
using API.ViewModels;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace API.Tests
{
    public class NotificationServiceTests : IDisposable
    {
        private readonly Sep490Context _context;
        private readonly Mock<ILog> _mockLogger;
        private readonly Mock<IHubContext<NotifyHub>> _mockHubContext;
        private readonly NotificationService _service;

        public NotificationServiceTests()
        {
            var options = new DbContextOptionsBuilder<Sep490Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new Sep490Context(options);
            _mockLogger = new Mock<ILog>();

            // ===== MOCK HUB CONTEXT =====
            var mockClientProxy = new Mock<IClientProxy>();
            mockClientProxy
                .Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
                .Returns(Task.CompletedTask);

            var mockClients = new Mock<IHubClients>();
            mockClients
                .Setup(x => x.Group(It.IsAny<string>()))
                .Returns(mockClientProxy.Object);

            _mockHubContext = new Mock<IHubContext<NotifyHub>>();
            _mockHubContext
                .Setup(x => x.Clients)
                .Returns(mockClients.Object);

            _service = new NotificationService(_context, _mockLogger.Object, _mockHubContext.Object);
        }

        [Fact]
        public async Task GetAlert_NoNotificationsFound_ReturnsError()
        {
            var search = new NotifySearchVM { CurrentPage = 1, PageSize = 10 };
            var (message, result) = await _service.GetAlert(search, "user1");
            Assert.Equal("No notifications found.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task Create_NullInput_ReturnsError()
        {
            var message = await _service.Create(null, "user1");
            Assert.Equal("Notification data is null", message);
        }

        [Fact]
        public async Task Delete_EmptyIds_ReturnsError()
        {
            var message = await _service.Delete(new List<string>());
            Assert.Equal("Notification IDs cannot be null or empty.", message);
        }

        [Fact]
        public async Task GetAlert_WithNotifications_ReturnsList()
        {
            var user = new User
            {
                UserId = "1",
                FullName = "Tester",
                AvatarUrl = "avatar.jpg",
                Email = "tester@email.com"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            var notify = new Notification { NotifyId = "n1", SendToId = "user1", CreatedBy = "1", Message = "msg", IsRead = false, CreatedAt = DateTime.UtcNow };
            _context.Notifications.Add(notify);
            await _context.SaveChangesAsync();
            var search = new NotifySearchVM { CurrentPage = 1, PageSize = 10 };
            var (message, result) = await _service.GetAlert(search, "user1");
            Assert.Equal("", message);
            Assert.NotNull(result);
            Assert.Single((List<NotificationVM>)result.Result);
        }

        [Fact]
        public async Task GetAlert_WithNotifications_ReturnsListWithSearch()
        {
            var user = new User
            {
                UserId = "1",
                FullName = "Tester",
                AvatarUrl = "avatar.jpg",
                Email = "tester@email.com"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            var notify = new Notification { NotifyId = "n1", SendToId = "user1", CreatedBy = "1", Message = "msg", IsRead = false, CreatedAt = DateTime.UtcNow };
            _context.Notifications.Add(notify);
            await _context.SaveChangesAsync();
            var search = new NotifySearchVM { CurrentPage = 1, PageSize = 10, TextSearch= "msg" };
            var (message, result) = await _service.GetAlert(search, "user1");
            Assert.Equal("", message);
            Assert.NotNull(result);
            Assert.Single((List<NotificationVM>)result.Result);
        }

        [Fact]
        public async Task Create_ValidInput_ReturnsEmptyMessage()
        {
            var send = new NotificationCreateVM { SendToId = "user1", Message = "msg" };
            var message = await _service.Create(send, "user1");
            Assert.Equal("", message);
            Assert.Single(_context.Notifications);
        }
        [Fact]
        public async Task GetTotalUnread_InvalidUser_ReturnsZero()
        {
            var (message, count) = await _service.GetTotalUnread("notfound");
            Assert.Equal("", message);
            Assert.Equal(0, count);
        }


        [Fact]
        public async Task GetTotalUnread_NullNotifications_ReturnsZero()
        {
            var (message, count) = await _service.GetTotalUnread(null);
            Assert.Equal("", message);
            Assert.Equal(0, count);
        }

        [Fact]
        public async Task GetTotalUnread_NoNotifications_ReturnsZero()
        {
            var (message, count) = await _service.GetTotalUnread("user1");
            Assert.Equal("", message);
            Assert.Equal(0, count);
        }
        [Fact]
        public async Task GetTotalUnread_WithUnread_ReturnsCount()
        {
            _context.Notifications.Add(new Notification { NotifyId = "n1", CreatedBy = "1", SendToId = "user1", Message = "msg", IsRead = false });
            _context.Notifications.Add(new Notification { NotifyId = "n2", CreatedBy = "1", SendToId = "user1", Message = "msg2", IsRead = true });
            await _context.SaveChangesAsync();
            var (message, count) = await _service.GetTotalUnread("user1");
            Assert.Equal("", message);
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task Delete_ValidIds_ReturnsEmptyMessage()
        {
            var notify = new Notification { NotifyId = "n1", CreatedBy = "1", SendToId = "user1", Message = "msg" };
            _context.Notifications.Add(notify);
            await _context.SaveChangesAsync();
            var message = await _service.Delete(new List<string> { "n1" });
            Assert.Equal("", message);
            Assert.Empty(_context.Notifications);
        }

        [Fact]
        public async Task Delete_NotFoundIds_ReturnsError()
        {
            var message = await _service.Delete(new List<string> { "notfound" });
            Assert.Equal("No notifications found for the provided IDs.", message);
        }

        [Fact]
        public async Task GetAlert_Pagination_WorksCorrectly()
        {
            var user = new User
            {
                UserId = "1",
                FullName = "Tester",
                AvatarUrl = "avatar.jpg",
                Email = "tester@email.com"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            for (int i = 0; i < 15; i++)
                _context.Notifications.Add(new Notification { NotifyId = $"n{i}", CreatedBy = "1", SendToId = "user1", Message = $"msg{i}", IsRead = false, CreatedAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();
            var search = new NotifySearchVM { CurrentPage = 2, PageSize = 10 };
            var (message, result) = await _service.GetAlert(search, "user1");
            Assert.Equal("", message);
            Assert.NotNull(result);
            Assert.Equal(5, ((List<NotificationVM>)result.Result).Count);
        }

        [Fact]
        public async Task Create_WithSpecialCharacters_ReturnsSuccess()
        {
            var send = new NotificationCreateVM { SendToId = "user1", Message = "!@#$%^&*()_+" };
            var message = await _service.Create(send, "user1");
            Assert.Equal("", message);
        }

        [Fact]
        public async Task Delete_AlreadyDeleted_ReturnsError()
        {
            var notify = new Notification { NotifyId = "n1", CreatedBy = "1", SendToId = "user1", Message = "msg" };
            _context.Notifications.Add(notify);
            await _context.SaveChangesAsync();
            await _service.Delete(new List<string> { "n1" });
            var message = await _service.Delete(new List<string> { "n1" });
            Assert.Equal("No notifications found for the provided IDs.", message);
        }

       
        [Fact]
        public async Task Create_LongMessage_ReturnsSuccess()
        {
            var longMsg = new string('a', 1000);
            var send = new NotificationCreateVM { SendToId = "user1", Message = longMsg };
            var message = await _service.Create(send, "user1");
            Assert.Equal("", message);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}