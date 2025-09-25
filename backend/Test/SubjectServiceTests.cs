using API.Commons;
using API.Models;
using API.Services;
using API.ViewModels;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace API.Tests
{
    public class SubjectServiceTests : IDisposable
    {
        private readonly Sep490Context _context;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILog> _mockLogger;
        private readonly SubjectService _service;

        public SubjectServiceTests()
        {
            var options = new DbContextOptionsBuilder<Sep490Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new Sep490Context(options);
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILog>();
            _mockLogger.Setup(l => l.WriteActivity(It.IsAny<AddUserLogVM>())).ReturnsAsync("");
            _service = new SubjectService(_context, _mockMapper.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllSubjects_NoSubjectsFound_ReturnsErrorMessage()
        {
            var search = new SearchSubjectVM { CurrentPage = 1, PageSize = 10 };
            var (message, result) = await _service.GetAllSubjects(search);
            Assert.Equal("No subjects found.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllSubjects_SubjectsFound_ReturnsNoError()
        {
            var subject = new Subject { SubjectId = "1", SubjectCode = "SUB123", SubjectName = "Test Subject" };
            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            var search = new SearchSubjectVM { CurrentPage = 1, PageSize = 10 };
            var (message, result) = await _service.GetAllSubjects(search);
            Assert.Equal("", message);
            Assert.NotNull(result);
        }


        [Fact]
        public async Task GetAllSubjects_SubjectsNotFoundPage2_ReturnsMessageError()
        {
            var subject = new Subject { SubjectId = "1", SubjectCode = "SUB123", SubjectName = "Test Subject" };
            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            var search = new SearchSubjectVM { CurrentPage = 5, PageSize = 10 };
            var (message, result) = await _service.GetAllSubjects(search);
            Assert.Equal("", message);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetSubjectById_NullId_ReturnsErrorMessage()
        {
            var (message, result) = await _service.GetSubjectById(null);
            Assert.Equal("Subject ID cannot be null or empty.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetSubjectById_NotFound_ReturnsErrorMessage()
        {
            var (message, result) = await _service.GetSubjectById("1");
            Assert.Equal("Subject not found.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task ChangeActivateSubject_NullId_ReturnsErrorMessage()
        {
            var message = await _service.ChangeActivateSubject(null, "token");
            Assert.Equal("Subject ID cannot be null or empty.", message);
        }

        [Fact]
        public async Task ChangeActivateSubject_NotFound_ReturnsErrorMessage()
        {
            var message = await _service.ChangeActivateSubject("1", "token");
            Assert.Equal("Subject not found.", message);
        }

        [Fact]
        public async Task ChangeActivateSubject_Found_ReturnsNoMessage()
        {
            var subject = new Subject { SubjectId = "1", SubjectCode = "SUB123", SubjectName = "Test Subject" };
            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            var message = await _service.ChangeActivateSubject("1", "token");
            Assert.Equal("", message);
        }

        [Fact]
        public async Task CreateUpdateSubject_NullInput_ReturnsErrorMessage()
        {
            var message = await _service.CreateUpdateSubject(null, "token");
            Assert.Equal("Subject cannot be null.", message);
        }

        [Fact]
        public void CreateUpdateSubjectVM_Should_HaveValidationErrors_When_RequiredFieldsMissing()
        {
            var model = new CreateUpdateSubjectVM
            {
                SubjectName = null,
                SubjectCode = null,
                Credits = 3
            };

            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(model, context, results, true);

            Assert.False(isValid);
            Assert.Contains(results, r => r.ErrorMessage.Contains("SubjectName"));
            Assert.Contains(results, r => r.ErrorMessage.Contains("SubjectCode"));
        }


        [Fact]
        public async Task CreateUpdateSubject_DuplicateSubjectCode_ReturnsErrorMessage()
        {
            var subject = new Subject { SubjectId = "1", SubjectCode = "SUB123", SubjectName = "Test Subject" };
            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            var input = new CreateUpdateSubjectVM
            {
                SubjectName = "Another Subject",
                SubjectCode = "SUB123",
                Credits = 3
            };
            var message = await _service.CreateUpdateSubject(input, "token");
            Assert.Contains("already in use", message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CreateUpdateSubject_DuplicateSubjectName_ReturnsErrorMessage()
        {
            var subject = new Subject { SubjectId = "1", SubjectCode = "SUB123", SubjectName = "Test Subject" };
            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            var input = new CreateUpdateSubjectVM
            {
                SubjectName = "Test Subject",
                SubjectCode = "SUB100",
                Credits = 3
            };
            var message = await _service.CreateUpdateSubject(input, "token");
            Assert.Contains("already in use", message, StringComparison.OrdinalIgnoreCase);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
