using API.Hubs;
using API.Models;
using API.Services;
using API.Services.Interfaces;
using API.ViewModels;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace API.Tests
{
    public class FaceCaptureServiceTests : IDisposable
    {
        private readonly Sep490Context _context;
        private readonly Mock<IAmazonS3Service> _mockS3;
        private readonly FaceCaptureService _service;
        private readonly IHubContext<ExamHub> _hubContext;
        private readonly HttpClient _httpClient;

        public FaceCaptureServiceTests()
        {
            var options = new DbContextOptionsBuilder<Sep490Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new Sep490Context(options);
            _hubContext = new Mock<IHubContext<ExamHub>>().Object; // Mocking the hub context if needed
            _mockS3 = new Mock<IAmazonS3Service>();
            // Mocking the HttpClient if needed
            _httpClient = new HttpClient();
            _service = new FaceCaptureService(_context, _mockS3.Object, _hubContext, _httpClient);
        }

        [Fact]
        public async Task GetList_StudentExamNotFound_ReturnsError()
        {
            var input = new FaceCaptureSearchVM { StudentExamId = "notfound", ExamId = "e1", CurrentPage = 1, PageSize = 10 };
            var (message, result) = await _service.GetList(input);
            Assert.Equal("Student exam not found.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetList_WithData_ReturnsList()
        {
            var user = new User
            {
                UserId = "1",
                FullName = "Tester",
                AvatarUrl = "avatar.jpg",
                Email = "tester@email.com"
            };
            _context.Users.Add(user);
            _context.Exams.Add(new Exam { ExamId = "1", Title = "Test Exam", CreateUser = "1", RoomId = "1" });
            var studentExam = new StudentExam { StudentExamId = "se1", ExamId = "1", StudentId = "1" };
            var capture = new FaceCapture { CaptureId = "c1", StudentExamId = "se1", ImageUrl = "url1", CreatedAt = DateTime.UtcNow };
            _context.StudentExams.Add(studentExam);
            _context.FaceCaptures.Add(capture);
            await _context.SaveChangesAsync();
            var input = new FaceCaptureSearchVM { StudentExamId = "se1", ExamId = "1", CurrentPage = 1, PageSize = 10 };
            var (message, result) = await _service.GetList(input);
            Assert.Equal("", message);
            Assert.NotNull(result);
            var vm = (FaceCaptureVM)result.Result;
            Assert.NotNull(vm.Captures);
        }

        [Fact]
        public async Task GetList_WithSpecialCharacters_ReturnsList()
        {
            var user = new User
            {
                UserId = "1",
                FullName = "Tester",
                AvatarUrl = "avatar.jpg",
                Email = "tester@email.com"
            };
            _context.Users.Add(user);
            _context.Exams.Add(new Exam { ExamId = "1", Title = "Test Exam", CreateUser = "1", RoomId = "1" });
            var studentExam = new StudentExam { StudentExamId = "se1", ExamId = "1", StudentId = "1" };
            var capture = new FaceCapture { CaptureId = "c1", StudentExamId = "se1", ImageUrl = "!@#$%^&*()_+", CreatedAt = DateTime.UtcNow };
            _context.StudentExams.Add(studentExam);
            _context.FaceCaptures.Add(capture);
            await _context.SaveChangesAsync();
            var input = new FaceCaptureSearchVM { StudentExamId = "se1", ExamId = "1", CurrentPage = 1, PageSize = 10 };
            var (message, result) = await _service.GetList(input);
            Assert.Equal("", message);
            Assert.NotNull(result);
            var vm = (FaceCaptureVM)result.Result;
            Assert.NotNull(vm.Captures);
            Assert.Single(vm.Captures);
        }

        [Fact]
        public async Task GetList_Pagination_WorksCorrectly()
        {
            var user = new User
            {
                UserId = "1",
                FullName = "Tester",
                AvatarUrl = "avatar.jpg",
                Email = "tester@email.com"
            };
            _context.Users.Add(user);
            _context.Exams.Add(new Exam { ExamId = "1", Title = "Test Exam", CreateUser = "1", RoomId = "1" });
            var studentExam = new StudentExam { StudentExamId = "se1", ExamId = "1", StudentId = "1" };
            for (int i = 0; i < 12; i++)
                _context.FaceCaptures.Add(new FaceCapture { CaptureId = $"c{i}", StudentExamId = "se1", ImageUrl = $"url{i}", CreatedAt = DateTime.UtcNow });
            _context.StudentExams.Add(studentExam);
            await _context.SaveChangesAsync();
            var input = new FaceCaptureSearchVM { StudentExamId = "se1", ExamId = "1", CurrentPage = 2, PageSize = 10 };
            var (message, result) = await _service.GetList(input);
            Assert.Equal("", message);
            Assert.NotNull(result);
            Assert.Equal(2, ((FaceCaptureVM)result.Result).Captures.Count);
        }

        [Fact]
        public async Task GetList_NoResult_ReturnsEmptyList()
        {
            var user = new User
            {
                UserId = "1",
                FullName = "Tester",
                AvatarUrl = "avatar.jpg",
                Email = "tester@email.com"
            };
            _context.Users.Add(user);
            _context.Exams.Add(new Exam { ExamId = "1", Title = "Test Exam", CreateUser = "1", RoomId = "1" });

            var studentExam = new StudentExam { StudentExamId = "se1", ExamId = "1", StudentId = "1" };
            _context.StudentExams.Add(studentExam);
            await _context.SaveChangesAsync();
            var input = new FaceCaptureSearchVM { StudentExamId = "se1", ExamId = "1", CurrentPage = 1, PageSize = 10 };
            var (message, result) = await _service.GetList(input);
            Assert.Equal("No captures found for this student exam.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task Create_NoResult_ReturnsEmptyList()
        {
            var user = new User
            {
                UserId = "1",
                FullName = "Tester",
                AvatarUrl = "avatar.jpg",
                Email = "tester@email.com"
            };
            _context.Users.Add(user);
            _context.Exams.Add(new Exam { ExamId = "1", Title = "Test Exam", CreateUser = "1", RoomId = "1" });

            var studentExam = new StudentExam { StudentExamId = "se1", ExamId = "1", StudentId = "1" };
            _context.StudentExams.Add(studentExam);
            await _context.SaveChangesAsync();

            var message = await _service.AddCapture(new FaceCaptureRequest
            {
                StudentExamId = "se1",
                ImageCapture = null, // Simulating no image provided
                Description = "Test capture",
            });
            Assert.Equal("Student exam not found or students not in the exam process.", message);
        }

        [Fact]
        public async Task GetOne_InvalidFormat_ReturnsError()
        {
            var (message, result) = await _service.GetOne("");
            Assert.Equal("Capture not found.", message);
            Assert.Null(result);
        }
        [Fact]
        public async Task GetOne_Valid_ReturnsCapture()
        {
            var capture = new FaceCapture { CaptureId = "c1", StudentExamId = "se1", ImageUrl = "url1", CreatedAt = DateTime.UtcNow };
            _context.FaceCaptures.Add(capture);
            await _context.SaveChangesAsync();
            var (message, result) = await _service.GetOne("c1");
            Assert.Equal("", message);
            Assert.NotNull(result);
            Assert.Equal("url1", result.ImageUrl);
        }

        [Fact]
        public async Task GetOne_NotFound_ReturnsError()
        {
            var (message, result) = await _service.GetOne("notfound");
            Assert.Equal("Capture not found.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task Delete_NotFound_ReturnsError()
        {
            var message = await _service.Delete("notfound");
            Assert.Equal("Capture not found.", message);
        }

        //[Fact]
        //public async Task Delete_ValidId_ReturnsEmptyMessage()
        //{
        //    var capture = new FaceCapture { CaptureId = "c1", StudentExamId = "se1", ImageUrl = "url1" };
        //    _context.FaceCaptures.Add(capture);
        //    await _context.SaveChangesAsync();
        //    var message = await _service.Delete("c1");
        //    Assert.Equal("", message);
        //    Assert.Empty(_context.FaceCaptures);
        //}

        //[Fact]
        //public async Task Delete_AlreadyDeleted_ReturnsError()
        //{
        //    var capture = new FaceCapture { CaptureId = "c1", StudentExamId = "se1", ImageUrl = "url1" };
        //    _context.FaceCaptures.Add(capture);
        //    await _context.SaveChangesAsync();
        //    await _service.Delete("c1");
        //    var message = await _service.Delete("c1");
        //    Assert.Equal("Capture not found.", message);
        //}
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
} 