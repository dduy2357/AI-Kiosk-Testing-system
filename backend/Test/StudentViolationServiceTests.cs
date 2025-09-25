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
    public class StudentViolationServiceTests : IDisposable
    {
        private readonly Sep490Context _context;
        private readonly Mock<ILog> _mockLogger;
        private readonly Mock<IAmazonS3Service> _mockS3;
        private readonly StudentViolationService _service;

        public StudentViolationServiceTests()
        {
            var options = new DbContextOptionsBuilder<Sep490Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new Sep490Context(options);
            _mockLogger = new Mock<ILog>();
            _mockS3 = new Mock<IAmazonS3Service>();
            // Setup logger trả về chuỗi rỗng cho mọi WriteActivity
            _mockLogger.Setup(l => l.WriteActivity(It.IsAny<AddUserLogVM>())).ReturnsAsync("");
            _service = new StudentViolationService(_context, _mockLogger.Object, _mockS3.Object);
        }

        [Fact]
        public async Task GetAll_StudentExamIdEmpty_ReturnsError()
        {
            var search = new SearchStudentViolation { StudentExamId = null, CurrentPage = 1, PageSize = 10 };
            var (message, result) = await _service.GetAll(search);
            Assert.Equal("ExamId is required.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAll_NoValid_ReturnsError()
        {
            var search = new SearchStudentViolation { StudentExamId = "notfound", CurrentPage = 1, PageSize = 10 };
            var (message, result) = await _service.GetAll(search);
            Assert.Equal("ExamId is required.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetById_NotFound_ReturnsError()
        {
            var (message, result) = await _service.GetById("notfound");
            Assert.Equal("No student violation found", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task Create_NullInput_ReturnsError()
        {
            var message = await _service.Create(null, "token");
            Assert.Equal("Student violation data is null", message);
        }

        [Fact]
        public async Task Delete_IdEmpty_ReturnsError()
        {
            var message = await _service.Delete(null, "token");
            Assert.Equal("Violation ID is required.", message);
        }

        [Fact]
        public async Task GetAll_HasViolations_ReturnsList()
        {
            var user = new User { UserId = "u1", FullName = "User 1", UserCode = "U1", Email = "u1@email.com" };
            var exam = new Exam { ExamId = "e1", Title = "Exam 1", CreateUser = "u1", RoomId = "1" };
            var studentExam = new StudentExam { StudentExamId = "se1", User = user, Exam = exam };
            var violation = new StudentViolation { Id = "v1", CreatedBy = "1", StudentExamId = "se1", StudentExam = studentExam, ViolationName = "Cheating", Message = "msg", CreatedAt = DateTime.UtcNow };
            _context.Users.Add(user);
            _context.Exams.Add(exam);
            _context.StudentExams.Add(studentExam);
            _context.StudentViolations.Add(violation);
            await _context.SaveChangesAsync();
            var search = new SearchStudentViolation { StudentExamId = "se1", ExamId = "e1", CurrentPage = 1, PageSize = 10 };
            var (message, result) = await _service.GetAll(search);
            Assert.Equal("", message);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetAll_NoViolationPage2_ReturnsList()
        {
            var user = new User { UserId = "u1", FullName = "User 1", UserCode = "U1", Email = "u1@email.com" };
            var exam = new Exam { ExamId = "e1", Title = "Exam 1", CreateUser = "u1", RoomId = "1" };
            var studentExam = new StudentExam { StudentExamId = "se1", User = user, Exam = exam };
            var violation = new StudentViolation { Id = "v1", CreatedBy = "1", StudentExamId = "se1", StudentExam = studentExam, ViolationName = "Cheating", Message = "msg", CreatedAt = DateTime.UtcNow };
            _context.Users.Add(user);
            _context.Exams.Add(exam);
            _context.StudentExams.Add(studentExam);
            _context.StudentViolations.Add(violation);
            await _context.SaveChangesAsync();
            var search = new SearchStudentViolation { StudentExamId = "se1", ExamId = "e1", CurrentPage = 2, PageSize = 10 };
            var (message, result) = await _service.GetAll(search);
            Assert.Equal("", message);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetById_Valid_ReturnsViolation()
        {
            var user = new User { UserId = "u1", FullName = "User 1", UserCode = "U1", Email = "u1@email.com" };
            var exam = new Exam { ExamId = "e1", Title = "Exam 1", CreateUser = "u1", RoomId = "1" };
            var studentExam = new StudentExam { StudentExamId = "se1", User = user, Exam = exam };
            var violation = new StudentViolation { Id = "v1", StudentExamId = "se1", CreatedBy = "u1", StudentExam = studentExam, ViolationName = "Cheating", Message = "msg", CreatedAt = DateTime.UtcNow };
            _context.Users.Add(user);
            _context.Exams.Add(exam);
            _context.StudentExams.Add(studentExam);
            _context.StudentViolations.Add(violation);
            await _context.SaveChangesAsync();
            var (message, result) = await _service.GetById("v1");
            Assert.Equal("", message);
            Assert.NotNull(result);
            Assert.Equal("Cheating", result.ViolationName);
        }

        [Fact]
        public async Task Create_StudentExamNotFound_ReturnsError()
        {
            var send = new SendStudentViolationVM { StudentExamId = "notfound", Message = "msg", ViolateName = "Cheating" };
            var message = await _service.Create(send, "token");
            Assert.Equal("Student exam not found", message);
        }

        [Fact]
        public async Task Create_StudentNoEmail_ReturnsError()
        {
            var user = new User { UserId = "u1", FullName = "User 1", UserCode = "U1", Email = null };
            var exam = new Exam { ExamId = "e1", Title = "Exam 1", CreateUser = "1", RoomId = "1" };
            var studentExam = new StudentExam { StudentExamId = "se1", User = user, Exam = exam };
            _context.Users.Add(user);
            _context.Exams.Add(exam);
            _context.StudentExams.Add(studentExam);
            await _context.SaveChangesAsync();
            var send = new SendStudentViolationVM { StudentExamId = "se1", Message = "msg", ViolateName = "Cheating", IsSendMail = true };
            var message = await _service.Create(send, "token");
            Assert.Equal("Student email not found.", message);
        }

        [Fact]
        public async Task Create_Success_ReturnsEmptyMessage()
        {
            var user = new User { UserId = "u1", FullName = "User 1", UserCode = "U1", Email = "u1@email.com" };
            var exam = new Exam { ExamId = "e1", Title = "Exam 1", CreateUser = "u1", RoomId = "1" };
            var studentExam = new StudentExam { StudentExamId = "se1", User = user, Exam = exam };
            _context.Users.Add(user);
            _context.Exams.Add(exam);
            _context.StudentExams.Add(studentExam);
            await _context.SaveChangesAsync();
            var send = new SendStudentViolationVM { StudentExamId = "se1", Message = "msg", ViolateName = "Cheating", IsSendMail = false };
            var message = await _service.Create(send, "token");
            Assert.Equal("", message);
            Assert.Single(_context.StudentViolations);
        }

        [Fact]
        public async Task Delete_NotFound_ReturnsError()
        {
            var message = await _service.Delete("notfound", "token");
            Assert.Equal("Violation not found.", message);
        }

        [Fact]
        public async Task Delete_Success_ReturnsEmptyMessage()
        {
            var violation = new StudentViolation { Id = "v1", CreatedBy = "1", StudentExamId = "se1", Message = "msg", ViolationName = "Cheating" };
            _context.StudentViolations.Add(violation);
            await _context.SaveChangesAsync();
            var message = await _service.Delete("v1", "token");
            Assert.Equal("", message);
            Assert.Empty(_context.StudentViolations);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}