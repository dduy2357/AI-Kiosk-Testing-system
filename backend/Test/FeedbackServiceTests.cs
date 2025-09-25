using API.Commons;
using API.Models;
using API.Services;
using API.Services.Interfaces;
using API.ViewModels;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace API.Tests
{
    public class FeedbackServiceTests : IDisposable
    {
        private readonly Sep490Context _context;
        private readonly Mock<INotificationService> _mockNotification;
        private readonly FeedbackService _service;

        public FeedbackServiceTests()
        {
            var options = new DbContextOptionsBuilder<Sep490Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new Sep490Context(options);
            _mockNotification = new Mock<INotificationService>();
            _service = new FeedbackService(_context, _mockNotification.Object);
        }

        [Fact]
        public async Task GetList_NoFeedbacksFound_ReturnsError()
        {
            var search = new FeedbackSearchVM { CurrentPage = 1, PageSize = 10 };
            var (message, result) = await _service.GetList(search);
            Assert.Equal("No feedbacks found.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetList_FeedbacksFound_ReturnsNoMessage()
        {
            var user = new User { UserId = "u3", FullName = "User 3", Email = "u3@email.com" };
            var fb = new Feedback { Id = "f2", Title = "T2", Content = "C2", UserId = "u3", User = user, CreatedAt = DateTime.UtcNow };
            _context.Users.Add(user);
            _context.Feedbacks.Add(fb);
            await _context.SaveChangesAsync();
            var search = new FeedbackSearchVM { CurrentPage = 1, PageSize = 10, TextSearch = "C2" };
            var (message, result) = await _service.GetList(search);
            Assert.Equal("", message);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetList_FeedbacksFoundPage2_ReturnsNoMessage()
        {
            var user = new User { UserId = "u3", FullName = "User 3", Email = "u3@email.com" };
            var fb = new Feedback { Id = "f2", Title = "T2", Content = "C2", UserId = "u3", User = user, CreatedAt = DateTime.UtcNow };
            _context.Users.Add(user);
            _context.Feedbacks.Add(fb);
            await _context.SaveChangesAsync();
            var search = new FeedbackSearchVM { CurrentPage = 2, PageSize = 10, TextSearch = "C2" };
            var (message, result) = await _service.GetList(search);
            Assert.Equal("", message);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetOne_NotFound_ReturnsError()
        {
            var (message, result) = await _service.GetOne("notfound");
            Assert.Equal("No found feedback", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetOne_NoValid_ReturnsError()
        {
            var (message, result) = await _service.GetOne(null);
            Assert.Equal("No found feedback", message);
            Assert.Null(result);
        }
        [Fact]
        public async Task GetOne_Found_ReturnsNoMessage()
        {
            var user = new User { UserId = "u3", FullName = "User 3", Email = "u3@email.com" };
            var fb = new Feedback { Id = "f2", Title = "T2", Content = "C2", UserId = "u3", User = user, CreatedAt = DateTime.UtcNow };
            _context.Users.Add(user);
            _context.Feedbacks.Add(fb);
            await _context.SaveChangesAsync();
            var (message, result) = await _service.GetOne("f2");
            Assert.Equal("", message);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CreateUpdate_NullId_CreatesSuccessfully()
        {
            var model = new CreateUpdateFeedbackVM { Title = "Test", Content = "Content" };
            var message = await _service.CreateUpdate(model, "user1");
            Assert.Equal("", message);
            Assert.Single(_context.Feedbacks);
        }

        //[Fact]
        //public async Task CreateUpdate_CreateWithDifferentUser_Success()
        //{
        //    var user = new User { UserId = "u2", FullName = "User 2", Email = "u2@email.com" };
        //    _context.Users.Add(user);
        //    await _context.SaveChangesAsync();
        //    var model = new CreateUpdateFeedbackVM { Title = "Test2", Content = "Content2" };
        //    var message = await _service.CreateUpdate(model, "u2");
        //    Assert.Equal("", message);
        //    Assert.Single(_context.Feedbacks);
        //    Assert.Equal("u2", _context.Feedbacks.FirstAsync().Result.UserId);
        //}

        [Fact]
        public async Task CreateUpdate_UpdateWithDifferentUser_Success()
        {
            var user = new User { UserId = "u3", FullName = "User 3", Email = "u3@email.com" };
            var fb = new Feedback { Id = "f2", Title = "T2", Content = "C2", UserId = "u3", User = user, CreatedAt = DateTime.UtcNow };
            _context.Users.Add(user);
            _context.Feedbacks.Add(fb);
            await _context.SaveChangesAsync();
            var model = new CreateUpdateFeedbackVM { Id = "f2", Title = "T2 update", Content = "C2 update" };
            var message = await _service.CreateUpdate(model, "u3");
            Assert.Equal("", message);
            var updated = await _context.Feedbacks.FindAsync("f2");
            Assert.Equal("T2 update", updated.Title);
            Assert.Equal("C2 update", updated.Content);
        }

        [Fact]
        public async Task Delete_FeedbackOfAnotherUser_Success()
        {
            var fb = new Feedback { Id = "f3", Title = "T3", Content = "C3", UserId = "u4", CreatedAt = DateTime.UtcNow };
            _context.Feedbacks.Add(fb);
            await _context.SaveChangesAsync();
            var message = await _service.Delete("f3");
            Assert.Equal("", message);
            Assert.Empty(_context.Feedbacks);
        }

        [Fact]
        public async Task GetList_TextSearch_ReturnsFiltered()
        {
            var user = new User { UserId = "u5", FullName = "User 5", Email = "u5@email.com" };
            var fb1 = new Feedback { Id = "f4", Title = "SpecialTitle", Content = "C4", UserId = "u5", User = user, CreatedAt = DateTime.UtcNow };
            var fb2 = new Feedback { Id = "f5", Title = "Normal", Content = "C5", UserId = "u5", User = user, CreatedAt = DateTime.UtcNow };
            _context.Users.Add(user);
            _context.Feedbacks.AddRange(fb1, fb2);
            await _context.SaveChangesAsync();
            var search = new FeedbackSearchVM { CurrentPage = 1, PageSize = 10, TextSearch = "SpecialTitle" };
            var (message, result) = await _service.GetList(search);
            Assert.Equal("", message);
            Assert.NotNull(result);
            var list = (IEnumerable<object>)result.Result;
            Assert.Single(list);
        }

        [Fact]
        public async Task CreateUpdate_EmptyTitleOrContent_StillCreates()
        {
            var model = new CreateUpdateFeedbackVM { Title = "", Content = "" };
            var message = await _service.CreateUpdate(model, "u6");
            Assert.Equal("", message);
            Assert.Single(_context.Feedbacks);
        }

        [Fact]
        public async Task Delete_IdNull_ReturnsError()
        {
            var message = await _service.Delete(null);
            Assert.Equal("Feedback ID cannot be null or empty.", message);
        }



        [Fact]
        public async Task Delete_IdEmpty_ReturnsError()
        {
            var message = await _service.Delete("");
            Assert.Equal("Feedback ID cannot be null or empty.", message);
        }
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
} 