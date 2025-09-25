using API.Factory;
using API.Helper;
using API.Models;
using API.Repository;
using API.Repository.Interface;
using API.Services;
using API.Strategy.Interface;
using API.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Xunit;

namespace API.Tests
{
    public class StudentExamServiceTests : IDisposable
    {
        private readonly Sep490Context _context;
        private readonly StudentExamService _service;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IScoringStrategyFactory> _factory;


        public StudentExamServiceTests()
        {
            var options = new DbContextOptionsBuilder<Sep490Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            _context = new Sep490Context(options);
            _factory = new Mock<IScoringStrategyFactory>();

            var mockStrategy = new Mock<IScoringStrategy>();
            mockStrategy
                .Setup(s => s.ScoreAnswers(
                    It.IsAny<List<StudentAnswer>>(),
                    It.IsAny<Dictionary<string, ExamQuestion>>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IStudentAnswerRepository>()))
                .ReturnsAsync((10m, 1)); // Trả điểm giả định và trạng thái hợp lệ

            _factory
                .Setup(f => f.GetStrategy(It.IsAny<QuestionTypeChoose>()))
                .Returns(mockStrategy.Object);


            _unitOfWork = new Mock<IUnitOfWork>();
            _service = new StudentExamService(_context, _unitOfWork.Object, _factory.Object);
            var studentExamRepo = new Mock<IStudentExamRepository>();
            _unitOfWork.Setup(u => u.StudentExams).Returns(studentExamRepo.Object);
            var examRepo = new Mock<IExamRepository>();
            _unitOfWork.Setup(u => u.Exams).Returns(examRepo.Object);

            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpContext.Setup(ctx => ctx.Request.Headers["X-Forwarded-For"])
                .Returns(new Microsoft.Extensions.Primitives.StringValues("127.0.0.1"));
            _mockHttpContext
                .Setup(c => c.Request.Headers["User-Agent"])
                .Returns("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        }

        public void Dispose()
        {
            _context.Dispose();
        }
        [Fact]
        public async Task GetList_WithExams_ReturnsList()
        {
            // Arrange
            var user = new User { UserId = "student1", FullName = "Test Student", Email = "student@email.com" };
            var subject = new Subject { SubjectId = "sub1", SubjectName = "Test Subject", SubjectCode = "1" };
            var class1 = new Class { ClassId = "class1", ClassCode = "C001", CreatedBy = "1" };
            var room = new Room { RoomId = "room1", RoomCode = "R001", SubjectId = "sub1", ClassId = "class1" };
            var roomUser = new RoomUser { RoomId = "room1", UserId = "student1", Status = (int)ActiveStatus.Active, RoomUserId = "1" };
            var exam = new Exam
            {
                ExamId = "exam1",
                Title = "Test Exam",
                CreateUser = "1",
                Status = (int)ExamStatus.Published,
                RoomId = "room1",
                StartTime = DateTime.UtcNow.AddMinutes(5),
                EndTime = DateTime.UtcNow.AddHours(1),
                Duration = 60
            };

            _context.Users.Add(user);
            _context.Subjects.Add(subject);
            _context.Classes.Add(class1);
            _context.Rooms.Add(room);
            _context.RoomUsers.Add(roomUser);
            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();

            // Mock dependencies
            var mockStudentExamRepo = new Mock<IStudentExamRepository>();
            mockStudentExamRepo.Setup(x => x.GetSubmittedExamIds(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<string>());

            mockStudentExamRepo.Setup(x => x.GetByExamIds(It.IsAny<string>(), It.IsAny<List<string>>()))
                .ReturnsAsync(new List<StudentExam>());

            var mockExamRepo = new Mock<IExamRepository>();
            mockExamRepo.Setup(x => x.GetAvailableExamsForStudent(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<Exam> { exam });

            _unitOfWork.Setup(x => x.StudentExams).Returns(mockStudentExamRepo.Object);
            _unitOfWork.Setup(x => x.Exams).Returns(mockExamRepo.Object);

            // Act
            var (message, result) = await _service.GetList("student1");

            // Assert
            Assert.Empty(message);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetList_NoExams_ReturnsEmptyMessage()
        {
            // Arrange
            var mockStudentExamRepo = new Mock<IStudentExamRepository>();
            mockStudentExamRepo.Setup(x => x.GetSubmittedExamIds(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<string>());

            var mockExamRepo = new Mock<IExamRepository>();
            mockExamRepo.Setup(x => x.GetAvailableExamsForStudent(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<Exam>());  // No exams

            _unitOfWork.Setup(x => x.StudentExams).Returns(mockStudentExamRepo.Object);
            _unitOfWork.Setup(x => x.Exams).Returns(mockExamRepo.Object);

            // Act
            var (message, result) = await _service.GetList("student1");

            // Assert
            Assert.Equal("No exams found.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task AccessExam_ValidOtp_ReturnsSuccess()
        {
            // Arrange
            var user = new User { UserId = "student1", FullName = "Test Student", Email = "student@email.com" };
            var subject = new Subject { SubjectId = "sub1", SubjectName = "Test Subject", SubjectCode = "1" };
            var class1 = new Class { ClassId = "class1", ClassCode = "C001", CreatedBy = "1" };
            var room = new Room { RoomId = "room1", RoomCode = "R001", SubjectId = "sub1", ClassId = "class1" };
            var roomUser = new RoomUser { RoomId = "room1", UserId = "student1", Status = (int)ActiveStatus.Active, RoomUserId = "1" };
            var exam = new Exam
            {
                ExamId = "exam1",
                Title = "Test Exam",
                Status = (int)ExamStatus.Published,
                RoomId = "room1",
                CreateUser = "teacher1",
                StartTime = DateTime.UtcNow.AddHours(-1),
                EndTime = DateTime.UtcNow.AddHours(1),
                Duration = 60
            };
            var examOtp = new ExamOtp
            {
                ExamOtpId = "eo1",
                ExamId = "exam1",
                CreatedBy = "teacher1",
                OtpCode = 123456,
                ExpiredAt = DateTime.UtcNow.AddHours(1)
            };

            _context.Users.Add(user);
            _context.Subjects.Add(subject);
            _context.Classes.Add(class1);
            _context.Rooms.Add(room);
            _context.RoomUsers.Add(roomUser);
            _context.Exams.Add(exam);
            _context.ExamOtps.Add(examOtp);
            await _context.SaveChangesAsync();
            // Mock ExamRepository
            var examRepo = new Mock<IExamRepository>();
            examRepo.Setup(r => r.GetExamWithRoomUsers("exam1"))
                    .ReturnsAsync(exam);
            _unitOfWork.Setup(u => u.Exams).Returns(examRepo.Object);

            // Mock ExamOtpRepository
            var otpRepo = new Mock<IExamOtpRepository>();
            otpRepo.Setup(r => r.IsOtpValid("exam1", 123456, It.IsAny<DateTime>()))
                   .ReturnsAsync(true);
            _unitOfWork.Setup(u => u.ExamOTPs).Returns(otpRepo.Object);

            // Mock StudentExamRepository
            var studentExamRepo = new Mock<IStudentExamRepository>();
            studentExamRepo.Setup(r => r.GetByExamAndStudent("exam1", "student1"))
                           .ReturnsAsync((StudentExam?)null); // Không có bài thi trước đó
            _unitOfWork.Setup(u => u.StudentExams).Returns(studentExamRepo.Object);

            // Mock thêm Add + SaveChanges
            studentExamRepo.Setup(r => r.Add(It.IsAny<StudentExam>()))
                           .Callback<StudentExam>(se => se.StudentExamId = "se123"); // gán ID giả

            _unitOfWork.Setup(u => u.StudentExams).Returns(studentExamRepo.Object);
            _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var request = new StudentExamRequest
            {
                ExamId = "exam1",
                OtpCode = 123456
            };

            // Act
            var (message, result) = await _service.AccessExam(request, "student1", _mockHttpContext.Object);

            // Assert
            Assert.Empty(message);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task AccessExam_ExamNotFound_ReturnsError()
        {
            // Arrange
            var request = new StudentExamRequest
            {
                ExamId = "nonexistent",
                OtpCode = 123456
            };

            // Act
            var (message, result) = await _service.AccessExam(request, "student1", _mockHttpContext.Object);

            // Assert
            Assert.Equal("Exam not found.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task AccessExam_ExamNotStarted_ReturnsError()
        {
            // Arrange
            var exam = new Exam
            {
                ExamId = "exam1",
                Title = "Test Exam",
                Status = (int)ExamStatus.Published,
                RoomId = "room1",
                CreateUser = "teacher1",
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2),
                Duration = 60
            };

            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();

            var request = new StudentExamRequest
            {
                ExamId = "exam1",
                OtpCode = 123456
            };

            // Act
            var (message, result) = await _service.AccessExam(request, "student1", _mockHttpContext.Object);

            // Assert
            Assert.Contains("Exam not found", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task AccessExam_ExamEnded_ReturnsError()
        {
            // Arrange
            var exam = new Exam
            {
                ExamId = "exam1",
                Title = "Test Exam",
                Status = (int)ExamStatus.Published,
                RoomId = "room1",
                StartTime = DateTime.UtcNow.AddHours(-2),
                EndTime = DateTime.UtcNow.AddHours(-1),
                CreateUser = "teacher1",
                Duration = 60
            };

            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();

            var request = new StudentExamRequest
            {
                ExamId = "exam1",
                OtpCode = 123456
            };

            // Act
            var (message, result) = await _service.AccessExam(request, "student1", _mockHttpContext.Object);

            // Assert
            Assert.Contains("Exam not found.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task AccessExam_InvalidOtp_ReturnsError()
        {
            // Arrange
            var user = new User { UserId = "student1", FullName = "Test Student", Email = "student@email.com" };
            var subject = new Subject { SubjectId = "sub1", SubjectName = "Test Subject", SubjectCode = "1" };
            var class1 = new Class { ClassId = "class1", ClassCode = "C001", CreatedBy = "1" };
            var room = new Room { RoomId = "room1", RoomCode = "R001", SubjectId = "sub1", ClassId = "class1" };
            var roomUser = new RoomUser { RoomId = "room1", UserId = "student1", Status = (int)ActiveStatus.Active, RoomUserId = "1" };
            var exam = new Exam
            {
                ExamId = "exam1",
                Title = "Test Exam",
                Status = (int)ExamStatus.Published,
                RoomId = "room1",
                CreateUser = "1",
                StartTime = DateTime.UtcNow.AddHours(-1),
                EndTime = DateTime.UtcNow.AddHours(1),
                Duration = 60,
            };

            _context.Users.Add(user);
            _context.Subjects.Add(subject);
            _context.Classes.Add(class1);
            _context.Rooms.Add(room);
            _context.RoomUsers.Add(roomUser);
            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();

            var request = new StudentExamRequest
            {
                ExamId = "exam1",
                OtpCode = 0
            };

            // Mock UnitOfWork
            var examRepo = new Mock<IExamRepository>();
            examRepo.Setup(r => r.GetExamWithRoomUsers("exam1"))
                    .ReturnsAsync(exam);

            var otpRepo = new Mock<IExamOtpRepository>();
            otpRepo.Setup(r => r.IsOtpValid("exam1", 0, It.IsAny<DateTime>()))
                   .ReturnsAsync(false);

            var studentExamRepo = new Mock<IStudentExamRepository>();
            studentExamRepo.Setup(r => r.GetByExamAndStudent("exam1", "student1"))
                           .ReturnsAsync((StudentExam?)null); // Không có bài thi trước đó

            _unitOfWork.Setup(u => u.Exams).Returns(examRepo.Object);
            _unitOfWork.Setup(u => u.ExamOTPs).Returns(otpRepo.Object);
            _unitOfWork.Setup(u => u.StudentExams).Returns(studentExamRepo.Object);

            // Act
            var (message, result) = await _service.AccessExam(request, "student1", _mockHttpContext.Object);

            // Assert
            Assert.Equal("Invalid or expired OTP.", message);
            Assert.Null(result);
        }

        //[Fact]
        //public async Task GetExamDetail_ValidExamId_ReturnsExamDetail()
        //{
        //    // Arrange
        //    var exam = new Exam
        //    {
        //        ExamId = "exam1",
        //        Title = "Test Exam",
        //        Status = (int)ExamStatus.Published,
        //        Duration = 60,
        //        CreateUser = "teacher1",
        //        RoomId = "room1",
        //        TotalQuestions = 10,
        //        ExamType = (int)QuestionType.MultipleChoice
        //    };

        //    var question = new Question
        //    {
        //        QuestionId = "q1",
        //        Content = "Test question",
        //        CorrectAnswer = "A",
        //        Explanation = "Explanation test",
        //        Options = "[\"A\",\"B\",\"C\",\"D\"]",
        //        Type = (int)QuestionTypeChoose.MultipleChoice,
        //        CreateUser = "admin",
        //        UpdateUser = "admin"
        //    };

        //    var examQuestions = new List<ExamQuestion>
        //    {
        //        new ExamQuestion
        //        {
        //            ExamId = "exam1",
        //            QuestionId = "q1",
        //            Points = 10,
        //            Question = question
        //        }
        //    };

        //    // Setup mocks
        //    _unitOfWork.Setup(u => u.Exams.GetById("exam1")).ReturnsAsync(exam);

        //    var examQuestionRepo = new Mock<IExamQuestionRepository>();
        //    _unitOfWork.Setup(u => u.ExamQuestions).Returns(examQuestionRepo.Object);

        //    // Act
        //    var (message, result) = await _service.GetExamDetail("exam1");

        //    // Assert
        //    Assert.Empty(message);
        //    Assert.NotNull(result);
        //    Assert.Equal("exam1", result.ExamId);
        //    Assert.Equal("Test Exam", result.Title);
        //    Assert.Equal(10, result.TotalQuestions);
        //    Assert.Single(result.Questions);
        //    Assert.Equal("q1", result.Questions[0].QuestionId);
        //    Assert.Equal("Test question", result.Questions[0].Content);
        //}


        [Fact]
        public async Task GetExamDetail_ExamNotFound_ReturnsError()
        {
            // Act
            var (message, result) = await _service.GetExamDetail("nonexistent");

            // Assert
            Assert.Equal("Exam not found.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task SubmitExam_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var exam = new Exam
            {
                ExamId = "exam1",
                Title = "Test Exam",
                Status = (int)ExamStatus.Published,
                EndTime = DateTime.UtcNow.AddHours(1),
                CreateUser = "teacher1",
                RoomId = "room1",
                ExamType = (int)QuestionTypeChoose.MultipleChoice
            };
            var studentExam = new StudentExam
            {
                StudentExamId = "se1",
                ExamId = "exam1",
                StudentId = "student1",
                Status = (int)StudentExamStatus.InProgress,
                StartTime = DateTime.UtcNow.AddMinutes(-30)
            };

            _context.Exams.Add(exam);
            _context.StudentExams.Add(studentExam);
            await _context.SaveChangesAsync();

            var request = new SubmitExamRequest
            {
                ExamId = "exam1",
                StudentExamId = "se1",
                Answers = new List<StudentAnswerVM>
                {
                    new StudentAnswerVM { QuestionId = "q1", UserAnswer = "A" }
                }
            };
            var result = await _service.SubmitExam1(request, "student1", _mockHttpContext.Object);
            Assert.Empty(result);
        }

        [Fact]
        public async Task SubmitExam_StudentExamNotFound_ReturnsError()
        {
            // Arrange
            var request = new SubmitExamRequest
            {
                ExamId = "exam1",
                StudentExamId = "nonexistent",
                Answers = new List<StudentAnswerVM>()
            };

            // Act
            var result = await _service.SubmitExam1(request, "student1", _mockHttpContext.Object);

            // Assert
            Assert.Contains("not started this exam", result);
        }

        [Fact]
        public async Task SaveAnswerTemporary_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var exam = new Exam
            {
                ExamId = "exam1",
                Title = "Test Exam",
                Status = (int)ExamStatus.Published,
                EndTime = DateTime.UtcNow.AddHours(1),
                CreateUser = "teacher1",
                RoomId = "room1",
            };

            var studentExam = new StudentExam
            {
                StudentExamId = "se1",
                ExamId = "exam1",
                StudentId = "student1",
                Status = (int)StudentExamStatus.InProgress,
                Exam = exam,
                StartTime = DateTime.UtcNow.AddMinutes(-30)
            };

            var request = new SubmitExamRequest
            {
                ExamId = "exam1",
                StudentExamId = "se1",
                Answers = new List<StudentAnswerVM>
                {
                    new StudentAnswerVM { QuestionId = "q1", UserAnswer = "A" }
                }
            };

            // Mock UnitOfWork + repos
            var unitOfWork = new Mock<IUnitOfWork>();

            var studentExamRepo = new Mock<IStudentExamRepository>();
            studentExamRepo.Setup(r => r.GetByStudentExamId("se1", "exam1", "student1"))
                           .ReturnsAsync(studentExam);
            unitOfWork.Setup(u => u.StudentExams).Returns(studentExamRepo.Object);

            var studentAnswerRepo = new Mock<IStudentAnswerRepository>();
            studentAnswerRepo.Setup(r => r.GetByStudentExamAndQuestionIds("se1", It.IsAny<List<string>>(), false))
                             .ReturnsAsync(new List<StudentAnswer>());
            studentAnswerRepo.Setup(r => r.AddRangeAsync(It.IsAny<List<StudentAnswer>>()))
                             .Returns(Task.CompletedTask);
            unitOfWork.Setup(u => u.StudentAnswers).Returns(studentAnswerRepo.Object);

            unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Context chỉ cần cho constructor (không dùng studentAnswers nếu mock đủ)
            var context = new Sep490Context(new DbContextOptionsBuilder<Sep490Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options);

            var service = new StudentExamService(context, unitOfWork.Object, Mock.Of<IScoringStrategyFactory>());

            // Act
            var result = await service.SaveAnswerTemporary(request, "student1");

            // Assert
            Assert.Empty(result);
        }


        [Fact]
        public async Task SaveAnswerTemporary_StudentExamNotFound_ReturnsError()
        {
            // Arrange
            var request = new SubmitExamRequest
            {
                ExamId = "exam1",
                StudentExamId = "nonexistent",
                Answers = new List<StudentAnswerVM>()
            };

            // Act
            var result = await _service.SaveAnswerTemporary(request, "student1");

            // Assert
            Assert.Contains("not found in this exam", result);
        }

        [Fact]
        public async Task GetHistoryExams_WithExams_ReturnsList()
        {
            // Arrange
            var exam = new Exam { ExamId = "exam1", Title = "Test Exam", CreateUser = "1", RoomId = "1" };
            var studentExam = new StudentExam
            {
                StudentExamId = "se1",
                ExamId = "exam1",
                StudentId = "student1",
                Status = (int)StudentExamStatus.Submitted,
                SubmitTime = DateTime.UtcNow,
                Score = 85
            };

            _context.Exams.Add(exam);
            _context.StudentExams.Add(studentExam);
            await _context.SaveChangesAsync();

            var search = new SearchStudentExamVM { CurrentPage = 1, PageSize = 10 };

            // Act
            var (message, result) = await _service.GetHistoryExams(search, "student1");

            // Assert
            Assert.Empty(message);
            Assert.NotNull(result);
            Assert.Equal(1, result.Total);
        }

        [Fact]
        public async Task GetHistoryExamDetail_ValidId_ReturnsDetail()
        {
            // Arrange
            var user = new User { UserId = "student1", FullName = "Test Student", Email = "student@email.com" };
            var exam = new Exam { ExamId = "exam1", Title = "Test Exam", IsShowResult = true, CreateUser = "1", RoomId = "1" };
            var studentExam = new StudentExam
            {
                StudentExamId = "se1",
                ExamId = "exam1",
                StudentId = "student1",
                Status = (int)StudentExamStatus.Submitted,
                SubmitTime = DateTime.UtcNow,
                Score = 85,
                TotalQuestions = 10
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
            var studentAnswer = new StudentAnswer
            {
                StudentAnswerId = "sa1",
                StudentExamId = "se1",
                QuestionId = "q1",
                UserAnswer = "A",
                IsCorrect = true,
                PointsEarned = 10
            };

            _context.Exams.Add(exam);
            _context.Users.Add(user);
            _context.StudentExams.Add(studentExam);
            _context.Questions.Add(question);
            _context.StudentAnswers.Add(studentAnswer);
            await _context.SaveChangesAsync();

            // Act
            var (message, result) = await _service.GetHistoryExamDetail("se1", "student1");

            // Assert
            Assert.Empty(message);
            Assert.NotNull(result);
            Assert.Equal("Test Exam", result.ExamTitle);
            Assert.Equal(85, result.Score);
        }

        [Fact]
        public async Task GetSavedAnswers_ValidExam_ReturnsAnswers()
        {
            // Arrange
            var exam = new Exam
            {
                ExamId = "exam1",
                Title = "Test Exam",
                Status = (int)ExamStatus.Published,
                CreateUser = "teacher1",
                RoomId = "room1",

                EndTime = DateTime.UtcNow.AddHours(1)
            };
            var studentExam = new StudentExam
            {
                StudentExamId = "se1",
                ExamId = "exam1",
                StudentId = "student1",
                Status = (int)StudentExamStatus.InProgress,
                Exam = exam
            };
            var studentAnswer = new StudentAnswer
            {
                StudentAnswerId = "sa1",
                StudentExamId = "se1",
                QuestionId = "q1",
                UserAnswer = "A"
            };

            _context.Exams.Add(exam);
            _context.StudentExams.Add(studentExam);
            _context.StudentAnswers.Add(studentAnswer);
            await _context.SaveChangesAsync();
            _unitOfWork.Setup(u => u.StudentExams.GetExamInProgress("exam1", "student1"))
    .ReturnsAsync(studentExam);

            // Act
            var (message, result) = await _service.GetSavedAnswers("exam1", "student1");

            // Assert
            Assert.Empty(message);
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("q1", result[0].QuestionId);
            Assert.Equal("A", result[0].UserAnswer);
        }

        [Fact]
        public async Task GetEssayExam_ValidRequest_ReturnsEssayExam()
        {
            // Arrange
            var user = new User { UserId = "student1", FullName = "Test", Email = "a@email.com" };
            var role = new Role { Id = 4, Name = "Admin" };
            var userRole = new UserRole { UserId = "student1", RoleId = 4 };
            var room = new Room { RoomId = "room1", Capacity = 30, RoomCode = "R001", SubjectId = "sub1", ClassId = "class1" };
            var subject = new Subject { SubjectId = "sub1", SubjectName = "Test Subject", SubjectCode = "1" };
            var examSupervisor = new ExamSupervisor
            {
                ExamId = "exam1",
                CreatedBy = "teacher1",
                ExamSupervisorId = "es1",
                SupervisorId = "supervisor1"
            };
            var exam = new Exam
            {
                CreateUser = "teacher1",
                RoomId = "room1",
                ExamId = "exam1",
                Title = "Test Essay Exam",
                ExamType = (int)QuestionTypeChoose.Essay
            };
            var studentExam = new StudentExam
            {
                StudentExamId = "se1",
                ExamId = "exam1",
                StudentId = "student1",
                Status = (int)StudentExamStatus.Submitted
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
            var examQuestion = new ExamQuestion { ExamId = "exam1", QuestionId = "q1", Points = 20, ExamQuestionId = "1" };
            var studentAnswer = new StudentAnswer
            {
                StudentAnswerId = "sa1",
                StudentExamId = "se1",
                QuestionId = "q1",
                UserAnswer = "Student answer",
                PointsEarned = 15
            };

            _context.Exams.Add(exam);
            _context.Roles.Add(role);
            _context.UserRoles.Add(userRole);
            _context.Users.Add(user);
            _context.Rooms.Add(room);
            _context.ExamSupervisors.Add(examSupervisor);
            _context.Subjects.Add(subject);
            _context.StudentExams.Add(studentExam);
            _context.Questions.Add(question);
            _context.ExamQuestions.Add(examQuestion);
            _context.StudentAnswers.Add(studentAnswer);
            await _context.SaveChangesAsync();

            // Act
            var (message, result) = await _service.GetEssayExam("se1", "exam1", "student1");

            // Assert
            Assert.Empty(message);
            var essayResult = result as StudentEssayAnswerVM;
            Assert.NotNull(essayResult);
            Assert.Single(essayResult.Answers);
            Assert.Equal("q1", essayResult.Answers[0].QuestionId);
            Assert.Equal("Test question", essayResult.Answers[0].QuestionContent);
        }

        [Fact]
        public async Task MarkEssay_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var user = new User { UserId = "a", FullName = "Test", Email = "a@email.com" };
            var role = new Role { Id = 4, Name = "Admin" };
            var userRole = new UserRole { UserId = "a", RoleId = 1 };
            var exam = new Exam { ExamId = "exam1", Title = "Test Essay Exam", CreateUser = "a", RoomId = "1" };
            var examSupervisor = new ExamSupervisor
            {
                ExamId = "exam1",
                CreatedBy = "a",
                ExamSupervisorId = "a1",
                SupervisorId = "1d"
            };
            var studentExam = new StudentExam
            {
                StudentExamId = "se1",
                ExamId = "exam1",
                StudentId = "student1",
                Status = (int)StudentExamStatus.Submitted
            };
            var examQuestion = new ExamQuestion { ExamId = "exam1", QuestionId = "q1", Points = 20, ExamQuestionId = "1" };
            var studentAnswer = new StudentAnswer
            {
                StudentAnswerId = "sa1",
                StudentExamId = "se1",
                QuestionId = "q1",
                UserAnswer = "Student answer"
            };

            _context.Exams.Add(exam);
            _context.Users.Add(user);
            _context.Roles.Add(role);
            _context.UserRoles.Add(userRole);
            _context.ExamSupervisors.Add(examSupervisor);
            _context.StudentExams.Add(studentExam);
            _context.ExamQuestions.Add(examQuestion);
            _context.StudentAnswers.Add(studentAnswer);
            await _context.SaveChangesAsync();

            var request = new MarkEssayRequest
            {
                StudentExamId = "se1",
                ExamId = "exam1",
                Scores = new List<EssayScoreVM>
                {
                    new EssayScoreVM { QuestionId = "q1", PointsEarned = 15 }
                }
            };
            var result = await _service.MarkEssay(request, "a");
            Assert.Empty(result);
        }

        [Fact]
        public async Task MarkEssay_StudentExamNotFound_ReturnsError()
        {
            // Arrange
            var request = new MarkEssayRequest
            {
                StudentExamId = "nonexistent",
                ExamId = "exam1",
                Scores = new List<EssayScoreVM>()
            };

            // Act
            var result = await _service.MarkEssay(request, "admin");

            // Assert
            Assert.Contains("not found", result);
        }
    }
}