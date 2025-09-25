using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Models;
using API.Helper;
using Microsoft.EntityFrameworkCore;
using Xunit;
using API.Services;
using API.ViewModels;
using DocumentFormat.OpenXml.InkML;
using Microsoft.Extensions.Options;
using System.Text.Json;
using API.Repository;
using Moq;

namespace TestExamServicePackage
{
    public class ExamServiceTests
    {
        private Mock<IUnitOfWork> mockUnitOfWork = new Mock<IUnitOfWork>();
        private Sep490Context GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<Sep490Context>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            return new Sep490Context(options);
        }

        private User CreateUser(string userId, bool isAdmin, bool isLecture)
        {
            var roles = new List<UserRole>();
            if (isAdmin) roles.Add(new UserRole { RoleId = (int)RoleEnum.Admin, UserId = userId });
            if (isLecture) roles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture, UserId = userId });

            return new User
            {
                UserId = userId,
                UserRoles = roles
            };
        }

        private QuestionBank CreateQuestionBank(string bankId, int questionType = 1, int status = 1, decimal point = 5, int count = 2)
        {
            string subjectId = $"Subject{bankId}";
            var questions = new List<Question>();
            for (int i = 0; i < count; i++)
            {
                questions.Add(new Question
                {
                    QuestionId = Guid.NewGuid().ToString(),
                    QuestionBankId = bankId,
                    Content = $"{i + 1} = ?",
                    Type = questionType,
                    Point = point,
                    Status = status,
                    DifficultLevel = 1,
                    Options = $"A:{i + 1},B:{i + 2},C:{i + 3},D:{i + 4}",
                    CorrectAnswer = $"A:{i + 1}",
                    Explanation = $"Explanation for question {i + 1}",
                    CreateUser = "",
                    UpdateUser = "",
                    SubjectId = subjectId

                });
            }

            return new QuestionBank
            {
                QuestionBankId = bankId,
                Title = "Bank 1",
                Questions = questions,
                CreateUserId = "",
                SubjectId = subjectId
            };
        }

        private AddExamRequest CreateValidRequest(string bankId, string roomId, List<string> questionIds, int examType = 1)
        {
            return new AddExamRequest
            {
                QuestionBankId = bankId,
                RoomId = roomId,
                QuestionIds = questionIds,
                Title = "Test Exam",
                Description = "Desc",
                Duration = 60,
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2),
                IsShowResult = true,
                IsShowCorrectAnswer = true,
                Status = 1,
                ExamType = examType,
                GuideLines = "GL"
            };
        }
//--------------------------------------------------------------------------------ADD---------------------------------------------------------------------------------------
        [Fact]
        public async Task AddExamAsync_UserWithoutPermission_ShouldFail()
        {
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object); var userId = Guid.NewGuid().ToString();
            context.Users.Add(CreateUser(userId, false, false));
            await context.SaveChangesAsync();

            var req = CreateValidRequest(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), new List<string> { "Q1", "Q2" });

            var result = await service.AddExamAsync(req, userId);

            Assert.False(result.Success);
            Assert.Contains("permission", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AddExamAsync_EndTimeBeforeStartTime_ShouldFail()
        {
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object);

            var userId = Guid.NewGuid().ToString();
            context.Users.Add(CreateUser(userId, true, true));
            await context.SaveChangesAsync();

            var req = CreateValidRequest(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), new List<string> { "Q1", "Q2" });
            req.EndTime = req.StartTime.AddMinutes(-1);

            var result = await service.AddExamAsync(req, userId);

            Assert.False(result.Success);
            Assert.Contains("EndTime", result.Message);
        }

        [Fact]
        public async Task AddExamAsync_RoomNotExist_ShouldFail()
        {
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object);
            var userId = Guid.NewGuid().ToString();
            context.Users.Add(CreateUser(userId, true, true));
            await context.SaveChangesAsync();

            var req = CreateValidRequest(Guid.NewGuid().ToString(), "invalidRoom", new List<string> { "Q1", "Q2" });

            var result = await service.AddExamAsync(req, userId);

            Assert.False(result.Success);
            Assert.Contains("room", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AddExamAsync_TitleAlreadyExists_ShouldFail()
        {
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object);
            var userId = Guid.NewGuid().ToString();

            context.Users.Add(CreateUser(userId, true, true));
            var roomId = Guid.NewGuid().ToString();
            context.Rooms.Add(new Room { RoomId = roomId, RoomCode = "Room", ClassId = "", SubjectId = "" });
            context.Exams.Add(new Exam { ExamId = Guid.NewGuid().ToString(), RoomId = roomId, Title = "Test Exam", CreateUser = userId });
            await context.SaveChangesAsync();

            var req = CreateValidRequest(Guid.NewGuid().ToString(), roomId, new List<string> { "Q1", "Q2" });

            var result = await service.AddExamAsync(req, userId);

            Assert.False(result.Success);
            Assert.Contains("An exam with the same title already exist", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AddExamAsync_QuestionBankNotFound_ShouldFail()
        {
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object); var userId = Guid.NewGuid().ToString();

            context.Users.Add(CreateUser(userId, true, true));
            var roomId = Guid.NewGuid().ToString();
            context.Rooms.Add(new Room { RoomId = roomId, RoomCode = "Room", ClassId = "", SubjectId = "" });
            await context.SaveChangesAsync();

            var req = CreateValidRequest("nonexistentBank", roomId, new List<string> { "Q1", "Q2" });

            var result = await service.AddExamAsync(req, userId);

            Assert.False(result.Success);
            Assert.Contains("bank not found", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AddExamAsync_QuestionBankEmpty_ShouldFail()
        {
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object); var userId = Guid.NewGuid().ToString();

            context.Users.Add(CreateUser(userId, true, true));
            var roomId = Guid.NewGuid().ToString();
            context.Rooms.Add(new Room { RoomId = roomId, RoomCode = "Room", ClassId = "ClassId1", SubjectId = "subjectId1" });
            context.QuestionBanks.Add(new QuestionBank { QuestionBankId = "bank1", SubjectId = $"subjectId1", CreateUserId = "", Title = "QuestionBankEmpty", Questions = new List<Question>() });
            await context.SaveChangesAsync();

            var req = CreateValidRequest("bank1", roomId, new List<string> { "Q1", "Q2" });

            var result = await service.AddExamAsync(req, userId);

            Assert.False(result.Success);
            Assert.Contains("no questions", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AddExamAsync_InvalidQuestionIds_ShouldFail()
        {
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object);
            var userId = Guid.NewGuid().ToString();

            context.Users.Add(CreateUser(userId, true, true));
            var roomId = Guid.NewGuid().ToString();
            context.Rooms.Add(new Room { RoomId = roomId, RoomCode = "Room", ClassId = "", SubjectId = "" });
            var bank = CreateQuestionBank("bank1");
            context.QuestionBanks.Add(bank);
            await context.SaveChangesAsync();

            var req = CreateValidRequest("bank1", roomId, new List<string> { "invalidId" });

            var result = await service.AddExamAsync(req, userId);

            Assert.False(result.Success);
            Assert.Contains("do not belong", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AddExamAsync_NoQuestionsSelected_ShouldFail()
        {
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object);
            var userId = Guid.NewGuid().ToString();

            context.Users.Add(CreateUser(userId, true, true));
            var roomId = Guid.NewGuid().ToString();
            context.Rooms.Add(new Room { RoomId = roomId, RoomCode = "Room", ClassId = "", SubjectId = "" });
            var bank = CreateQuestionBank("bank1");
            context.QuestionBanks.Add(bank);
            await context.SaveChangesAsync();

            var req = CreateValidRequest("bank1", roomId, new List<string>());

            var result = await service.AddExamAsync(req, userId);

            Assert.False(result.Success);
            Assert.Contains("No questions", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AddExamAsync_DuplicateQuestionIds_ShouldFail()
        {
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object);
            var userId = Guid.NewGuid().ToString();

            context.Users.Add(CreateUser(userId, true, true));
            var roomId = Guid.NewGuid().ToString();
            context.Rooms.Add(new Room { RoomId = roomId, RoomCode = "Room", ClassId = "", SubjectId = "" });
            var bank = CreateQuestionBank("bank1");
            context.QuestionBanks.Add(bank);
            await context.SaveChangesAsync();

            var qId = bank.Questions.First().QuestionId;
            var req = CreateValidRequest("bank1", roomId, new List<string> { qId, qId });

            var result = await service.AddExamAsync(req, userId);

            Assert.False(result.Success);
            Assert.Contains("Duplicate", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AddExamAsync_MultipleTypes_ShouldFail()
        {
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object); var userId = Guid.NewGuid().ToString();

            context.Users.Add(CreateUser(userId, true, true));
            var roomId = Guid.NewGuid().ToString();
            context.Rooms.Add(new Room { RoomId = roomId, RoomCode = "Room", ClassId = "ClassId1", SubjectId = "subjectId1" });

            var bankId = "bank1";
            var q1 = new Question { QuestionId = "Q1", QuestionBankId = bankId, Type = 0, Point = 5, Status = 1, Content = "", CorrectAnswer = "", CreateUser = "", Explanation = "", Options = "", SubjectId = "", UpdateUser = "" };
            var q2 = new Question { QuestionId = "Q2", QuestionBankId = bankId, Type = 1, Point = 5, Status = 1, Content = "", CorrectAnswer = "", CreateUser = "", Explanation = "", Options = "", SubjectId = "", UpdateUser = "" };
            context.QuestionBanks.Add(new QuestionBank { QuestionBankId = bankId, CreateUserId ="", SubjectId = $"subjectId1", Title = "BankMultiple", Questions = new List<Question> { q1, q2 } });
            await context.SaveChangesAsync();

            var req = CreateValidRequest(bankId, roomId, new List<string> { "Q1", "Q2" });

            var result = await service.AddExamAsync(req, userId);

            Assert.False(result.Success);
            Assert.Contains("same type", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AddExamAsync_ExamTypeMismatch_ShouldFail()
        {
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object);
            var userId = Guid.NewGuid().ToString();

            context.Users.Add(CreateUser(userId, true, true));
            var roomId = Guid.NewGuid().ToString();
            context.Rooms.Add(new Room { RoomId = roomId, RoomCode = "Room", ClassId = "", SubjectId = "" });
            var bank = CreateQuestionBank("bank1", questionType: 1);
            context.QuestionBanks.Add(bank);
            await context.SaveChangesAsync();

            var req = CreateValidRequest("bank1", roomId, bank.Questions.Select(q => q.QuestionId).ToList(), examType: 0);

            var result = await service.AddExamAsync(req, userId);

            Assert.False(result.Success);
            Assert.Contains("ExamType mismatch", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AddExamAsync_TotalPointsZero_ShouldFail()
        {
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object);
            var userId = Guid.NewGuid().ToString();

            context.Users.Add(CreateUser(userId, true, true));
            var roomId = Guid.NewGuid().ToString();
            context.Rooms.Add(new Room { RoomId = roomId, RoomCode = "Room", ClassId = "", SubjectId = "" });
            var bank = CreateQuestionBank("bank1", point: 0);
            context.QuestionBanks.Add(bank);
            await context.SaveChangesAsync();

            var req = CreateValidRequest("bank1", roomId, bank.Questions.Select(q => q.QuestionId).ToList());

            var result = await service.AddExamAsync(req, userId);

            Assert.False(result.Success);
            Assert.Contains("greater than 0", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AddExamAsync_ValidData_ShouldSucceed()
        {
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object);
            var userId = Guid.NewGuid().ToString();

            context.Users.Add(CreateUser(userId, true, true));
            var roomId = Guid.NewGuid().ToString();
            context.Rooms.Add(new Room { RoomId = roomId, RoomCode = "Room", ClassId = "", SubjectId = "" });
            var bank = CreateQuestionBank("bank1");
            context.QuestionBanks.Add(bank);
            await context.SaveChangesAsync();

            var req = CreateValidRequest("bank1", roomId, bank.Questions.Select(q => q.QuestionId).ToList());

            var result = await service.AddExamAsync(req, userId);

            Assert.True(result.Success);
            Assert.Equal(2, result.Questions.Count);
            Assert.NotNull(await context.Exams.FirstOrDefaultAsync());
        }

//----------------------------------------------------------UPDATE-------------------------------------------------------------------------------
        [Fact]
        public async Task UpdateExamAsync_UserWithoutPermission_ShouldFail()
        {
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object);
            var userId = Guid.NewGuid().ToString();
            context.Users.Add(CreateUser(userId, false, false));
            await context.SaveChangesAsync();

            var req = new UpdateExamRequest
            {
                ExamId = Guid.NewGuid().ToString(),
                Title = "Updated Title"
            };

            var result = await service.UpdateExamAsync(req, userId);

            Assert.False(result.Success);
            Assert.Contains("permission", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task UpdateExamAsync_ExamNotFound_ShouldFail()
        {
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object);
            var userId = Guid.NewGuid().ToString();
            context.Users.Add(CreateUser(userId, true, true));
            await context.SaveChangesAsync();

            var req = new UpdateExamRequest
            {
                ExamId = "nonexistentId",
                Title = "Updated Title"
            };

            var result = await service.UpdateExamAsync(req, userId);

            Assert.False(result.Success);
            Assert.Contains("not found", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task UpdateExamAsync_EndTimeBeforeStartTime_ShouldFail()
        {
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object);
            var userId = Guid.NewGuid().ToString();
            context.Users.Add(CreateUser(userId, true, true));

            var roomId = Guid.NewGuid().ToString();
            var examId = Guid.NewGuid().ToString();
            context.Rooms.Add(new Room { RoomId = roomId, RoomCode = "Room", ClassId = "classId1", SubjectId = "subjectId1" });
            context.Exams.Add(new Exam
            {
                ExamId = examId,
                RoomId = roomId,
                Title = "Old Title",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                CreateUser = userId
            });
            await context.SaveChangesAsync();

            var req = new UpdateExamRequest
            {
                ExamId = examId,
                StartTime = DateTime.UtcNow.AddHours(2),
                EndTime = DateTime.UtcNow.AddHours(1)
            };

            var result = await service.UpdateExamAsync(req, userId);

            Assert.False(result.Success);
            Assert.Contains("EndTime", result.Message);
        }

        [Fact]
        public async Task UpdateExamAsync_TitleAlreadyExists_ShouldFail()
        {
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object);
            var userId = Guid.NewGuid().ToString();
            context.Users.Add(CreateUser(userId, true, true));

            var roomId = Guid.NewGuid().ToString();
            var exam1Id = Guid.NewGuid().ToString();
            var exam2Id = Guid.NewGuid().ToString();

            context.Rooms.Add(new Room { RoomId = roomId, RoomCode = "Room", ClassId = "classId1", SubjectId = "subjectId1" });
            context.Exams.Add(new Exam
            {
                ExamId = exam1Id,
                RoomId = roomId,
                Title = "Existing Title",
                StartTime = new DateTime(2025, 8, 12, 12, 0, 0),
                EndTime = new DateTime(2025, 8, 12, 13, 0, 0),
                CreateUser = userId
            });
            context.Exams.Add(new Exam
            {
                ExamId = exam2Id,
                RoomId = roomId,
                Title = "Old Title",
                StartTime = new DateTime(2025, 8, 12, 12, 0, 0),
                EndTime = new DateTime(2025, 8, 12, 13, 0, 0),
                CreateUser = userId
            });
            await context.SaveChangesAsync();

            var req = new UpdateExamRequest
            {
                ExamId = exam2Id,
                Title = "Exist i ng Tit le",
                RoomId = roomId,
                //QuestionBankId = "someValidBankId", // bạn cần thêm ngân hàng câu hỏi hợp lệ vào context test
                StartTime = new DateTime(2025, 8, 12, 12, 0, 0),
                EndTime = new DateTime(2025, 8, 12, 13, 0, 0),
                Duration = 60,
                IsShowResult = false,
                IsShowCorrectAnswer = false,
                Status = 0,
                ExamType = 1, // ví dụ MultipleChoice
                QuestionIds = new List<string> { "validQuestionId" } // cũng cần thêm câu hỏi phù hợp vào context
            };


            var result = await service.UpdateExamAsync(req, userId);

            Assert.False(result.Success);
            Assert.Contains("An exam with the same title already exists.", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task UpdateExamAsync_ValidData_ShouldSucceed()
        {
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object);
            var userId = Guid.NewGuid().ToString();
            context.Users.Add(CreateUser(userId, true, true));

            var roomId = Guid.NewGuid().ToString();
            var examId = Guid.NewGuid().ToString();
            context.Rooms.Add(new Room { RoomId = roomId, RoomCode = "Room", ClassId = "classId1", SubjectId = "subjectId1" });

            var bankId = Guid.NewGuid().ToString();
            context.QuestionBanks.Add(new QuestionBank
            {
                QuestionBankId = bankId,
                Title = "Sample Bank",
                CreateUserId = userId,
                SubjectId = "subjectId1"
            });

            var questionId = Guid.NewGuid().ToString();
            context.Questions.Add(new Question
            {
                QuestionId = questionId,
                QuestionBankId = bankId,
                Status = 1,
                Type = 1,
                Point = 5,
                Content = "Sample question",
                CorrectAnswer = "A",
                CreateUser = userId,
                Explanation = "Explanation",
                Options = "A;B;C;D",
                SubjectId = "subjectId1",
                UpdateUser = userId
            });

            context.Exams.Add(new Exam
            {
                ExamId = examId,
                RoomId = roomId,
                Title = "Old Title",
                StartTime = new DateTime(2025, 8, 12, 12, 0, 0),
                EndTime = new DateTime(2025, 8, 12, 13, 0, 0),
                CreateUser = userId,
                Status = 0 // Đảm bảo exam ở trạng thái Inactive
            });

            await context.SaveChangesAsync();

            var req = new UpdateExamRequest
            {
                ExamId = examId,
                Title = "New Title",
                Description = "Updated Description",
                RoomId = roomId,
                QuestionBankId = bankId,
                StartTime = new DateTime(2025, 8, 12, 12, 0, 0),
                EndTime = new DateTime(2025, 8, 12, 13, 0, 0),
                Duration = 60,
                IsShowResult = true,
                IsShowCorrectAnswer = true,
                Status = 1,
                GuideLines = "Some guidelines",
                ExamType = 1,
                QuestionIds = new List<string> { questionId }
            };

            var result = await service.UpdateExamAsync(req, userId);

            Assert.True(result.Success);
            var updated = await context.Exams.FirstAsync(e => e.ExamId == examId);
            Assert.Equal("New Title", updated.Title);
            Assert.Equal("Updated Description", updated.Description);
        }


        [Fact]
        public async Task UpdateExamAsync_ExamTypeMismatch_ShouldFail()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object); var userId = Guid.NewGuid().ToString();
            var roomId = Guid.NewGuid().ToString();
            var bankId = Guid.NewGuid().ToString();

            // User with permission
            context.Users.Add(CreateUser(userId, true, true));

            // Room + Bank
            context.Rooms.Add(new Room { RoomId = roomId, RoomCode = "Room 1", ClassId = "classId1", SubjectId = "subjectId1" });
            context.QuestionBanks.Add(new QuestionBank { QuestionBankId = bankId, Title = "Bank 1", CreateUserId = "", SubjectId = "subjectId1" });

            // Question in bank
            var question = new Question
            {
                QuestionId = Guid.NewGuid().ToString(),
                QuestionBankId = bankId,
                Status = 1,
                Type = (int)QuestionTypeChoose.MultipleChoice,
                Point = 5,
                Content = "",
                CorrectAnswer = "",
                CreateUser = "",
                Explanation = "",
                Options = "",
                SubjectId = "",
                UpdateUser = ""
            };
            context.Questions.Add(question);

            // Exam with inactive status
            var exam = new Exam
            {
                ExamId = Guid.NewGuid().ToString(),
                RoomId = roomId,
                Title = "Exam Type Test",
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(2),
                CreateUser = userId,
                Status = 0 // inactive
            };
            context.Exams.Add(exam);

            await context.SaveChangesAsync();

            // Request with different ExamType than the question
            var req = new UpdateExamRequest
            {
                ExamId = exam.ExamId,
                Title = "Updated Title",
                RoomId = roomId,
                QuestionBankId = bankId,
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(2),
                Duration = 60,
                ExamType = (int)QuestionTypeChoose.Essay,
                QuestionIds = new List<string> { question.QuestionId }
            };

            // Act
            var result = await service.UpdateExamAsync(req, userId);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("ExamType mismatch", result.Message);
        }

        [Fact]
        public async Task UpdateExamAsync_TotalPointsZero_ShouldFail()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object); var userId = Guid.NewGuid().ToString();
            var roomId = Guid.NewGuid().ToString();
            var bankId = Guid.NewGuid().ToString();

            context.Users.Add(CreateUser(userId, true, true));
            context.Rooms.Add(new Room { RoomId = roomId, RoomCode = "Room 2", ClassId = "classId2", SubjectId = "subjectId2" });
            context.QuestionBanks.Add(new QuestionBank { QuestionBankId = bankId, Title = "Bank 2", SubjectId = "subjectId2", CreateUserId = "" });

            var question = new Question
            {
                QuestionId = Guid.NewGuid().ToString(),
                QuestionBankId = bankId,
                Status = 1,
                Type = (int)QuestionTypeChoose.MultipleChoice,
                Point = 0, // total points zero case
                Content = "",
                CorrectAnswer = "",
                CreateUser = "",
                Explanation = "",
                Options = "",
                SubjectId = "",
                UpdateUser = ""
            };
            context.Questions.Add(question);

            var exam = new Exam
            {
                ExamId = Guid.NewGuid().ToString(),
                RoomId = roomId,
                Title = "Zero Point Exam",
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(2),
                CreateUser = userId,
                Status = 0
            };
            context.Exams.Add(exam);

            await context.SaveChangesAsync();

            var req = new UpdateExamRequest
            {
                ExamId = exam.ExamId,
                Title = "Updated Zero Point",
                RoomId = roomId,
                QuestionBankId = bankId,
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(2),
                Duration = 60,
                ExamType = (int)QuestionTypeChoose.MultipleChoice,
                QuestionIds = new List<string> { question.QuestionId }
            };

            // Act
            var result = await service.UpdateExamAsync(req, userId);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Total point", result.Message);
        }
//----------------------------------------------------------------------VIEW DETAIL------------------------------------------------------------------------
        [Fact]
        public async Task GetExamDetailAsync_ExamExists_WithQuestions_ShouldReturnSuccess()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object); var userId = Guid.NewGuid().ToString();
            var roomId = Guid.NewGuid().ToString();
            var examId = Guid.NewGuid().ToString();
            var bankId = Guid.NewGuid().ToString();

            var user = CreateUser(userId, true, true);
            context.Users.Add(user);

            var room = new Room { RoomId = roomId, RoomCode = "ROOM-1",ClassId = "", SubjectId = "" };
            context.Rooms.Add(room);

            var bank = new QuestionBank { QuestionBankId = bankId, Title = "Bank 1", CreateUserId = "", SubjectId = "" };
            context.QuestionBanks.Add(bank);

            var question = new Question
            {
                QuestionId = Guid.NewGuid().ToString(),
                QuestionBankId = bankId,
                Content = "Sample Question",
                Status = 1,
                Type = (int)QuestionTypeChoose.MultipleChoice,
                DifficultLevel = (int)DifficultyLevel.Medium,
                Point = 5,
                CorrectAnswer = "",
                CreateUser = "",
                Explanation = "",
                Options = "",
                SubjectId = "",
                UpdateUser = "",
                QuestionBank = bank
            };
            context.Questions.Add(question);

            var exam = new Exam
            {
                ExamId = examId,
                RoomId = roomId,
                Title = "Exam 1",
                TotalQuestions = 1,
                TotalPoints = 5,
                Duration = 60,
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(2),
                CreateUser = userId,
                Status = 1,
                IsShowResult = true,
                IsShowCorrectAnswer = false,
                ExamType = (int)QuestionTypeChoose.MultipleChoice,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Room = room,
                Creator = user
            };

            exam.ExamQuestions.Add(new ExamQuestion
            {
                ExamQuestionId = Guid.NewGuid().ToString(),
                ExamId = examId,
                QuestionId = question.QuestionId,
                Question = question
            });

            context.Exams.Add(exam);
            await context.SaveChangesAsync();

            // Act
            var (success, message, detail) = await service.GetExamDetailAsync(examId, userId);

            // Assert
            Assert.True(success);
            Assert.Equal("Success", message);
            Assert.NotNull(detail);
            Assert.Single(detail!.Questions);
            Assert.Equal("Sample Question", detail.Questions[0].Content);
        }

        [Fact]
        public async Task GetExamDetailAsync_ExamExists_NoQuestions_ShouldReturnEmptyList()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object); var userId = Guid.NewGuid().ToString();
            var roomId = Guid.NewGuid().ToString();
            var examId = Guid.NewGuid().ToString();

            // Thêm user và lấy lại instance từ context
            var user = CreateUser(userId, true, true);
            context.Users.Add(user);

            var room = new Room { RoomId = roomId, RoomCode = "ROOM-EMPTY", ClassId = "", SubjectId = "" };
            context.Rooms.Add(room);

            var exam = new Exam
            {
                ExamId = examId,
                RoomId = roomId,
                Title = "Empty Exam",
                TotalQuestions = 0,
                TotalPoints = 0,
                Duration = 45,
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(2),
                CreateUser = userId,
                Status = 1,
                IsShowResult = false,
                IsShowCorrectAnswer = false,
                ExamType = (int)QuestionTypeChoose.MultipleChoice,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Room = room,
                Creator = user 
            };

            context.Exams.Add(exam);
            await context.SaveChangesAsync();

            // Act
            var (success, message, detail) = await service.GetExamDetailAsync(examId, userId);

            // Assert
            Assert.True(success);
            Assert.Equal("Success", message);
            Assert.NotNull(detail);
            Assert.Empty(detail!.Questions);
        }


        [Fact]
        public async Task GetExamDetailAsync_ExamNotFound_ShouldReturnFail()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object); var userId = Guid.NewGuid().ToString();
            context.Users.Add(CreateUser(userId, true, true));
            await context.SaveChangesAsync();

            var nonExistingExamId = Guid.NewGuid().ToString();

            // Act
            var (success, message, detail) = await service.GetExamDetailAsync(nonExistingExamId, userId);

            // Assert
            Assert.False(success);
            Assert.Equal("Exam not found.", message);
            Assert.Null(detail);
        }
//------------------------------------------------------------------------GET LIST------------------------------------------------------------------------

        [Fact]
        public async Task GetExamListAsync_UserWithoutPermission_ShouldFail()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object); var userId = Guid.NewGuid().ToString();

            // User không có quyền Lecture/Admin
            context.Users.Add(CreateUser(userId, isLecture: false, isAdmin: false));
            await context.SaveChangesAsync();

            var request = new ExamListRequest
            {
                CurrentPage = 1,
                PageSize = 10
            };

            // Act
            var (success, message, result) = await service.GetExamListAsync(request, userId);

            // Assert
            Assert.False(success);
            Assert.Contains("permission", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetExamListAsync_WithPermission_ShouldReturnAll()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object); var userId = Guid.NewGuid().ToString();

            var user = CreateUser(userId, isLecture: true, isAdmin: false);
            context.Users.Add(user);

            var room = new Room { RoomId = Guid.NewGuid().ToString(), RoomCode = "ROOM-A", ClassId = "", SubjectId = "" };
            context.Rooms.Add(room);

            var exam1 = new Exam
            {
                ExamId = Guid.NewGuid().ToString(),
                Title = "Math Exam",
                RoomId = room.RoomId,
                Room = room,
                CreateUser = userId,
                Creator = user,
                Status = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };
            var exam2 = new Exam
            {
                ExamId = Guid.NewGuid().ToString(),
                Title = "Physics Exam",
                RoomId = room.RoomId,
                Room = room,
                CreateUser = userId,
                Creator = user,
                Status = 1,
                CreatedAt = DateTime.UtcNow
            };
            context.Exams.AddRange(exam1, exam2);
            await context.SaveChangesAsync();

            var request = new ExamListRequest
            {
                CurrentPage = 1,
                PageSize = 10
            };

            // Act
            var (success, message, result) = await service.GetExamListAsync(request, userId);

            // Assert
            Assert.True(success);
            Assert.Equal("Success", message);
            Assert.NotNull(result);
            Assert.Equal(2, result.Total);
        }

        [Fact]
        public async Task GetExamListAsync_FilterByTextSearchAndIsMyQuestion_ShouldReturnFiltered()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object); var userId = Guid.NewGuid().ToString();

            var user = CreateUser(userId, isLecture: true, isAdmin: false);
            context.Users.Add(user);

            var otherUser = CreateUser(Guid.NewGuid().ToString(), isLecture: true, isAdmin: false);
            context.Users.Add(otherUser);

            var room = new Room { RoomId = Guid.NewGuid().ToString(), RoomCode = "ROOM-B", ClassId = "", SubjectId = "" };
            context.Rooms.Add(room);

            // Exam của userId (sẽ match filter)
            var myExam = new Exam
            {
                ExamId = Guid.NewGuid().ToString(),
                Title = "Chemistry Exam",
                RoomId = room.RoomId,
                Room = room,
                CreateUser = userId,
                Creator = user,
                Status = 1,
                CreatedAt = DateTime.UtcNow
            };

            // Exam của user khác (sẽ bị loại)
            var otherExam = new Exam
            {
                ExamId = Guid.NewGuid().ToString(),
                Title = "History Exam",
                RoomId = room.RoomId,
                Room = room,
                CreateUser = otherUser.UserId,
                Creator = otherUser,
                Status = 1,
                CreatedAt = DateTime.UtcNow
            };

            context.Exams.AddRange(myExam, otherExam);
            await context.SaveChangesAsync();

            var request = new ExamListRequest
            {
                CurrentPage = 1,
                PageSize = 10,
                TextSearch = "Chemistry", // match myExam
                IsMyQuestion = true,
                Status = (ExamStatus)1
            };

            // Act
            var (success, message, result) = await service.GetExamListAsync(request, userId);

            // Assert
            Assert.True(success);
            Assert.Equal("Success", message);
            Assert.NotNull(result);
            var exams = (IEnumerable<ExamListVM>)result!.Result;
            Assert.Single(exams);
            Assert.Equal("Chemistry Exam", exams.First().Title);

        }
//------------------------------------------------------------------------EXAM RESULT REPORT------------------------------------------------------------------------
        [Fact]
        public async Task GetExamResultReportAsync_UserWithoutPermission_ShouldFail()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object); var userId = Guid.NewGuid().ToString();

            // User không có quyền Lecture/Admin
            context.Users.Add(CreateUser(userId, isLecture: false, isAdmin: false));
            await context.SaveChangesAsync();

            var examId = Guid.NewGuid().ToString();

            // Act
            var (success, message, data) = await service.GetExamResultReportAsync(examId, userId);

            // Assert
            Assert.False(success);
            Assert.Contains("permission", message);
            Assert.Null(data);
        }

        [Fact]
        public async Task GetExamResultReportAsync_ExamNotFound_ShouldFail()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object); var userId = Guid.NewGuid().ToString();

            // User có quyền Lecture
            context.Users.Add(CreateUser(userId, isLecture: true, isAdmin: false));
            await context.SaveChangesAsync();

            var examId = Guid.NewGuid().ToString(); // exam không tồn tại

            // Act
            var (success, message, data) = await service.GetExamResultReportAsync(examId, userId);

            // Assert
            Assert.False(success);
            Assert.Contains("not found", message);
            Assert.Null(data);
        }

        [Fact]
        public async Task GetExamResultReportAsync_WithValidData_ShouldReturnReport()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object); var userId = Guid.NewGuid().ToString();
            var examId = Guid.NewGuid().ToString();

            // User có quyền Lecture
            var user = CreateUser(userId, isLecture: true, isAdmin: false);
            user.FullName = "Lecture User";
            context.Users.Add(user);

            // Room
            var room = new Room { RoomId = Guid.NewGuid().ToString(), RoomCode = "ROOM-C", ClassId = "", SubjectId = "" };
            context.Rooms.Add(room);

            // Exam
            var exam = new Exam
            {
                ExamId = examId,
                RoomId = room.RoomId,
                Room = room,
                Creator = user,
                CreateUser = userId,
                Title = "Biology Exam",
                Status = (int)ExamStatus.Finished,
                StartTime = DateTime.UtcNow.AddDays(-1),
                Duration = 90,
                TotalPoints = 100,
                ExamType = (int)QuestionTypeChoose.MultipleChoice,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
            };
            context.Exams.Add(exam);

            // Student users
            var studentUser1 = CreateUser(Guid.NewGuid().ToString(), isLecture: false, isAdmin: false);
            studentUser1.FullName = "Student One";
            var studentUser2 = CreateUser(Guid.NewGuid().ToString(), isLecture: false, isAdmin: false);
            studentUser2.FullName = "Student Two";
            context.Users.AddRange(studentUser1, studentUser2);

            await context.SaveChangesAsync(); // Save User trước để EF nhận biết khóa chính

            // Student exams
            context.StudentExams.AddRange(
                new StudentExam
                {
                    StudentExamId = Guid.NewGuid().ToString(),
                    ExamId = examId,
                    Exam = exam,
                    StudentId = studentUser1.UserId,  // chỉ set UserId, EF sẽ liên kết khi query
                    Score = 80,
                    Status = (int)StudentExamStatus.Passed,
                    StartTime = DateTime.UtcNow.AddHours(-2),
                    SubmitTime = DateTime.UtcNow.AddHours(-1)
                },
                new StudentExam
                {
                    StudentExamId = Guid.NewGuid().ToString(),
                    ExamId = examId,
                    Exam = exam,
                    StudentId = studentUser2.UserId,
                    Score = null,
                    Status = (int)StudentExamStatus.InProgress,
                    StartTime = DateTime.UtcNow.AddHours(-1),
                    SubmitTime = null
                });

            await context.SaveChangesAsync();

            // Act
            var (success, message, data) = await service.GetExamResultReportAsync(examId, userId);

            // Assert
            Assert.True(success);
            Assert.Equal("Success", message);
            Assert.NotNull(data);
            Assert.Equal("ROOM-C", data!.SubjectName);
            Assert.Equal("Biology Exam", exam.Title);
            Assert.Equal(2, data.TotalStudents);
            Assert.Equal(80, data.AverageScore);
            Assert.Contains(data.StudentResults, r => r.FullName == "Student One" && r.Score == 80);
            Assert.Contains(data.StudentResults, r => r.FullName == "Student Two" && r.Score == null);
        }
//------------------------------------------------------------------------EXPORT STUDENT EXAM RESULT REPORT------------------------------------------------------------------------
        [Fact]
        public async Task ExportStudentExamResultReport_WithValidData_ShouldReturnExcelStream()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object); var userId = Guid.NewGuid().ToString();
            var examId = Guid.NewGuid().ToString();

            // User có quyền Lecture
            var user = CreateUser(userId, isLecture: true, isAdmin: false);
            user.FullName = "Lecture User";
            context.Users.Add(user);

            // Room
            var room = new Room { RoomId = Guid.NewGuid().ToString(), RoomCode = "ROOM-D", ClassId = "", SubjectId = "" };
            context.Rooms.Add(room);

            // Exam
            var exam = new Exam
            {
                ExamId = examId,
                RoomId = room.RoomId,
                Room = room,
                Creator = user,
                CreateUser = userId,
                Title = "History Exam",
                Status = (int)ExamStatus.Finished,
                StartTime = DateTime.UtcNow.AddDays(-1),
                Duration = 60,
                TotalPoints = 100,
                ExamType = (int)QuestionTypeChoose.MultipleChoice,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
            };
            context.Exams.Add(exam);

            // Student users
            var studentUser1 = CreateUser(Guid.NewGuid().ToString(), isLecture: false, isAdmin: false);
            studentUser1.FullName = "Student One";
            var studentUser2 = CreateUser(Guid.NewGuid().ToString(), isLecture: false, isAdmin: false);
            studentUser2.FullName = "Student Two";
            context.Users.AddRange(studentUser1, studentUser2);

            await context.SaveChangesAsync();

            // Student exams
            context.StudentExams.AddRange(
                new StudentExam
                {
                    StudentExamId = Guid.NewGuid().ToString(),
                    ExamId = examId,
                    Exam = exam,
                    StudentId = studentUser1.UserId,
                    Score = 85,
                    Status = (int)StudentExamStatus.Passed,
                    StartTime = DateTime.UtcNow.AddHours(-3),
                    SubmitTime = DateTime.UtcNow.AddHours(-2)
                },
                new StudentExam
                {
                    StudentExamId = Guid.NewGuid().ToString(),
                    ExamId = examId,
                    Exam = exam,
                    StudentId = studentUser2.UserId,
                    Score = null,
                    Status = (int)StudentExamStatus.InProgress,
                    StartTime = DateTime.UtcNow.AddHours(-1),
                    SubmitTime = null
                });

            await context.SaveChangesAsync();

            // Act
            var (message, fileStream) = await service.ExportStudentExamResultReport(examId, userId);

            // Assert
            Assert.True(string.IsNullOrEmpty(message), $"Unexpected message: {message}");
            Assert.NotNull(fileStream);
            Assert.IsType<MemoryStream>(fileStream);

            // Optionally check fileStream length > 0 (file content exists)
            Assert.True(fileStream!.Length > 0);
        }

        [Fact]
        public async Task ExportStudentExamResultReport_UserWithoutPermission_ShouldReturnError()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object); var userId = Guid.NewGuid().ToString();
            var examId = Guid.NewGuid().ToString();

            // User không có quyền Lecture/Admin
            var user = CreateUser(userId, isLecture: false, isAdmin: false);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            var (message, fileStream) = await service.ExportStudentExamResultReport(examId, userId);

            // Assert
            Assert.Equal("User does not have permission to export exam result report.", message);
            Assert.Null(fileStream);
        }

        [Fact]
        public async Task ExportStudentExamResultReport_NoStudentResults_ShouldReturnNoResultsMessage()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object); var userId = Guid.NewGuid().ToString();
            var examId = Guid.NewGuid().ToString();

            // User có quyền Lecture
            var user = CreateUser(userId, isLecture: true, isAdmin: false);
            context.Users.Add(user);

            // Exam tồn tại nhưng không có student exam
            var exam = new Exam
            {
                ExamId = examId,
                CreateUser = userId,
                Status = (int)ExamStatus.Finished,
                ExamType = (int)QuestionTypeChoose.MultipleChoice,
                StartTime = DateTime.UtcNow.AddDays(-1),
                Duration = 60,
                TotalPoints = 100,
                RoomId = ""
            };
            context.Exams.Add(exam);
            await context.SaveChangesAsync();

            // Act
            var (message, fileStream) = await service.ExportStudentExamResultReport(examId, userId);

            // Assert
            Assert.Equal("No student exam results found.", message);
            Assert.Null(fileStream);
        }
//------------------------------------------------------------------------STUDENT EXAM DETAIL-----------------------------------------------------------------------------

        [Fact]
        public async Task GetStudentExamDetailAsync_WithValidData_ShouldReturnDetail()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object);
            var userId = Guid.NewGuid().ToString();
            var studentExamId = Guid.NewGuid().ToString();

            // User có quyền Lecture
            var lectureUser = CreateUser(userId, isLecture: true, isAdmin: false);
            lectureUser.FullName = "Lecture User";
            context.Users.Add(lectureUser);

            // Student User
            var studentUser = CreateUser(Guid.NewGuid().ToString(), isLecture: false, isAdmin: false);
            studentUser.FullName = "Student User";
            studentUser.UserCode = "S001";
            context.Users.Add(studentUser);

            // Room
            var room = new Room { RoomId = Guid.NewGuid().ToString(), RoomCode = "ROOM-E", ClassId = "SE1749", SubjectId = "SubjectId-code" };
            context.Rooms.Add(room);

            // Exam
            var exam = new Exam
            {
                ExamId = Guid.NewGuid().ToString(),
                RoomId = room.RoomId,
                Room = room,
                Title = "Math Exam",
                TotalPoints = 100,
                Duration = 60,
                StartTime = DateTime.UtcNow.AddDays(-1),
                CreateUser = ""
            };
            context.Exams.Add(exam);

            // StudentExam
            var studentExam = new StudentExam
            {
                StudentExamId = studentExamId,
                ExamId = exam.ExamId,
                Exam = exam,
                StudentId = studentUser.UserId,
                User = studentUser,
                TotalQuestions = 5,
                Score = 80,
                StartTime = DateTime.UtcNow.AddHours(-2),
                SubmitTime = DateTime.UtcNow.AddHours(-1)
            };

            // StudentAnswers
            var question1 = new Question
            {
                QuestionId = Guid.NewGuid().ToString(),
                Content = "What is 2+2?",
                Options = JsonSerializer.Serialize(new List<string> { "1", "2", "3", "4" }),
                CorrectAnswer = "4",
                Explanation = "Basic addition",
                CreateUser = lectureUser.UserId,
                QuestionBankId = "QSBank-Id",
                SubjectId = "SubjectId-code",
                UpdateUser = lectureUser.UserId
            };
            var question2 = new Question
            {
                QuestionId = Guid.NewGuid().ToString(),
                Content = "What is 3*3?",
                Options = JsonSerializer.Serialize(new List<string> { "6", "7", "8", "9" }),
                CorrectAnswer = "9",
                Explanation = "Basic multiplication",
                CreateUser = lectureUser.UserId,
                QuestionBankId = "QSBank-Id",
                SubjectId = "SubjectId-code",
                UpdateUser = lectureUser.UserId
            };

            context.Questions.AddRange(question1, question2);

            var answer1 = new StudentAnswer
            {
                StudentAnswerId = Guid.NewGuid().ToString(),
                StudentExamId = studentExamId,
                QuestionId = question1.QuestionId,
                Question = question1,
                UserAnswer = "4",
                IsCorrect = true,
                PointsEarned = 10,
                TimeSpent = 30
            };
            var answer2 = new StudentAnswer
            {
                StudentAnswerId = Guid.NewGuid().ToString(),
                StudentExamId = studentExamId,
                QuestionId = question2.QuestionId,
                Question = question2,
                UserAnswer = "7",
                IsCorrect = false,
                PointsEarned = 0,
                TimeSpent = 45
            };

            studentExam.StudentAnswers = new List<StudentAnswer> { answer1, answer2 };

            context.StudentExams.Add(studentExam);
            context.RoomUsers.Add(new RoomUser
            {
                RoomUserId = Guid.NewGuid().ToString(),
                UserId = studentUser.UserId,
                RoomId = room.RoomId
            });
            await context.SaveChangesAsync();

            // Act
            var (success, message, data) = await service.GetStudentExamDetailAsync(studentExamId, userId);

            // Assert
            Assert.True(success);
            Assert.Equal("Success", message);
            Assert.NotNull(data);

            Assert.Equal("Student User", data!.StudentName);
            Assert.Equal("ROOM-E", data.ClassName);
            Assert.Equal("S001", data.StudentCode);
            Assert.Equal("Math Exam", data.ExamTitle);
            Assert.Equal(5, data.TotalQuestions);
            Assert.Equal(100, data.TotalPoints);
            Assert.Equal(60, data.Duration);
            Assert.Equal(80, data.Score);
            Assert.Equal(1, data.CorrectAnswers);
            Assert.Equal(1, data.WrongAnswers);
            Assert.Contains(data.Answers, a => a.QuestionContent == "What is 2+2?" && a.IsCorrect);
            Assert.Contains(data.Answers, a => a.QuestionContent == "What is 3*3?" && !a.IsCorrect);
            Assert.Equal("Basic addition", data.Answers.First(a => a.QuestionContent == "What is 2+2?").Explanation);
        }

        [Fact]
        public async Task GetStudentExamDetailAsync_UserWithoutPermission_ShouldReturnError()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object);
            var userId = Guid.NewGuid().ToString();
            var studentExamId = Guid.NewGuid().ToString();

            // User không có quyền
            var normalUser = CreateUser(userId, isLecture: false, isAdmin: false);
            context.Users.Add(normalUser);
            await context.SaveChangesAsync();

            // Act
            var (success, message, data) = await service.GetStudentExamDetailAsync(studentExamId, userId);

            // Assert
            Assert.False(success);
            Assert.Equal("User does not have permission to student exam detail.", message);
            Assert.Null(data);
        }

        [Fact]
        public async Task GetStudentExamDetailAsync_StudentExamNotFound_ShouldReturnError()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object);
            var userId = Guid.NewGuid().ToString();

            // User có quyền Lecture
            var lectureUser = CreateUser(userId, isLecture: true, isAdmin: false);
            context.Users.Add(lectureUser);
            await context.SaveChangesAsync();

            // Act
            var (success, message, data) = await service.GetStudentExamDetailAsync("nonexistent-id", userId);

            // Assert
            Assert.False(success);
            Assert.Equal("Student exam not found", message);
            Assert.Null(data);
        }

//------------------------------------------------------------------------CHANGE EXAM STATUS-----------------------------------------------------------------------------

        [Fact]
        public async Task ChangeExamStatusAsync_WithValidData_ShouldChangeStatus()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object); var userId = Guid.NewGuid().ToString();
            var examId = Guid.NewGuid().ToString();

            var user = CreateUser(userId, isLecture: true, isAdmin: true);
            context.Users.Add(user);

            var exam = new Exam
            {
                ExamId = examId,
                Status = (int)ExamStatus.Draft,
                CreateUser = userId,
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                UpdateUser = userId,
                RoomId = "RoomId-code",
                StartTime = new DateTime(2025, 1, 1, 9, 0, 0, DateTimeKind.Utc), // 1 Jan 2025, 9:00 AM UTC
                EndTime = new DateTime(2025, 1, 1, 11, 0, 0, DateTimeKind.Utc)  // 1 Jan 2025, 11:00 AM UTC
            };
            context.Exams.Add(exam);

            await context.SaveChangesAsync();

            var oldUpdatedAt = exam.UpdatedAt;

            // Act
            var (success, message) = await service.ChangeExamStatusAsync(examId, (int)ExamStatus.Published, userId);

            // Assert
            Assert.True(success);
            Assert.Equal("Exam status changed to Published successfully.", message);

            var updatedExam = await context.Exams.FindAsync(examId);
            Assert.Equal((int)ExamStatus.Published, updatedExam!.Status);
            Assert.Equal(userId, updatedExam.UpdateUser);
            Assert.True(updatedExam.UpdatedAt > oldUpdatedAt);
        }
        [Fact]
        public async Task ChangeExamStatusAsync_UserWithoutPermission_ShouldReturnError()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object); var userId = Guid.NewGuid().ToString();
            var examId = Guid.NewGuid().ToString();

            // User không có quyền
            var user = CreateUser(userId, isLecture: false, isAdmin: false);
            context.Users.Add(user);

            // Exam
            var exam = new Exam { ExamId = examId, Status = (int)ExamStatus.Draft, RoomId = "RoomId-code", CreateUser = userId };
            context.Exams.Add(exam);

            await context.SaveChangesAsync();

            // Act
            var (success, message) = await service.ChangeExamStatusAsync(examId, (int)ExamStatus.Published, userId);

            // Assert
            Assert.False(success);
            Assert.Equal("User does not have permission to change exam status.", message);
        }
        [Fact]
        public async Task ChangeExamStatusAsync_ExamNotFound_ShouldReturnError()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object); var userId = Guid.NewGuid().ToString();

            // User có quyền
            var user = CreateUser(userId, isLecture: true, isAdmin: false);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            var (success, message) = await service.ChangeExamStatusAsync("non-existent-id", (int)ExamStatus.Published, userId);

            // Assert
            Assert.False(success);
            Assert.Equal("Exam not found.", message);
        }
        [Theory]
        [InlineData("Ongoing", ExamStatus.Published, "Cannot change status while the exam is ongoing.")]
        [InlineData("Completed", ExamStatus.Published, "Cannot publish an exam that has already been completed.")]
        public async Task ChangeExamStatusAsync_InvalidLiveStatus_ShouldReturnError(string liveStatusStr, ExamStatus newStatus, string expectedMessage)
        {
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object); var userId = Guid.NewGuid().ToString();
            var examId = Guid.NewGuid().ToString();

            var user = CreateUser(userId, isLecture: true, isAdmin: false);
            context.Users.Add(user);

            DateTime startTime, endTime;

            if (liveStatusStr == "Ongoing")
            {
                startTime = new DateTime(2020, 1, 1, 9, 0, 0, DateTimeKind.Utc);
                endTime = new DateTime(2100, 1, 1, 11, 0, 0, DateTimeKind.Utc);
            }
            else // Completed
            {
                startTime = new DateTime(2020, 1, 1, 9, 0, 0, DateTimeKind.Utc);
                endTime = new DateTime(2020, 1, 1, 11, 0, 0, DateTimeKind.Utc);

            }

            var exam = new Exam
            {
                ExamId = examId,
                Status = (int)ExamStatus.Published,
                CreateUser = userId,
                RoomId = "RoomId-code",
                StartTime = startTime,
                EndTime = endTime
            };
            context.Exams.Add(exam);

            await context.SaveChangesAsync();

            var (success, message) = await service.ChangeExamStatusAsync(examId, (int)newStatus, userId);

            Assert.False(success);
            Assert.Equal(expectedMessage, message);
        }
        [Fact]
        public async Task ChangeExamStatusAsync_NewStatusEqualsCurrentStatus_ShouldReturnError()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object); var userId = Guid.NewGuid().ToString();
            var examId = Guid.NewGuid().ToString();

            var user = CreateUser(userId, isLecture: true, isAdmin: false);
            context.Users.Add(user);

            var currentStatus = ExamStatus.Published;

            var exam = new Exam
            {
                ExamId = examId,
                Status = (int)currentStatus,
                CreateUser = userId,
                RoomId = "RoomId-code",
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(1).AddHours(1)
            };
            context.Exams.Add(exam);

            await context.SaveChangesAsync();

            // Act
            var (success, message) = await service.ChangeExamStatusAsync(examId, (int)currentStatus, userId);

            // Assert
            Assert.False(success);
            Assert.Equal($"Exam is already in status: {currentStatus}.", message);
        }
//------------------------------------------------------------------------EXAM GUIDE LINE-----------------------------------------------------------------------------

        [Fact]
        public async Task GetExamGuideLinesAsync_ExistingExam_ShouldReturnGuideLines()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object);
            var examId = Guid.NewGuid().ToString();
            var guideLinesText = "Exam guide lines example.";

            var exam = new Exam
            {
                ExamId = examId,
                GuildeLines = guideLinesText,
                CreateUser = "UserId",
                RoomId = "Se1749"
            };
            context.Exams.Add(exam);
            await context.SaveChangesAsync();

            // Act
            var (success, message, guideLines) = await service.GetExamGuideLinesAsync(examId, "UserId");

            // Assert
            Assert.True(success);
            Assert.Equal("Success", message);
            Assert.Equal(guideLinesText, guideLines);
        }

        [Fact]
        public async Task GetExamGuideLinesAsync_NonExistingExam_ShouldReturnNotFound()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            // Mock UnitOfWork
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1); // giả lập luôn thành công khi save
            var service = new ExamService(context, mockUnitOfWork.Object);
            var nonExistentExamId = Guid.NewGuid().ToString();

            // Act
            var (success, message, guideLines) = await service.GetExamGuideLinesAsync(nonExistentExamId, "anyUserId");

            // Assert
            Assert.False(success);
            Assert.Equal("Exam not found.", message);
            Assert.Null(guideLines);
        }

    }
}
