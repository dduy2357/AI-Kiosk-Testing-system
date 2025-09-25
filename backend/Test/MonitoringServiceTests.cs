using API.Commons;
using API.Factory;
using API.Helper;
using API.Hubs;
using API.Models;
using API.Repository;
using API.Repository.Interface;
using API.Services;
using API.Services.Interfaces;
using API.Subjects;
using API.ViewModels;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Xunit;

namespace API.Tests
{
    public class MonitoringServiceTests : IDisposable
    {
        private readonly Sep490Context _context;
        private readonly Mock<ILog> _mockLogger;
        private readonly Mock<IAmazonS3Service> _s3Service;
        private readonly Mock<IHubContext<ExamHub>> _mockHubContext;
        private readonly MonitoringService _service;
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IMonitoringSubject> _monitorSubject;
        private readonly Mock<IScoringStrategyFactory> _factory;
        private readonly Mock<IMonitoringSubject> _subject;

        public MonitoringServiceTests()
        {
            var options = new DbContextOptionsBuilder<Sep490Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new Sep490Context(options);
            _mockLogger = new Mock<ILog>();
            _subject = new Mock<IMonitoringSubject>();
            _s3Service = new Mock<IAmazonS3Service>();
            _unitOfWork = new Mock<IUnitOfWork>();
            _monitorSubject = new Mock<IMonitoringSubject>();
            _factory = new Mock<IScoringStrategyFactory>();

            // -----------------------------
            // Setup HubContext here:

            var mockClientProxy = new Mock<IClientProxy>();
            mockClientProxy
                .Setup(x => x.SendCoreAsync(
                    It.IsAny<string>(),
                    It.IsAny<object[]>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var mockClients = new Mock<IHubClients>();
            mockClients
                .Setup(c => c.Group(It.IsAny<string>()))
                .Returns(mockClientProxy.Object);

            mockClients
                .Setup(c => c.All)
                .Returns(mockClientProxy.Object);

            mockClients
                .Setup(c => c.User(It.IsAny<string>()))
                .Returns(mockClientProxy.Object);

            _mockHubContext = new Mock<IHubContext<ExamHub>>();
            _mockHubContext
                .Setup(h => h.Clients)
                .Returns(mockClients.Object);
            // -----------------------------

            _mockLogger.Setup(x => x.WriteActivity(It.IsAny<AddUserLogVM>())).ReturnsAsync("");

            _service = new MonitoringService(_context, _mockLogger.Object, _mockHubContext.Object, _s3Service.Object, _unitOfWork.Object, _monitorSubject.Object, _factory.Object);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task GetExamOverview_WithExams_ReturnsList()
        {
            // Arrange
            var user = new User { UserId = "admin", FullName = "Admin", Email = "admin@email.com" };
            var role = new Role { Id = 1, Name = "Admin" };
            var userRole = new UserRole { UserId = "admin", RoleId = 1 };
            var subject = new Subject { SubjectId = "sub1", SubjectName = "Test Subject", SubjectCode = "1" };
            var class1 = new Class { ClassId = "class1", ClassCode = "C001", CreatedBy = "admin" };
            var room = new Room { RoomId = "room1", RoomCode = "R001", SubjectId = "sub1", ClassId = "class1" };
            var roomUser = new RoomUser
            {
                RoomUserId = "ru1",
                UserId = "admin",
                RoomId = "room1",
                RoleId = (int)RoleEnum.Student,
                Status = 1
            };
            var exam = new Exam
            {
                ExamId = "exam1",
                Title = "Test Exam",
                Status = (int)ExamStatus.Published,
                RoomId = "room1",
                StartTime = DateTime.UtcNow.AddHours(-1),
                EndTime = DateTime.UtcNow.AddHours(1),
                Duration = 60,
                Creator = user
            };
            var examSupervisor = new ExamSupervisor
            {
                ExamSupervisorId = "es1",
                ExamId = "exam1",
                SupervisorId = "admin",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "admin"
            };
            var studentExam = new StudentExam
            {
                StudentExamId = "se1",
                ExamId = "exam1",
                StudentId = "admin",
                Status = (int)StudentExamStatus.InProgress,
                StartTime = DateTime.UtcNow.AddMinutes(-30)
            };
            _context.StudentExams.Add(studentExam);

            _context.Users.Add(user);
            _context.Roles.Add(role);
            _context.UserRoles.Add(userRole);
            _context.Subjects.Add(subject);
            _context.Classes.Add(class1);
            _context.Rooms.Add(room);
            _context.RoomUsers.Add(roomUser);
            _context.Exams.Add(exam);
            _context.ExamSupervisors.Add(examSupervisor);
            await _context.SaveChangesAsync();

            var search = new MonitorExamSearchVM { CurrentPage = 1, PageSize = 10 };

            // Act
            var (message, result) = await _service.GetExamOverview(search, "admin");

            // Assert
            Assert.Empty(message);
            Assert.NotNull(result);
            Assert.Equal(1, result.Total);
        }

        [Fact]
        public async Task GetExamOverview_NoPermission_ReturnsError()
        {
            // Arrange
            var search = new MonitorExamSearchVM { CurrentPage = 1, PageSize = 10 };

            // Act
            var (message, result) = await _service.GetExamOverview(search, "user1");

            // Assert
            Assert.Equal("You do not have the right to supervise any exams.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetExamOverview_WithTextSearch_ReturnsFilteredResults()
        {
            // Arrange
            var user = new User { UserId = "admin", FullName = "Admin", Email = "admin@email.com" };
            var role = new Role { Id = 1, Name = "Admin" };
            var userRole = new UserRole { UserId = "admin", RoleId = 1 };
            var subject = new Subject { SubjectId = "sub1", SubjectName = "Test Subject", SubjectCode = "1" };
            var class1 = new Class { ClassId = "class1", ClassCode = "C001", CreatedBy = "admin" };
            var room = new Room { RoomId = "room1", RoomCode = "R001", SubjectId = "sub1", ClassId = "class1" };
            var roomUser = new RoomUser
            {
                RoomUserId = "ru1",
                UserId = "admin",
                RoomId = "room1",
                RoleId = (int)RoleEnum.Student,
                Status = 1
            };
            var exam = new Exam
            {
                ExamId = "exam1",
                Title = "Test Exam",
                Status = (int)ExamStatus.Published,
                RoomId = "room1",
                StartTime = DateTime.UtcNow.AddHours(-1),
                EndTime = DateTime.UtcNow.AddHours(1),
                Duration = 60,
                Creator = user
            };
            var examSupervisor = new ExamSupervisor
            {
                ExamSupervisorId = "es1",
                ExamId = "exam1",
                SupervisorId = "admin",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "admin"
            };
            var studentExam = new StudentExam
            {
                StudentExamId = "se1",
                ExamId = "exam1",
                StudentId = "admin",
                Status = (int)StudentExamStatus.InProgress,
                StartTime = DateTime.UtcNow.AddMinutes(-30)
            };
            _context.StudentExams.Add(studentExam);

            _context.Users.Add(user);
            _context.Roles.Add(role);
            _context.UserRoles.Add(userRole);
            _context.Subjects.Add(subject);
            _context.Classes.Add(class1);
            _context.Rooms.Add(room);
            _context.RoomUsers.Add(roomUser);
            _context.Exams.Add(exam);
            _context.ExamSupervisors.Add(examSupervisor);
            await _context.SaveChangesAsync();

            var search = new MonitorExamSearchVM { CurrentPage = 1, PageSize = 10, TextSearch = "Test" };

            // Act
            var (message, result) = await _service.GetExamOverview(search, "admin");

            // Assert
            Assert.Empty(message);
            Assert.NotNull(result);
            Assert.Equal(1, result.Total);
        }

        [Fact]
        public async Task GetExamMonitorDetail_WithPermission_ReturnsList()
        {
            // Arrange
            var user = new User { UserId = "admin", FullName = "Admin", Email = "admin@email.com" };
            var role = new Role { Id = 1, Name = "Admin" };
            var userRole = new UserRole { UserId = "admin", RoleId = 1 };
            var subject = new Subject { SubjectId = "sub1", SubjectCode = "1", SubjectName = "Test Subject" };
            var class1 = new Class { ClassId = "class1", ClassCode = "C001", CreatedBy = "admin" };
            var room = new Room { RoomId = "room1", RoomCode = "R001", SubjectId = "sub1", ClassId = "class1" };
            var exam = new Exam
            {
                ExamId = "exam1",
                Title = "Test Exam",
                Status = (int)ExamStatus.Published,
                RoomId = "room1",
                TotalQuestions = 10,
                CreateUser = "admin"
            };
            var studentExam = new StudentExam
            {
                StudentExamId = "se1",
                ExamId = "exam1",
                StudentId = "student1",
                Status = (int)StudentExamStatus.InProgress,
                StartTime = DateTime.UtcNow.AddMinutes(-30),
                User = user
            };
            var examSupervisor = new ExamSupervisor
            {
                ExamSupervisorId = "es1",
                ExamId = "exam1",
                SupervisorId = "admin",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "admin",
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.Roles.Add(role);
            _context.UserRoles.Add(userRole);
            _context.Subjects.Add(subject);
            _context.Classes.Add(class1);
            _context.Rooms.Add(room);
            _context.Exams.Add(exam);
            _context.StudentExams.Add(studentExam);
            _context.ExamSupervisors.Add(examSupervisor);
            await _context.SaveChangesAsync();

            var search = new MonitorExamDetailSearchVM { ExamId = "exam1", CurrentPage = 1, PageSize = 10 };

            // Act
            var (message, result) = await _service.GetExamMonitorDetail(search, "admin");

            // Assert
            Assert.Empty(message);
            Assert.NotNull(result);
            Assert.Equal(1, result.Total);
        }

        [Fact]
        public async Task GetExamMonitorDetail_NoPermission_ReturnsError()
        {
            // Arrange
            var search = new MonitorExamDetailSearchVM { ExamId = "exam1", CurrentPage = 1, PageSize = 10 };

            // Act
            var (message, result) = await _service.GetExamMonitorDetail(search, "user1");

            // Assert
            Assert.Equal("You do not have permission to view this exam.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task AddStudentExtraTime_ValidStudent_ReturnsSuccess()
        {
            // Arrange
            var exam = new Exam { ExamId = "exam1", Duration = 60 };
            var studentExam = new StudentExam
            {
                StudentExamId = "se1",
                StudentId = "student1",
                Exam = exam,
                StartTime = DateTime.UtcNow.AddMinutes(-30),
                ExtraTimeMinutes = 0,
                Status = (int)StudentExamStatus.InProgress
            };

            var timeRequest = new StudentExamExtraTime
            {
                StudentExamId = "se1",
                ExtraMinutes = 10
            };

            var studentExamRepo = new Mock<IStudentExamRepository>();
            studentExamRepo.Setup(r => r.GetStudentExamWithExamUser("se1", StudentExamStatus.InProgress, false))
                           .ReturnsAsync(studentExam);

            _unitOfWork.Setup(u => u.StudentExams).Returns(studentExamRepo.Object);
            _unitOfWork.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(Mock.Of<IDbContextTransaction>());
            _unitOfWork.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            _subject.Setup(s => s.Notify(timeRequest, studentExam, "student1")).Returns(Task.CompletedTask);

            // Act
            var result = await _service.AddStudentExtraTime(timeRequest, "student1");

            // Assert
            Assert.Equal("", result);
            Assert.Equal(10, studentExam.ExtraTimeMinutes);
            Assert.NotNull(studentExam.SubmitTime);
        }


        [Fact]
        public async Task AddStudentExtraTime_StudentExamNotFound_ReturnsError()
        {
            // Arrange
            var request = new StudentExamExtraTime
            {
                StudentExamId = "nonexistent",
                ExtraMinutes = 15
            };

            var studentExamRepo = new Mock<IStudentExamRepository>();
            studentExamRepo.Setup(r => r.GetStudentExamWithExamUser("nonexistent", StudentExamStatus.InProgress, false))
                           .ReturnsAsync((StudentExam?)null);

            _unitOfWork.Setup(u => u.StudentExams).Returns(studentExamRepo.Object);
            _unitOfWork.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(Mock.Of<IDbContextTransaction>());
            _unitOfWork.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _service.AddStudentExtraTime(request, "admin");

            // Assert
            Assert.Equal("Student is not taking in exam.", result);
        }

        [Fact]
        public async Task AddExamExtraTime_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var user = new User { UserId = "student1", FullName = "Test Student", Email = "student@email.com" };
            var room = new Room { RoomId = "room1", RoomCode = "R001", SubjectId = "sub1", ClassId = "class1" };
            var roomUser = new RoomUser
            {
                RoomUserId = "ru1",
                UserId = "student1",
                RoomId = "room1",
                RoleId = (int)RoleEnum.Student,
                Status = 1
            };
            var exam = new Exam { ExamId = "exam1", Title = "Test Exam", Duration = 60, RoomId = "room1", CreateUser = "1" };

            var now = DateTime.UtcNow;

            var studentExam = new StudentExam
            {
                StudentExamId = "se1",
                ExamId = "exam1",
                StudentId = "student1",
                Status = (int)StudentExamStatus.InProgress,
                StartTime = now.AddMinutes(10),            // StartTime in future
                SubmitTime = now.AddMinutes(-5),           // SubmitTime in past
                User = user,
                Exam = exam,
            };

            _context.Users.Add(user);
            _context.Exams.Add(exam);
            _context.Rooms.Add(room);
            _context.RoomUsers.Add(roomUser);
            _context.StudentExams.Add(studentExam);
            await _context.SaveChangesAsync();

            var request = new ExamExtraTime
            {
                ExamId = "exam1",
                RoomId = "room1",
                ExtraMinutes = 15
            };

            // Act
            var result = await _service.AddExamExtraTime(request, "admin");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task AddExamExtraTime_NoStudents_ReturnsError()
        {
            // Arrange
            var request = new ExamExtraTime
            {
                ExamId = "exam1",
                RoomId = "room1",
                ExtraMinutes = 15
            };

            // Act
            var result = await _service.AddExamExtraTime(request, "admin");

            // Assert
            Assert.Equal("No student is currently taking this exam.", result);
        }

        [Fact]
        public async Task FinishExam_ValidExam_ReturnsSuccess()
        {
            var subject = new Subject
            {
                SubjectId = "sub1",
                SubjectName = "Math",
                SubjectCode = "MTH101",
            };

            var questionBank = new QuestionBank
            {
                QuestionBankId = "qb1",
                SubjectId = "sub1",
                Status = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreateUserId = "admin",
                Description = "Test question bank",
                Title = "Test Bank",
            };

            var question = new Question
            {
                QuestionId = "q1",
                Content = "Test question",
                CorrectAnswer = "A",
                Explanation = "Explanation test",
                Options = "[\"A\",\"B\",\"C\",\"D\"]",
                QuestionBankId = "qb1",
                SubjectId = "sub1",
                CreateUser = "admin",
                UpdateUser = "admin"
            };

            var exam = new Exam
            {
                ExamId = "exam1",
                Title = "Test Exam",
                ExamType = (int)QuestionTypeChoose.MultipleChoice,
                CreateUser = "1",
                RoomId = "1"
            };

            var studentExam = new StudentExam
            {
                StudentExamId = "se1",
                ExamId = "exam1",
                StudentId = "student1",
                Status = (int)StudentExamStatus.InProgress,
                StartTime = DateTime.UtcNow.AddMinutes(-30),
                Exam = exam
            };

            var examQuestion = new ExamQuestion
            {
                ExamQuestionId = "1",
                ExamId = "exam1",
                QuestionId = "q1",
                Points = 10,
                Question = question
            };

            var studentAnswer = new StudentAnswer
            {
                StudentAnswerId = Guid.NewGuid().ToString(),
                StudentExamId = "se1",
                QuestionId = "q1",
                UserAnswer = "A"
            };

            _context.Subjects.Add(subject);
            _context.QuestionBanks.Add(questionBank);
            _context.Questions.Add(question);
            _context.Exams.Add(exam);
            _context.StudentExams.Add(studentExam);
            _context.ExamQuestions.Add(examQuestion);
            _context.StudentAnswers.Add(studentAnswer);

            await _context.SaveChangesAsync();

            // Act
            var result = await _service.FinishExam(new FinishExam
            {
                ExamId = "exam1",
            }, "admin");

            // Assert
            Assert.Empty(result);
        }

        //[Fact]
        //public async Task ReAssignSingleStudent_ValidRequest_ReturnsSuccess()
        //{
        //    // Arrange
        //    var exam = new Exam
        //    {
        //        ExamId = "exam1",
        //        Title = "Test Exam",
        //        Status = (int)ExamStatus.Published,
        //        CreateUser = "1",
        //        RoomId = "1",
        //        Duration = 60
        //    };
        //    var studentExam = new StudentExam
        //    {
        //        StudentExamId = "se1",
        //        ExamId = "exam1",
        //        StudentId = "student1",
        //        Status = (int)StudentExamStatus.InProgress,
        //        StartTime = DateTime.UtcNow.AddMinutes(-30)
        //    };
        //    var studentAnswer = new StudentAnswer
        //    {
        //        StudentAnswerId = Guid.NewGuid().ToString(),
        //        StudentExamId = "se1",
        //        QuestionId = "q1",
        //        UserAnswer = "A"
        //    };
        //    _context.Exams.Add(exam);
        //    _context.StudentExams.Add(studentExam);
        //    _context.StudentAnswers.Add(studentAnswer);
        //    await _context.SaveChangesAsync();

        //    // Act
        //    var message = await _service.ReAssignStudent(new ReAssignStudent
        //    {
        //        ExamId = "exam1",
        //        StudentId = "student1"
        //    }, "admin");
        //    // Assert
        //    Assert.Empty(message);
        //    // Kiểm tra dữ liệu đã được tạo mới
        //    var newStudentExam = await _context.StudentExams.FirstOrDefaultAsync(se => se.StudentId == "student1");
        //    Assert.NotNull(newStudentExam);
        //    Assert.NotEqual("se1", newStudentExam!.StudentExamId);
        //    Assert.True(newStudentExam.StartTime > DateTime.UtcNow.AddMinutes(-1));
        //}

        //[Fact]
        //public async Task ReAssignSingleStudent_ExamNotFound_ReturnsError()
        //{
        //    // Arrange
        //    var examId = "nonexistent";
        //    var studentId = "student1";

        //    // Act
        //    var message = await _service.ReAssignStudent(new ReAssignStudent
        //    {
        //        ExamId = "exam1",
        //        StudentId = "student1"
        //    }, "admin");
        //    Assert.Equal("Exam not found or not ongoing.", message);
        //}

        [Fact]
        public async Task GetExamOverview_WithSubjectFilter_ReturnsFilteredResults()
        {
            // Arrange
            var user = new User { UserId = "admin", FullName = "Admin", Email = "admin@email.com" };
            var role = new Role { Id = 1, Name = "Admin" };
            var userRole = new UserRole { UserId = "admin", RoleId = 1 };
            var subject = new Subject { SubjectId = "sub1", SubjectName = "Test Subject", SubjectCode = "1" };
            var class1 = new Class { ClassId = "class1", ClassCode = "C001", CreatedBy = "admin" };
            var room = new Room { RoomId = "room1", RoomCode = "R001", SubjectId = "sub1", ClassId = "class1" };
            var roomUser = new RoomUser
            {
                RoomUserId = "ru1",
                UserId = "admin",
                RoomId = "room1",
                RoleId = (int)RoleEnum.Student,
                Status = 1
            };
            var exam = new Exam
            {
                ExamId = "exam1",
                Title = "Test Exam",
                Status = (int)ExamStatus.Published,
                RoomId = "room1",
                StartTime = DateTime.UtcNow.AddHours(-1),
                EndTime = DateTime.UtcNow.AddHours(1),
                Duration = 60,
                Creator = user
            };
            var examSupervisor = new ExamSupervisor
            {
                ExamSupervisorId = "es1",
                ExamId = "exam1",
                SupervisorId = "admin",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "admin"
            };
            var studentExam = new StudentExam
            {
                StudentExamId = "se1",
                ExamId = "exam1",
                StudentId = "admin",
                Status = (int)StudentExamStatus.InProgress,
                StartTime = DateTime.UtcNow.AddMinutes(-30)
            };
            _context.StudentExams.Add(studentExam);

            _context.Users.Add(user);
            _context.Roles.Add(role);
            _context.UserRoles.Add(userRole);
            _context.Subjects.Add(subject);
            _context.Classes.Add(class1);
            _context.Rooms.Add(room);
            _context.RoomUsers.Add(roomUser);
            _context.Exams.Add(exam);
            _context.ExamSupervisors.Add(examSupervisor);
            await _context.SaveChangesAsync();

            var search = new MonitorExamSearchVM { CurrentPage = 1, PageSize = 10, SubjectId = "sub1" };

            // Act
            var (message, result) = await _service.GetExamOverview(search, "admin");

            // Assert
            Assert.Empty(message);
            Assert.NotNull(result);
            Assert.Equal(1, result.Total);
        }

        [Fact]
        public async Task GetExamMonitorDetail_WithStatusFilter_ReturnsFilteredResults()
        {
            // Arrange
            var user = new User { UserId = "admin", FullName = "Admin", Email = "admin@email.com" };
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
                RoomId = "room1",
                TotalQuestions = 10,
                CreateUser = "admin"
            };
            var studentExam = new StudentExam
            {
                StudentExamId = "se1",
                ExamId = "exam1",
                StudentId = "student1",
                Status = (int)StudentExamStatus.InProgress,
                StartTime = DateTime.UtcNow.AddMinutes(-30),
                User = user
            };
            var examSupervisor = new ExamSupervisor
            {
                ExamSupervisorId = "es1",
                ExamId = "exam1",
                SupervisorId = "admin",
                CreatedBy = "admin",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.Roles.Add(role);
            _context.UserRoles.Add(userRole);
            _context.Subjects.Add(subject);
            _context.Classes.Add(class1);
            _context.Rooms.Add(room);
            _context.Exams.Add(exam);
            _context.StudentExams.Add(studentExam);
            _context.ExamSupervisors.Add(examSupervisor);
            await _context.SaveChangesAsync();

            var search = new MonitorExamDetailSearchVM
            {
                ExamId = "exam1",
                CurrentPage = 1,
                PageSize = 10,
                StudentExamStatus = StudentExamStatus.InProgress
            };

            // Act
            var (message, result) = await _service.GetExamMonitorDetail(search, "admin");

            // Assert
            Assert.Empty(message);
            Assert.NotNull(result);
            Assert.Equal(1, result.Total);
        }

        [Fact]
        public async Task GetExamMonitorDetail_WithTextSearch_ReturnsFilteredResults()
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
                RoomId = "room1",
                TotalQuestions = 10,
                CreateUser = "admin"
            };
            var studentExam = new StudentExam
            {
                StudentExamId = "se1",
                ExamId = "exam1",
                StudentId = "admin",
                Status = (int)StudentExamStatus.InProgress,
                StartTime = DateTime.UtcNow.AddMinutes(-30),
                User = user
            };
            var examSupervisor = new ExamSupervisor
            {
                ExamSupervisorId = "es1",
                ExamId = "exam1",
                SupervisorId = "admin",
                CreatedBy = "admin",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.Roles.Add(role);
            _context.UserRoles.Add(userRole);
            _context.Subjects.Add(subject);
            _context.Classes.Add(class1);
            _context.Rooms.Add(room);
            _context.Exams.Add(exam);
            _context.StudentExams.Add(studentExam);
            _context.ExamSupervisors.Add(examSupervisor);
            await _context.SaveChangesAsync();

            var search = new MonitorExamDetailSearchVM
            {
                ExamId = "exam1",
                CurrentPage = 1,
                PageSize = 10,
                TextSearch = "Admin"
            };

            // Act
            var (message, result) = await _service.GetExamMonitorDetail(search, "admin");

            // Assert
            Assert.Empty(message);
            Assert.NotNull(result);
            Assert.Equal(1, result.Total);
        }
    }
}