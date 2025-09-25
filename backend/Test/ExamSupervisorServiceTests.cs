using API.Commons;
using API.Helper;
using API.Models;
using API.Services;
using API.Services.Interfaces;
using API.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Xunit;

namespace API.Tests
{
    public class ExamSupervisorServiceTests : IDisposable
    {
        private readonly Sep490Context _context;
        private readonly Mock<ILog> _mockLogger;
        private readonly ExamSupervisorService _service;

        public ExamSupervisorServiceTests()
        {
            var options = new DbContextOptionsBuilder<Sep490Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            _context = new Sep490Context(options);
            _mockLogger = new Mock<ILog>();
            _mockLogger.Setup(x => x.WriteActivity(It.IsAny<AddUserLogVM>())).ReturnsAsync("");
            _service = new ExamSupervisorService(_context, _mockLogger.Object);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task GetAll_WithSupervisors_ReturnsList()
        {
            // Arrange
            var department = new Department { Id = "dep1", Name = "Dept A", Code = "1" };
            var major = new Major { Id = "maj1", Name = "Major A", Code = "1" };
            var specialization = new Specialization { Id = "spec1", Name = "Spec A", Status = true };

            _context.Departments.Add(department);
            _context.Majors.Add(major);
            _context.Specializations.Add(specialization);
            var user = new User { UserId = "user1", FullName = "Test User", Email = "test@email.com", UserCode = "TU001", SpecializationId = "spec1", MajorId = "maj1", DepartmentId = "dep1" };
            var role = new Role { Id = 2, Name = "Lecture" };
            var userRole = new UserRole { UserId = "user1", RoleId = 4 };
            var subject = new Subject { SubjectId = "sub1", SubjectName = "Test Subject", SubjectCode = "S001" };
            var class1 = new Class { ClassId = "class1", ClassCode = "C001", CreatedBy = "admin" };
            var room = new Room { RoomId = "1", SubjectId = "sub1", ClassId = "class1", RoomCode = "R001", Capacity = 30 };
            _context.Subjects.Add(subject);
            _context.Classes.Add(class1);
            _context.Rooms.Add(room);

            user.Department = department;
            user.Major = major;
            user.Specialization = specialization;

            var exam = new Exam { ExamId = "exam1", Title = "Test Exam", CreateUser = "1", RoomId = "1" };

            var examSupervisor = new ExamSupervisor
            {
                ExamSupervisorId = "es1",
                ExamId = "exam1",
                SupervisorId = "user1",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "user1"
            };

            _context.Exams.Add(exam);
            _context.Users.Add(user);
            _context.Roles.Add(role);
            _context.UserRoles.Add(userRole);
            _context.ExamSupervisors.Add(examSupervisor);
            await _context.SaveChangesAsync();

            // Act
            var (message, result) = await _service.GetAll("exam1", "user1");

            // Assert
            Assert.Empty(message);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetAll_NoSupervisors_ReturnsEmptyMessage()
        {
            // Act
            var (message, result) = await _service.GetAll("nonexistent", "token");

            // Assert
            Assert.Equal("You do not have permission to view this exam's supervisors.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetSupervisors_WithUsers_ReturnsList()
        {
            // Arrange
            var user = new User { UserId = "user1", FullName = "Test User", Email = "test@email.com", UserCode = "TU001", Status = (int)UserStatus.Active };
            var role = new Role { Id = 2, Name = "Lecture" };
            var userRole = new UserRole { UserId = "user1", RoleId = 2 };

            _context.Users.Add(user);
            _context.Roles.Add(role);
            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            var search = new SearchRequestVM { CurrentPage = 1, PageSize = 10 };

            // Act
            var (message, result) = await _service.GetSupervisors(search);

            // Assert
            Assert.Empty(message);
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalPage);
            Assert.Equal(1, result.Total);
        }

        [Fact]
        public async Task GetSupervisors_WithTextSearch_ReturnsFilteredResults()
        {
            // Arrange
            var user1 = new User { UserId = "user1", FullName = "Test User", Email = "test@email.com", UserCode = "TU001", Status = (int)UserStatus.Active };
            var user2 = new User { UserId = "user2", FullName = "Another User", Email = "another@email.com", UserCode = "AU001", Status = (int)UserStatus.Active };
            var role = new Role { Id = 2, Name = "Lecture" };
            var userRole1 = new UserRole { UserId = "user1", RoleId = 2 };
            var userRole2 = new UserRole { UserId = "user2", RoleId = 2 };

            _context.Users.AddRange(user1, user2);
            _context.Roles.Add(role);
            _context.UserRoles.AddRange(userRole1, userRole2);
            await _context.SaveChangesAsync();

            var search = new SearchRequestVM { CurrentPage = 1, PageSize = 10, TextSearch = "Test" };

            // Act
            var (message, result) = await _service.GetSupervisors(search);

            // Assert
            Assert.Empty(message);
            Assert.NotNull(result);
            Assert.Equal(1, result.Total);
        }

        [Fact]
        public async Task GetExams_WithExams_ReturnsList()
        {
            // Arrange
            var user = new User { UserId = "admin", FullName = "Admin", Email = "admin@email.com", UserCode = "AD001" };
            var role = new Role { Id = 1, Name = "Admin" };
            var userRole = new UserRole { UserId = "admin", RoleId = 1 };
            var subject = new Subject { SubjectId = "sub1", SubjectName = "Test Subject", SubjectCode = "1" };
            var class1 = new Class { ClassId = "class1", ClassCode = "C001", CreatedBy = "admin" };
            var room = new Room { RoomId = "room1", RoomCode = "R001", SubjectId = "sub1", ClassId = "class1" };
            var exam = new Exam
            {
                ExamId = "exam1",
                Title = "Test Exam",
                Status = (int)ExamStatus.Published,
                CreateUser = "admin",
                RoomId = "room1",
                CreatedAt = DateTime.UtcNow,
                StartTime = DateTime.UtcNow.AddHours(-1),  // exam đang diễn ra
                EndTime = DateTime.UtcNow.AddHours(1)
            };
            var roomUser = new RoomUser
            {
                RoomUserId = "ru1",
                RoomId = "room1",
                UserId = "admin",
                UpdatedAt = DateTime.UtcNow,
            };

            _context.Users.Add(user);
            _context.Roles.Add(role);
            _context.UserRoles.Add(userRole);
            _context.Subjects.Add(subject);
            _context.Classes.Add(class1);
            _context.Rooms.Add(room);
            _context.RoomUsers.Add(roomUser);
            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();

            var search = new SearchRequestVM { CurrentPage = 1, PageSize = 10 };

            // Act
            var (message, result) = await _service.GetExams(search, "admin");

            // Assert
            Assert.Empty(message);
            Assert.NotNull(result);
            Assert.Equal(1, result.Total);
        }

        [Fact]
        public async Task AssignSupervisor_ValidInput_ReturnsSuccess()
        {
            // Arrange
            var exam = new Exam { ExamId = "exam1", Title = "Test Exam", CreateUser = "1", RoomId = "1" };
            var user = new User { UserId = "user1", FullName = "Test User", Email = "test@email.com" };
            var role = new Role { Id = 2, Name = "Lecture" };
            var userRole = new UserRole { UserId = "user1", RoleId = 2 };
            var examSupervisor = new ExamSupervisor
            {
                ExamSupervisorId = "es1",
                ExamId = "exam1",
                SupervisorId = "existing",
                CreatedBy = "admin",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Exams.Add(exam);
            _context.Users.Add(user);
            _context.Roles.Add(role);
            _context.UserRoles.Add(userRole);
            _context.ExamSupervisors.Add(examSupervisor);
            await _context.SaveChangesAsync();

            var input = new EditExamSupervisorVM
            {
                ExamId = "exam1",
                SupervisorId = new List<string> { "user1" },
                Note = "Test note"
            };

            // Act
            var (message, result) = await _service.AssignSupervisor(input, "admin");

            // Assert
            Assert.Empty(message);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task AssignSupervisor_NoExamSupervisor_ReturnsError()
        {
            // Arrange
            var input = new EditExamSupervisorVM
            {
                ExamId = "nonexistent",
                SupervisorId = new List<string> { "user1" }
            };

            // Act
            var (message, result) = await _service.AssignSupervisor(input, "admin");

            // Assert
            Assert.Equal("No ExamSupervisor found for this exam.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task AssignSupervisor_StudentRole_ReturnsError()
        {
            // Arrange
            var exam = new Exam { ExamId = "exam1", Title = "Test Exam", CreateUser = "1", RoomId = "1" };
            var user = new User { UserId = "user1", FullName = "Test User", Email = "test@email.com" };
            var role = new Role { Id = 3, Name = "Student" };
            var userRole = new UserRole { UserId = "user1", RoleId = (int)RoleEnum.Student };
            var examSupervisor = new ExamSupervisor
            {
                ExamSupervisorId = "es1",
                ExamId = "exam1",
                SupervisorId = "existing",
                CreatedBy = "admin",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Exams.Add(exam);
            _context.Users.Add(user);
            _context.Roles.Add(role);
            _context.UserRoles.Add(userRole);
            _context.ExamSupervisors.Add(examSupervisor);
            await _context.SaveChangesAsync();

            var input = new EditExamSupervisorVM
            {
                ExamId = "exam1",
                SupervisorId = new List<string> { "user1" }
            };

            // Act
            var (message, result) = await _service.AssignSupervisor(input, "admin");

            // Assert
            Assert.Contains("have Student role", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task Remove_ValidInput_ReturnsSuccess()
        {
            // Arrange
            var exam = new Exam { ExamId = "exam1", Title = "Test Exam", CreateUser = "admin", RoomId = "room1" };
            _context.Exams.Add(exam);
            var user = new User { UserId = "admin", FullName = "Admin", Email = "admin@email.com" };
            var role = new Role { Id = 1, Name = "Admin" };
            var userRole = new UserRole { UserId = "admin", RoleId = 1 };
            var examSupervisor = new ExamSupervisor
            {
                ExamSupervisorId = "es1",
                ExamId = "exam1",
                SupervisorId = "user1",
                CreatedBy = "admin",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.Roles.Add(role);
            _context.UserRoles.Add(userRole);
            _context.ExamSupervisors.Add(examSupervisor);
            await _context.SaveChangesAsync();

            var input = new EditExamSupervisorVM
            {
                ExamId = "exam1",
                SupervisorId = new List<string> { "user1" }
            };

            // Act
            var (message, result) = await _service.Remove(input, "admin");

            // Assert
            Assert.Empty(message);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Remove_NoSupervisors_ReturnsError()
        {
            var exam = new Exam
            {
                ExamId = "nonexistent",
                Title = "Test Exam",
                CreateUser = "admin",
                RoomId = "room1"
            };
            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();
            // Arrange
            var input = new EditExamSupervisorVM
            {
                ExamId = "nonexistent",
                SupervisorId = new List<string> { "es1" }
            };

            // Act
            var (message, result) = await _service.Remove(input, "admin");

            // Assert
            Assert.Equal("No supervisors found to delete.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task Remove_NoMatchingSupervisors_ReturnsError()
        {
            // Arrange
            var user = new User { UserId = "admin", FullName = "Admin", Email = "admin@email.com" };
            var role = new Role { Id = 1, Name = "Admin" };
            var userRole = new UserRole { UserId = "admin", RoleId = 1 };
            var exam = new Exam
            {
                ExamId = "exam1",
                Title = "Test Exam",
                CreateUser = "admin",
                RoomId = "room1"
            };
            _context.Exams.Add(exam);

            var examSupervisor = new ExamSupervisor
            {
                ExamSupervisorId = "es1",
                ExamId = "exam1",
                SupervisorId = "user1",
                CreatedBy = "admin",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.Roles.Add(role);
            _context.UserRoles.Add(userRole);
            _context.ExamSupervisors.Add(examSupervisor);
            await _context.SaveChangesAsync();

            var input = new EditExamSupervisorVM
            {
                ExamId = "exam1",
                SupervisorId = new List<string> { "nonexistent" }
            };

            // Act
            var (message, result) = await _service.Remove(input, "admin");

            // Assert
            Assert.Equal("No matching supervisor id(s) found to delete.", message);
            Assert.NotNull(result);
        }
    }
}