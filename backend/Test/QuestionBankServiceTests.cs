using Xunit;
using Moq;
using System.Threading.Tasks;
using API.Services;
using API.ViewModels;
using Microsoft.Extensions.Logging;
using API.Models;
using Xunit.Abstractions;
using API.Helper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace TestQuestionBankServicePackage
{
    public class QuestionBankServiceTests : IDisposable
    {
        private readonly QuestionBankService _service;
        private readonly Sep490Context _context;
        private readonly ILogger<QuestionBankService> _logger;

        public QuestionBankServiceTests()
        {
            var options = new DbContextOptionsBuilder<Sep490Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new Sep490Context(options);

            var loggerMock = new Mock<ILogger<QuestionBankService>>();
            _logger = loggerMock.Object;

            _service = new QuestionBankService(_context, _logger);
        }
        // ADD

        [Fact]
        public async Task AddQuestionBankAsync_Should_Return_Fail_When_User_Has_No_Permission()
        {
            // Arrange
            var request = new AddQuestionBankRequest
            {
                Title = "Math",
                SubjectId = "SUB1"
            };

            var userId = "UserId1";

            // Act
            var result = await _service.AddQuestionBankAsync(request, userId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("You do not have permission to perform this action.", result.Message);
        }

        [Fact]
        public async Task AddQuestionBankAsync_Should_Return_Fail_When_Subject_Not_Found()
        {
            // Arrange
            var user = new User { UserId = "user1" };
            user.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture, UserId = "user1" });
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var request = new AddQuestionBankRequest
            {
                Title = "Math",
                SubjectId = "InvalidSubject"
            };

            // Act
            var result = await _service.AddQuestionBankAsync(request, "user1");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Subject not found. Please select a valid subject.", result.Message);
        }

        [Fact]
        public async Task AddQuestionBankAsync_Should_Return_Success_When_Valid_Data()
        {
            // Arrange
            var user = new User { UserId = "User2" };
            user.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture, UserId = "User2" });
            await _context.Users.AddAsync(user);

            var subject = new Subject { SubjectId = "SUB1", SubjectName = "Math", SubjectCode = "MAS101" };
            await _context.Subjects.AddAsync(subject);

            await _context.SaveChangesAsync();

            var request = new AddQuestionBankRequest
            {
                Title = "Math",
                SubjectId = "SUB1",
                Description = "Sample Description"
            };

            // Act
            var result = await _service.AddQuestionBankAsync(request, "User2");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Question bank has been created successfully.", result.Message);
        }

        // EDIT
        [Fact]
        public async Task EditQuestionBankAsync_UserNotAllowed_ReturnsError()
        {
            // Arrange
            var userId = "invalid-user";

            // Act
            var result = await _service.EditQuestionBankAsync("qb1", "Test Title", "sub1", "desc", userId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("You do not have permission to perform this action.", result.Message);
        }

        [Theory]
        [InlineData(null, "sub1")]
        [InlineData("  ", "sub1")]
        [InlineData("title", null)]
        [InlineData("title", " ")]
        public async Task EditQuestionBankAsync_MissingTitleOrSubject_ReturnsError(string title, string subjectId)
        {
            // Arrange
            var user = new User { UserId = "user1" };
            user.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture, UserId = "user1" });
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.EditQuestionBankAsync("qb1", title, subjectId, "desc", "user1");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Title and SubjectId are required.", result.Message);
        }

        [Fact]
        public async Task EditQuestionBankAsync_SubjectNotFound_ReturnsError()
        {
            // Arrange
            var user = new User { UserId = "user1" };
            user.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture, UserId = "user1" });
            await _context.Users.AddAsync(user);
            var subject = new Subject { SubjectId = "SUB1", SubjectName = "Math", SubjectCode = "MAS101" };
            await _context.Subjects.AddAsync(subject);

            await _context.SaveChangesAsync();

            // Act
            var result = await _service.EditQuestionBankAsync("qb1", "Test Title", "sub1", "desc", "user1");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Subject not found.", result.Message);
        }

        [Fact]
        public async Task EditQuestionBankAsync_QuestionBankNotFound_ReturnsError()
        {
            // Arrange
            var user = new User { UserId = "user1" };
            user.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture, UserId = "user1" });
            await _context.Users.AddAsync(user);
            var subject = new Subject { SubjectId = "SUB1", SubjectName = "Math", SubjectCode = "MAS101" };
            await _context.Subjects.AddAsync(subject);
            var QuestionBank = new QuestionBank { QuestionBankId = "QB1", SubjectId = "SUB1", Title = "Test", CreateUserId = "user1" };
            await _context.QuestionBanks.AddAsync(QuestionBank);

            await _context.SaveChangesAsync();

            // Act
            var result = await _service.EditQuestionBankAsync("QB2", "Test Title", "SUB1", "desc", "user1");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Question bank not found.", result.Message);
        }

        [Fact]
        public async Task EditQuestionBankAsync_UserNotCreatorNoWriteAccess_ReturnsError()
        {
            // Arrange

            var user = new User { UserId = "user1" };
            user.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture, UserId = "user1" });
            await _context.Users.AddAsync(user);
            var subject = new Subject { SubjectId = "SUB1", SubjectName = "Math", SubjectCode = "MAS101" };
            await _context.Subjects.AddAsync(subject);
            var QuestionBank = new QuestionBank { QuestionBankId = "QB1", SubjectId = "SUB1", Title = "Test Title 1", CreateUserId = "user1" };
            await _context.QuestionBanks.AddAsync(QuestionBank);

            await _context.SaveChangesAsync();

            // Act
            var result = await _service.EditQuestionBankAsync("QB1", "Test Title", "SUB1", "desc", "user2");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("You do not have permission to perform this action.", result.Message);
        }

        [Fact]
        public async Task EditQuestionBankAsync_UserHasOnlyViewAccess_ReturnsError()
        {
            // Arrange
            var user = new User { UserId = "user1" };
            user.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture, UserId = "user1" });
            await _context.Users.AddAsync(user);
            var subject = new Subject { SubjectId = "SUB1", SubjectName = "Math", SubjectCode = "MAS101" };
            await _context.Subjects.AddAsync(subject);
            var QuestionBank = new QuestionBank { QuestionBankId = "QB1", SubjectId = "SUB1", Title = "Test", CreateUserId = "user2" };
            await _context.QuestionBanks.AddAsync(QuestionBank);
            var share = new QuestionShare { QuestionShareId = "QS1" , QuestionBankId = "QB1", SharedWithUserId = "user1", AccessMode = 0 };
            await _context.QuestionShares.AddAsync(share);
            
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.EditQuestionBankAsync("QB1", "Test Title", "SUB1", "desc", "user1");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("You only have view permission and cannot edit this question bank.", result.Message);
        }

        [Fact]
        public async Task EditQuestionBankAsync_ValidUpdate_Success()
        {
            // Arrange
            var user = new User { UserId = "user1" };
            user.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture, UserId = "user1" });
            await _context.Users.AddAsync(user);
            var subject = new Subject { SubjectId = "SUB1", SubjectName = "Math", SubjectCode = "MAS101" };
            await _context.Subjects.AddAsync(subject);
            var QuestionBank = new QuestionBank { QuestionBankId = "QB1", SubjectId = "SUB1", Title = "Title", CreateUserId = "user1" };
            await _context.QuestionBanks.AddAsync(QuestionBank);

            await _context.SaveChangesAsync();

            // Act
            var result = await _service.EditQuestionBankAsync("QB1", "Test Title", "SUB1", "desc", "user1");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Question bank updated successfully.", result.Message);
        }
        //Deactive/Active

        [Fact]
        public async Task ToggleQuestionBankStatusAsync_UserNotAllowed_ReturnsError()
        {
            // Act
            var result = await _service.ToggleQuestionBankStatusAsync("qb1", "invalid-user");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("You do not have permission to perform this action.", result.Message);
        }

        [Fact]
        public async Task ToggleQuestionBankStatusAsync_QuestionBankNotFound_ReturnsError()
        {
            // Arrange
            var user = new User { UserId = "user1" };
            user.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture });
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.ToggleQuestionBankStatusAsync("invalid-qb", "user1");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Question bank not found.", result.Message);
        }


        [Fact]
        public async Task ToggleQuestionBankStatusAsync_NotCreator_ReturnsError()
        {
            // Arrange
            var user = new User { UserId = "user2" };
            user.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture });
            await _context.Users.AddAsync(user);

            var qb = new QuestionBank { QuestionBankId = "qb2", CreateUserId = "other-user", Status = 1, SubjectId = "", Title = "" };
            await _context.QuestionBanks.AddAsync(qb);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.ToggleQuestionBankStatusAsync("qb2", "user2");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("You are not the creator of this question bank, so you cannot Deactive/Active it.", result.Message);
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(0, 1)]
        public async Task ToggleQuestionBankStatusAsync_ValidRequest_TogglesStatus(int initialStatus, int expectedStatus)
        {
            // Arrange
            var user = new User { UserId = "user3" };
            user.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture });
            await _context.Users.AddAsync(user);

            var qb = new QuestionBank { QuestionBankId = "qb3", CreateUserId = "user3", Status = initialStatus, SubjectId = "", Title = "" };
            await _context.QuestionBanks.AddAsync(qb);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.ToggleQuestionBankStatusAsync("qb3", "user3");
            var updatedQb = await _context.QuestionBanks.FindAsync("qb3");

            // Assert
            Assert.True(result.Success);
            Assert.Equal($"Question bank change to {(ActiveStatus)expectedStatus} successfully.", result.Message);
            Assert.Equal(expectedStatus, updatedQb.Status);
        }

        // Get List
        //1 User không có quyền
        //2 Không tìm thấy dữ liệu
        //3 Truy xuất đúng dữ liệu khi có
        //4 Tìm kiếm theo từ khóa hoạt động
        [Fact]
        public async Task GetList_UserNotAllowed_ReturnsPermissionError()
        {
            // Arrange
            var filter = new QuestionBankFilterVM { CurrentPage = 1, PageSize = 10 };

            // Act
            var (message, result) = await _service.GetList(filter, "invalid-user");

            // Assert
            Assert.Equal("You do not have permission to perform this action.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetList_NoDataFound_ReturnsNoFoundMessage()
        {
            // Arrange
            var user = new User { UserId = "user1", FullName = "User One" };
            user.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture });
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var filter = new QuestionBankFilterVM { CurrentPage = 1, PageSize = 10 };

            // Act
            var (message, result) = await _service.GetList(filter, "user1");

            // Assert
            Assert.Equal("No found any question bank", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetList_WithMultipleData_ReturnsPagedResult()
        {
            // Arrange
            var user = new User { UserId = "user2", FullName = "User Two" };
            user.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture });
            await _context.Users.AddAsync(user);

            var subject = new Subject { SubjectId = "sub1", SubjectName = "Math", SubjectCode = "SubjectCode" };
            await _context.Subjects.AddAsync(subject);

            var qb1 = new QuestionBank
            {
                QuestionBankId = "qb1",
                Title = "Sample QB 1",
                SubjectId = "sub1",
                CreateUserId = "user2",
                Status = 1,
                CreatedAt = DateTime.UtcNow
            };
            var qb2 = new QuestionBank
            {
                QuestionBankId = "qb2",
                Title = "Sample QB 2",
                SubjectId = "sub1",
                CreateUserId = "user2",
                Status = 1,
                CreatedAt = DateTime.UtcNow
            };

            await _context.QuestionBanks.AddRangeAsync(qb1, qb2);

            var question1 = new Question
            {
                QuestionId = "q1",
                QuestionBankId = "qb1",
                Type = (int)QuestionTypeChoose.MultipleChoice,
                Content = "What is 2 + 2?",
                CorrectAnswer = "4",
                CreateUser = "user2",
                Explanation = "Basic addition",
                Options = JsonConvert.SerializeObject(new List<string> { "3", "4", "5" }),
                SubjectId = "sub1",
                UpdateUser = "",
            };
            var question2 = new Question
            {
                QuestionId = "q2",
                QuestionBankId = "qb2",
                Type = (int)QuestionTypeChoose.Essay,
                Content = "What is 2 + 2?",
                CorrectAnswer = "4",
                CreateUser = "user2",
                Explanation = "Basic addition",
                Options = JsonConvert.SerializeObject(new List<string> { "3", "4", "5" }),
                SubjectId = "sub1",
                UpdateUser = "",
            };

            await _context.Questions.AddRangeAsync(question1, question2);

            await _context.SaveChangesAsync();

            var filter = new QuestionBankFilterVM { CurrentPage = 1, PageSize = 10 };

            // Act
            var (message, result) = await _service.GetList(filter, "user2");
            var list = Assert.IsType<List<QuestionBankListVM>>(result.Result);

            // Assert
            Assert.Equal("", message);
            Assert.NotNull(result);
            Assert.Equal(2, list.Count);
            Assert.Equal(2, result.TotalQuestionBanks);
            Assert.Equal(2, result.TotalQuestionsQB);
            Assert.Equal(1, result.TotalSubjects);
            Assert.Equal(0, result.TotalSharedQB);
        }


        [Fact]
        public async Task GetList_WithSearchKeyword_FiltersCorrectly()
        {
            // Arrange
            var user = new User { UserId = "user3", FullName = "User Three" };
            user.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture });
            await _context.Users.AddAsync(user);

            var subject = new Subject { SubjectId = "sub2", SubjectName = "Science", SubjectCode = "SubjectCode" };
            await _context.Subjects.AddAsync(subject);

            var qb = new QuestionBank
            {
                QuestionBankId = "qb2",
                Title = "Physics Basics",
                SubjectId = "sub2",
                CreateUserId = "user3",
                Status = 1,
                CreatedAt = DateTime.UtcNow
            };
            await _context.QuestionBanks.AddAsync(qb);
            await _context.SaveChangesAsync();

            var filter = new QuestionBankFilterVM
            {
                CurrentPage = 1,
                PageSize = 10,
                TextSearch = "Physics"
            };

            // Act
            var (message, result) = await _service.GetList(filter, "user3");
            var list = Assert.IsType<List<QuestionBankListVM>>(result.Result);

            // Assert
            Assert.Equal("", message);
            Assert.NotNull(result);
            Assert.Single(list);
            Assert.Contains("Physics", list[0].Title);
        }

        //DELETE
        //1 User không có quyền
        //2 Không tìm thấy question bank
        //3 Không phải creator
        //4 Có question trong bank
        //5 Xóa thành công
        [Fact]
        public async Task DeleteQuestionBankAsync_UserNotAllowed_ReturnsError()
        {
            // Act
            var result = await _service.DeleteQuestionBankAsync("qb1", "sub1", "invalid-user");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("You do not have permission to perform this action.", result.Message);
        }

        [Fact]
        public async Task DeleteQuestionBankAsync_QuestionBankNotFound_ReturnsError()
        {
            // Arrange
            var user = new User { UserId = "user1" };
            user.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture });
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.DeleteQuestionBankAsync("invalid-qb", "sub1", "user1");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Question bank not found or not match subjectId.", result.Message);
        }

        [Fact]
        public async Task DeleteQuestionBankAsync_NotCreator_ReturnsError()
        {
            // Arrange
            var user = new User { UserId = "user2" };
            user.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture });
            await _context.Users.AddAsync(user);

            var qb = new QuestionBank
            {
                QuestionBankId = "qb2",
                Title = "Test QB",
                SubjectId = "sub1",
                CreateUserId = "other-user"
            };
            await _context.QuestionBanks.AddAsync(qb);

            await _context.SaveChangesAsync();

            // Act
            var result = await _service.DeleteQuestionBankAsync("qb2", "sub1", "user2");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("You are not the creator of this Question Bank and therefore cannot delete it.", result.Message);
        }

        [Fact]
        public async Task DeleteQuestionBankAsync_HasQuestions_ReturnsError()
        {
            // Arrange
            var user = new User { UserId = "user3" };
            user.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture });
            await _context.Users.AddAsync(user);
            var qb = new QuestionBank
            {
                QuestionBankId = "qb3",
                Title = "Test QB",
                SubjectId = "sub1",
                CreateUserId = "user3"
            };
            await _context.QuestionBanks.AddAsync(qb);

            var question = new Question
            {
                QuestionId = "q1",
                QuestionBankId = "qb3",
                Content = "2 + 2",
                CorrectAnswer = "4",
                CreateUser = "user3",
                Explanation = "",
                Options = JsonConvert.SerializeObject(new List<string> { "3", "4", "5" }),
                SubjectId = "sub1",
                UpdateUser = "",
                Type = (int)QuestionTypeChoose.MultipleChoice
            };
            await _context.Questions.AddAsync(question);

            await _context.SaveChangesAsync();

            // Act
            var result = await _service.DeleteQuestionBankAsync("qb3", "sub1", "user3");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Cannot delete question bank with existing questions.", result.Message);
        }

        [Fact]
        public async Task DeleteQuestionBankAsync_ValidRequest_DeletesSuccessfully()
        {
            // Arrange
            var user = new User { UserId = "user4" };
            user.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture });
            await _context.Users.AddAsync(user);

            var qb = new QuestionBank
            {
                QuestionBankId = "qb4",
                Title = "Test QB",
                SubjectId = "sub1",
                CreateUserId = "user4"
            };
            await _context.QuestionBanks.AddAsync(qb);

            await _context.SaveChangesAsync();

            // Act
            var result = await _service.DeleteQuestionBankAsync("qb4", "sub1", "user4");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Question bank deleted successfully.", result.Message);

            var qbInDb = await _context.QuestionBanks.FirstOrDefaultAsync(q => q.QuestionBankId == "qb4");
            Assert.Null(qbInDb);
        }
        // View Detail
        //1 Lấy thành công dữ liệu
        //2 User không có quyền
        //3 Không tìm thấy question bank
        //4 Không có câu hỏi trong question bank

        [Fact]
        public async Task GetQuestionBankDetail_WithValidData_ReturnsDetail()
        {
            // Arrange
            var user = new User { UserId = "user5", FullName = "User Five" };
            user.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture });
            await _context.Users.AddAsync(user);

            var subject = new Subject { SubjectId = "sub5", SubjectName = "History", SubjectCode = "SubjectCode" };
            await _context.Subjects.AddAsync(subject);

            var questionBank = new QuestionBank
            {
                QuestionBankId = "qb5",
                Title = "History Basics",
                SubjectId = "sub5",
                CreateUserId = "user5",
                Status = 1,
                CreatedAt = DateTime.UtcNow
            };
            await _context.QuestionBanks.AddAsync(questionBank);

            var question = new Question
            {
                QuestionId = "q5",
                QuestionBankId = "qb5",
                SubjectId = "sub5",
                Content = "Who discovered America?",
                Type = (int)QuestionTypeChoose.MultipleChoice,
                DifficultLevel = 1,
                Point = 1,
                Options = JsonConvert.SerializeObject(new List<string> { "Columbus", "Magellan" }),
                CorrectAnswer = "Columbus",
                CreateUser = "user5",
                Explanation = "",
                UpdateUser = ""
            };
            await _context.Questions.AddAsync(question);

            await _context.SaveChangesAsync();

            // Act
            var (success, message, data) = await _service.GetQuestionBankDetailAsync("qb5", "user5");

            // Assert
            Assert.True(success);
            Assert.Equal("Success", message);
            Assert.NotNull(data);
            Assert.Equal("History Basics", data.QuestionBankName);
            Assert.Single(data.Questions);
            Assert.Equal(1, data.MultipleChoiceCount);
            Assert.Equal(0, data.EssayCount);
            Assert.Equal("Who discovered America?", data.Questions[0].Content);
        }

        [Fact]
        public async Task GetQuestionBankDetail_UserWithoutPermission_ReturnsError()
        {
            // Arrange
            var user = new User { UserId = "user6", FullName = "User Six" };
            await _context.Users.AddAsync(user); // Không thêm role

            await _context.SaveChangesAsync();

            // Act
            var (success, message, data) = await _service.GetQuestionBankDetailAsync("anyQBId", "user6");

            // Assert
            Assert.False(success);
            Assert.Equal("You do not have permission to perform this action.", message);
            Assert.Null(data);
        }

        [Fact]
        public async Task GetQuestionBankDetail_QuestionBankNotFound_ReturnsError()
        {
            // Arrange
            var user = new User { UserId = "user7", FullName = "User Seven" };
            user.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture });
            await _context.Users.AddAsync(user);

            await _context.SaveChangesAsync();

            // Act
            var (success, message, data) = await _service.GetQuestionBankDetailAsync("nonexistentQB", "user7");

            // Assert
            Assert.False(success);
            Assert.Equal("Question bank not found.", message);
            Assert.Null(data);
        }

        [Fact]
        public async Task GetQuestionBankDetail_QuestionBankWithNoQuestions_ReturnsEmptyList()
        {
            // Arrange
            var user = new User { UserId = "user8", FullName = "User Eight" };
            user.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture });
            await _context.Users.AddAsync(user);

            var subject = new Subject { SubjectId = "sub8", SubjectName = "Geography", SubjectCode = "SubjectCode" };
            await _context.Subjects.AddAsync(subject);

            var questionBank = new QuestionBank
            {
                QuestionBankId = "qb8",
                Title = "Geography Basics",
                SubjectId = "sub8",
                CreateUserId = "user8",
                Status = 1,
                CreatedAt = DateTime.UtcNow
            };
            await _context.QuestionBanks.AddAsync(questionBank);

            await _context.SaveChangesAsync();

            // Act
            var (success, message, data) = await _service.GetQuestionBankDetailAsync("qb8", "user8");

            // Assert
            Assert.True(success);
            Assert.Equal("Success", message);
            Assert.NotNull(data);
            Assert.Equal(0, data.TotalQuestions);
            Assert.Empty(data.Questions);
        }

        //Share Question Bank
        //1 Thành công
        //2 Sharer không có quyền
        //3 QuestionBank không tồn tại
        //4 Không phải người tạo
        //5 Target User không tồn tại
        //6 Target User không có quyền
        //7 Đã chia sẻ trước đó

        // EmailHandler is static, can not inject throught DI contaniner so that can use mock in unit test 
        //[Fact]
        //public async Task ShareQuestionBank_ValidData_Success()
        //{
        //    // Arrange
        //    var sharer = new User { UserId = "userShare1", FullName = "Sharer", Email = "sharer@test.com" };
        //    sharer.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture });
        //    await _context.Users.AddAsync(sharer);

        //    var targetUser = new User { UserId = "userTarget1", FullName = "Target", Email = "target@test.com" };
        //    targetUser.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture });
        //    await _context.Users.AddAsync(targetUser);

        //    var qb = new QuestionBank
        //    {
        //        QuestionBankId = "qbShare1",
        //        Title = "Shared QB",
        //        SubjectId = "sub1",
        //        CreateUserId = "userShare1",
        //        Status = 1
        //    };
        //    await _context.QuestionBanks.AddAsync(qb);

        //    await _context.SaveChangesAsync();

        //    // Act
        //    var (success, message) = await _service.ShareQuestionBankAsync("qbShare1", "target@test.com", "userShare1", 1);

        //    // Assert
        //    Assert.True(success);
        //    Assert.Equal("Question bank shared successfully.", message);
        //    Assert.True(await _context.QuestionShares.AnyAsync(s => s.QuestionBankId == "qbShare1" && s.SharedWithUserId == "userTarget1"));
        //}

        [Fact]
        public async Task ShareQuestionBank_SharerWithoutPermission_Fails()
        {
            // Arrange
            var sharer = new User { UserId = "userShare2", FullName = "NoPermission", Email = "nopermission@test.com" };
            await _context.Users.AddAsync(sharer);

            await _context.SaveChangesAsync();

            // Act
            var (success, message) = await _service.ShareQuestionBankAsync("anyQb", "target@test.com", "userShare2", 1);

            // Assert
            Assert.False(success);
            Assert.Equal("You do not have permission to perform this action.", message);
        }

        [Fact]
        public async Task ShareQuestionBank_QuestionBankNotFound_Fails()
        {
            // Arrange
            var sharer = new User { UserId = "userShare3", FullName = "Sharer", Email = "sharer3@test.com" };
            sharer.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture });
            await _context.Users.AddAsync(sharer);

            await _context.SaveChangesAsync();

            // Act
            var (success, message) = await _service.ShareQuestionBankAsync("invalidQb", "target@test.com", "userShare3", 1);

            // Assert
            Assert.False(success);
            Assert.Equal("Question bank not found.", message);
        }

        [Fact]
        public async Task ShareQuestionBank_NotOwner_Fails()
        {
            // Arrange
            var sharer = new User { UserId = "userShare4", FullName = "Sharer", Email = "sharer4@test.com" };
            sharer.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture });
            await _context.Users.AddAsync(sharer);

            var qb = new QuestionBank
            {
                QuestionBankId = "qbShare4",
                Title = "Owned by Someone Else",
                SubjectId = "sub1",
                CreateUserId = "otherUser",
                Status = 1
            };
            await _context.QuestionBanks.AddAsync(qb);

            await _context.SaveChangesAsync();

            // Act
            var (success, message) = await _service.ShareQuestionBankAsync("qbShare4", "target@test.com", "userShare4", 1);

            // Assert
            Assert.False(success);
            Assert.Equal("You do not have permission to share this question bank.", message);
        }

        [Fact]
        public async Task ShareQuestionBank_TargetUserNotFound_Fails()
        {
            // Arrange
            var sharer = new User { UserId = "userShare5", FullName = "Sharer", Email = "sharer5@test.com" };
            sharer.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture });
            await _context.Users.AddAsync(sharer);

            var qb = new QuestionBank
            {
                QuestionBankId = "qbShare5",
                Title = "QB5",
                SubjectId = "sub1",
                CreateUserId = "userShare5",
                Status = 1
            };
            await _context.QuestionBanks.AddAsync(qb);

            await _context.SaveChangesAsync();

            // Act
            var (success, message) = await _service.ShareQuestionBankAsync("qbShare5", "nonexistent@test.com", "userShare5", 1);

            // Assert
            Assert.False(success);
            Assert.Equal("The user you are trying to share with does not exist.", message);
        }

        [Fact]
        public async Task ShareQuestionBank_TargetUserWithoutPermission_Fails()
        {
            // Arrange
            var sharer = new User { UserId = "userShare6", FullName = "Sharer", Email = "sharer6@test.com" };
            sharer.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture });
            await _context.Users.AddAsync(sharer);

            var targetUser = new User { UserId = "userTarget6", FullName = "Target", Email = "target6@test.com" }; // Không có role
            await _context.Users.AddAsync(targetUser);

            var qb = new QuestionBank
            {
                QuestionBankId = "qbShare6",
                Title = "QB6",
                SubjectId = "sub1",
                CreateUserId = "userShare6",
                Status = 1
            };
            await _context.QuestionBanks.AddAsync(qb);

            await _context.SaveChangesAsync();

            // Act
            var (success, message) = await _service.ShareQuestionBankAsync("qbShare6", "target6@test.com", "userShare6", 1);

            // Assert
            Assert.False(success);
            Assert.Equal("The user you are trying to share with does not have permission to receive shared question banks.", message);
        }

        [Fact]
        public async Task ShareQuestionBank_AlreadyShared_Fails()
        {
            // Arrange
            var sharer = new User { UserId = "userShare7", FullName = "Sharer", Email = "sharer7@test.com" };
            sharer.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture });
            await _context.Users.AddAsync(sharer);

            var targetUser = new User { UserId = "userTarget7", FullName = "Target", Email = "target7@test.com" };
            targetUser.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture });
            await _context.Users.AddAsync(targetUser);

            var qb = new QuestionBank
            {
                QuestionBankId = "qbShare7",
                Title = "QB7",
                SubjectId = "sub1",
                CreateUserId = "userShare7",
                Status = 1
            };
            await _context.QuestionBanks.AddAsync(qb);

            var share = new QuestionShare
            {
                QuestionShareId = Guid.NewGuid().ToString(),
                QuestionBankId = "qbShare7",
                SharedWithUserId = "userTarget7",
                AccessMode = 1
            };
            await _context.QuestionShares.AddAsync(share);

            await _context.SaveChangesAsync();

            // Act
            var (success, message) = await _service.ShareQuestionBankAsync("qbShare7", "target7@test.com", "userShare7", 1);

            // Assert
            Assert.False(success);
            Assert.Equal("This question bank has already been shared with this user.", message);
        }

        //Change Access Mode
        // 1.Quyền truy cập được cập nhật thành công.
        // 2.Người chia sẻ không có quyền thực hiện hành động.
        // 3.Không tìm thấy ngân hàng câu hỏi.
        // 4.Người chia sẻ không phải là người tạo ngân hàng câu hỏi.
        // 5.Không tìm thấy bản ghi chia sẻ tương ứng giữa ngân hàng và người dùng.
        // 6.AccessMode không hợp lệ.
        [Fact]
        public async Task ChangeAccessMode_ValidData_Success()
        {
            // Arrange
            var sharer = new User { UserId = "userChange1", FullName = "Sharer", Email = "sharer1@test.com" };
            sharer.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture });
            await _context.Users.AddAsync(sharer);

            var targetUser = new User { UserId = "userTargetChange1", FullName = "Target", Email = "target1@test.com" };
            targetUser.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture });
            await _context.Users.AddAsync(targetUser);

            var qb = new QuestionBank
            {
                QuestionBankId = "qbChange1",
                Title = "QB Change 1",
                SubjectId = "sub1",
                CreateUserId = "userChange1"
            };
            await _context.QuestionBanks.AddAsync(qb);

            var share = new QuestionShare
            {
                QuestionShareId = Guid.NewGuid().ToString(),
                QuestionBankId = "qbChange1",
                SharedWithUserId = "userTargetChange1",
                AccessMode = 1
            };
            await _context.QuestionShares.AddAsync(share);
            await _context.SaveChangesAsync();

            // Act
            var (success, message) = await _service.ChangeAccessModeAsync("qbChange1", "target1@test.com", "userChange1", 0);

            // Assert
            Assert.True(success);
            Assert.Equal("Access mode updated successfully.", message);

            var updatedShare = await _context.QuestionShares
                .FirstOrDefaultAsync(qs => qs.QuestionBankId == "qbChange1" && qs.SharedWithUserId == "userTargetChange1");
            Assert.Equal(0, updatedShare.AccessMode);
        }

        [Fact]
        public async Task ChangeAccessMode_SharerWithoutPermission_Fails()
        {
            // Arrange
            var sharer = new User { UserId = "userChange2", FullName = "NoPermission", Email = "nopermission@test.com" };
            await _context.Users.AddAsync(sharer);

            await _context.SaveChangesAsync();

            // Act
            var (success, message) = await _service.ChangeAccessModeAsync("anyQb", "anytarget@test.com", "userChange2", 1);

            // Assert
            Assert.False(success);
            Assert.Equal("You do not have permission to perform this action.", message);
        }

        [Fact]
        public async Task ChangeAccessMode_QuestionBankNotFound_Fails()
        {
            // Arrange
            var sharer = new User { UserId = "userChange3", FullName = "Sharer", Email = "sharer3@test.com" };
            sharer.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Admin });
            await _context.Users.AddAsync(sharer);
            await _context.SaveChangesAsync();

            // Act
            var (success, message) = await _service.ChangeAccessModeAsync("invalidQb", "target@test.com", "userChange3", 1);

            // Assert
            Assert.False(success);
            Assert.Equal("Question bank not found.", message);
        }

        [Fact]
        public async Task ChangeAccessMode_NotOwner_Fails()
        {
            // Arrange
            var sharer = new User { UserId = "userChange4", FullName = "Sharer", Email = "sharer4@test.com" };
            sharer.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture });
            await _context.Users.AddAsync(sharer);

            var qb = new QuestionBank
            {
                QuestionBankId = "qbChange4",
                Title = "Not Owned",
                SubjectId = "sub1",
                CreateUserId = "otherUser"
            };
            await _context.QuestionBanks.AddAsync(qb);

            await _context.SaveChangesAsync();

            // Act
            var (success, message) = await _service.ChangeAccessModeAsync("qbChange4", "someone@test.com", "userChange4", 1);

            // Assert
            Assert.False(success);
            Assert.Equal("You do not have permission to modify this share.", message);
        }

        [Fact]
        public async Task ChangeAccessMode_ShareRecordNotFound_Fails()
        {
            // Arrange
            var sharer = new User { UserId = "userChange5", FullName = "Sharer", Email = "sharer5@test.com" };
            sharer.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Admin });
            await _context.Users.AddAsync(sharer);

            var targetUser = new User { UserId = "userTargetChange5", FullName = "Target", Email = "target5@test.com" };
            await _context.Users.AddAsync(targetUser);

            var qb = new QuestionBank
            {
                QuestionBankId = "qbChange5",
                Title = "QB Change 5",
                SubjectId = "sub1",
                CreateUserId = "userChange5"
            };
            await _context.QuestionBanks.AddAsync(qb);

            await _context.SaveChangesAsync();

            // Act
            var (success, message) = await _service.ChangeAccessModeAsync("qbChange5", "target5@test.com", "userChange5", 1);

            // Assert
            Assert.False(success);
            Assert.Equal("Share record not found.", message);
        }

        [Fact]
        public async Task ChangeAccessMode_InvalidAccessMode_Fails()
        {
            // Arrange
            var sharer = new User { UserId = "userChange6", FullName = "Sharer", Email = "sharer6@test.com" };
            sharer.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture });
            await _context.Users.AddAsync(sharer);

            var targetUser = new User { UserId = "userTargetChange6", FullName = "Target", Email = "target6@test.com" };
            targetUser.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture });
            await _context.Users.AddAsync(targetUser);

            var qb = new QuestionBank
            {
                QuestionBankId = "qbChange6",
                Title = "QB Change 6",
                SubjectId = "sub1",
                CreateUserId = "userChange6"
            };
            await _context.QuestionBanks.AddAsync(qb);

            var share = new QuestionShare
            {
                QuestionShareId = Guid.NewGuid().ToString(),
                QuestionBankId = "qbChange6",
                SharedWithUserId = "userTargetChange6",
                AccessMode = 1
            };
            await _context.QuestionShares.AddAsync(share);
            await _context.SaveChangesAsync();

            // Act
            var (success, message) = await _service.ChangeAccessModeAsync("qbChange6", "target6@test.com", "userChange6", 999);

            // Assert
            Assert.False(success);
            Assert.Equal("Invalid access mode. Only 0 (ViewOnly) and 1 (CanEdit) are allowed.", message);
        }


        //Export question bank
        //1 Thành công export
        //2 Không có quyền
        //3 Không tìm thấy question bank
        //4 QuestionBank không có câu hỏi

        [Fact]
        public async Task ExportQuestionBankReport_ValidRequest_ReturnsFile()
        {
            // Arrange
            var user = new User { UserId = "userExport1", FullName = "Lecturer" };
            user.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture });
            await _context.Users.AddAsync(user);

            var subject = new Subject { SubjectId = "subExport1", SubjectName = "Math", SubjectCode = "SubjectCode" };
            await _context.Subjects.AddAsync(subject);

            var qb = new QuestionBank
            {
                QuestionBankId = "qbExport1",
                Title = "QB Export 1",
                SubjectId = "subExport1",
                CreateUserId = "userExport1"
            };
            await _context.QuestionBanks.AddAsync(qb);

            var question = new Question
            {
                QuestionId = "qExport1",
                QuestionBankId = "qbExport1",
                Content = "Sample Question",
                Type = (int)QuestionTypeChoose.MultipleChoice,
                DifficultLevel = (int)DifficultyLevel.Medium,
                Point = 2.5m,
                Options = JsonConvert.SerializeObject(new List<string> { "A", "B", "C" }),
                CorrectAnswer = "A",
                Status = 1,
                CreateUser = "userExport1",
                Explanation = "Explanation",
                SubjectId = "subExport1",
                UpdateUser = "userUpdateExport1"
            };
            await _context.Questions.AddAsync(question);

            await _context.SaveChangesAsync();

            // Act
            var (message, file) = await _service.ExportQuestionBankReportAsync("qbExport1", "userExport1");

            // Assert
            Assert.Equal("", message);
            Assert.NotNull(file);
            Assert.True(file.Length > 0);
        }

        [Fact]
        public async Task ExportQuestionBankReport_UserWithoutPermission_Fails()
        {
            // Arrange
            var user = new User { UserId = "userExport2" };
            await _context.Users.AddAsync(user);

            await _context.SaveChangesAsync();

            // Act
            var (message, file) = await _service.ExportQuestionBankReportAsync("anyQbId", "userExport2");

            // Assert
            Assert.Equal("You do not have permission to export question bank report.", message);
            Assert.Null(file);
        }

        [Fact]
        public async Task ExportQuestionBankReport_QuestionBankNotFound_Fails()
        {
            // Arrange
            var user = new User { UserId = "userExport3" };
            user.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Admin });
            await _context.Users.AddAsync(user);

            await _context.SaveChangesAsync();

            // Act
            var (message, file) = await _service.ExportQuestionBankReportAsync("nonexistentQb", "userExport3");

            // Assert
            Assert.Equal("Question bank not found.", message);
            Assert.Null(file);
        }


        [Fact]
        public async Task ExportQuestionBankReport_QuestionBankWithoutQuestions_Fails()
        {
            // Arrange
            var user = new User { UserId = "userExport4" };
            user.UserRoles.Add(new UserRole { RoleId = (int)RoleEnum.Lecture });
            await _context.Users.AddAsync(user);

            var subject = new Subject { SubjectId = "subExport4", SubjectName = "Physics", SubjectCode = "SubjectCode" };
            await _context.Subjects.AddAsync(subject);

            var qb = new QuestionBank
            {
                QuestionBankId = "qbExport4",
                Title = "QB Export 4",
                SubjectId = "subExport4",
                CreateUserId = "userExport4"
            };
            await _context.QuestionBanks.AddAsync(qb);

            await _context.SaveChangesAsync();

            // Act
            var (message, file) = await _service.ExportQuestionBankReportAsync("qbExport4", "userExport4");

            // Assert
            Assert.Equal("This question bank has no questions.", message);
            Assert.Null(file);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
