using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API.Helper;
using API.Models;
using API.Repository;
using API.Services;
using API.ViewModels;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace TestQuestionServicePackage
{
    public class QuestionServiceTests : IDisposable
    {
        private readonly QuestionService _service;
        private readonly Sep490Context _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpClient _httpClient;
        public QuestionServiceTests()
        {
            var options = new DbContextOptionsBuilder<Sep490Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new Sep490Context(options);
            _unitOfWork = new UnitOfWork(_context);
            _httpClient = new HttpClient();
            _service = new QuestionService(_context, _unitOfWork, _httpClient);
        }

        //ADD
        //1. Thành công (Success)
        //2. User không có quyền (không phải Lecturer/Admin)
        //3. Question bank không tồn tại
        //4. Question bank không có Subject
        //5. Điểm không hợp lệ (<= 0 hoặc > 10)
        //6. Câu hỏi Essay thiếu đáp án mẫu
        //7. MultipleChoice: Không có đủ option
        //8. MultipleChoice: Correct answer không nằm trong options
        [Theory]
        [InlineData(RoleEnum.Lecture)]
        [InlineData(RoleEnum.Admin)]
        public async Task AddQuestionAsync_ShouldSucceed_WithValidMultipleChoice(RoleEnum role)
        {
            // Arrange
            var userId = "user01";
            var questionBankId = "qb01";
            var subjectId = "sub01";

            _context.Users.Add(new User
            {
                UserId = userId,
                UserRoles = new List<UserRole> { new UserRole { UserId = userId, RoleId = (int)role } }
            });

            _context.Subjects.Add(new Subject
            {
                SubjectId = subjectId,
                SubjectName = "Test Subject",
                SubjectCode = "TS01"
            });

            _context.QuestionBanks.Add(new QuestionBank
            {
                QuestionBankId = questionBankId,
                Title = "Math Bank",
                SubjectId = subjectId,
                CreateUserId = userId
            });

            await _context.SaveChangesAsync();

            var request = new AddQuestionRequest
            {
                QuestionBankId = questionBankId,
                Content = "What is 2 + 2?",
                Type = 1,
                DifficultLevel = 2,
                Point = 5,
                Options = new List<string> { "3", "4", "5" },
                CorrectAnswer = "4"
            };

            // Act
            var result = await _service.AddQuestionAsync(request, userId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Question has been created successfully.", result.Message);
        }

        [Fact]
        public async Task AddQuestionAsync_ShouldFail_WhenUserIsNotLecturerOrAdmin()
        {
            // Arrange
            var userId = "u123";
            _context.Users.Add(new User
            {
                UserId = userId,
                UserRoles = new List<UserRole> { new UserRole { RoleId = (int)RoleEnum.Student, UserId = userId } }
            });

            await _context.SaveChangesAsync();

            var request = new AddQuestionRequest
            {
                QuestionBankId = "qb01",
                Content = "Sample",
                Type = 1,
                DifficultLevel = 1,
                Point = 1,
                Options = new List<string> { "A", "B" },
                CorrectAnswer = "A"
            };

            // Act
            var result = await _service.AddQuestionAsync(request, userId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("The user is not a lecturer or admin and does not have permission to add questions.", result.Message);
        }

        [Fact]
        public async Task AddQuestionAsync_ShouldFail_WhenQuestionBankNotFound()
        {
            // Arrange
            var userId = "user01";
            _context.Users.Add(new User
            {
                UserId = userId,
                UserRoles = new List<UserRole> { new UserRole { UserId = userId, RoleId = (int)RoleEnum.Admin } }
            });
            await _context.SaveChangesAsync();

            var req = new AddQuestionRequest
            {
                QuestionBankId = "nonexistent",
                Content = "Q?",
                Type = 1,
                DifficultLevel = 2,
                Point = 2,
                Options = new List<string> { "A", "B" },
                CorrectAnswer = "A"
            };

            var result = await _service.AddQuestionAsync(req, userId);

            Assert.False(result.Success);
            Assert.Equal("Question bank not found.", result.Message);
        }

        [Fact]
        public async Task AddQuestionAsync_ShouldFail_WhenSubjectNotFoundInBank()
        {
            var userId = "u1";
            var qbId = "qb1";

            _context.Users.Add(new User
            {
                UserId = userId,
                UserRoles = new List<UserRole> { new UserRole { RoleId = (int)RoleEnum.Lecture, UserId = userId }, new UserRole { RoleId = (int)RoleEnum.Admin, UserId = userId } }
            });

            _context.Subjects.Add(new Subject
            {
                SubjectId = "sub1",
                SubjectName = "Test Subject",
                SubjectCode = "TS01"
            });
            _context.QuestionBanks.Add(new QuestionBank
            {
                QuestionBankId = qbId,
                Title = "Test",
                SubjectId = "sub2",
                CreateUserId = userId
            });

            await _context.SaveChangesAsync();

            var request = new AddQuestionRequest
            {
                QuestionBankId = qbId,
                Content = "Test?",
                Type = 0,
                DifficultLevel = 1,
                Point = 1,
                CorrectAnswer = "Any"
            };

            var result = await _service.AddQuestionAsync(request, userId);

            Assert.False(result.Success);
            Assert.Equal("Subject not found.", result.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        [InlineData(11)]
        public async Task AddQuestionAsync_ShouldFail_WhenPointInvalid(int point)
        {
            var userId = "u1";
            var qbId = "qb1";
            var subId = "sub1";

            _context.Users.Add(new User
            {
                UserId = userId,
                UserRoles = new List<UserRole> { new UserRole { RoleId = (int)RoleEnum.Admin, UserId = userId } }
            });

            _context.Subjects.Add(new Subject
            {
                SubjectId = subId,
                SubjectName = "Test Subject",
                SubjectCode = "TS01"
            });

            _context.QuestionBanks.Add(new QuestionBank
            {
                QuestionBankId = qbId,
                SubjectId = subId,
                CreateUserId = userId,
                Title = "Test Bank"
            });

            await _context.SaveChangesAsync();

            var req = new AddQuestionRequest
            {
                QuestionBankId = qbId,
                Content = "Invalid point?",
                Type = 1,
                DifficultLevel = 2,
                Point = point,
                Options = new List<string> { "A", "B" },
                CorrectAnswer = "A"
            };

            var result = await _service.AddQuestionAsync(req, userId);

            Assert.False(result.Success);
            Assert.Equal("Content and point must be provided. Point must be > 0 and <= 10.", result.Message);
        }

        [Fact]
        public async Task AddQuestionAsync_ShouldFail_WhenEssayHasNoCorrectAnswer()
        {
            var userId = "u1";
            var qbId = "qb1";
            var subId = "sub1";

            _context.Users.Add(new User
            {
                UserId = userId,
                UserRoles = new List<UserRole> { new UserRole { RoleId = (int)RoleEnum.Lecture, UserId = userId } }
            });

            _context.Subjects.Add(new Subject
            {
                SubjectId = subId,
                SubjectName = "Test Subject",
                SubjectCode = "TS01"
            });

            _context.QuestionBanks.Add(new QuestionBank
            {
                QuestionBankId = qbId,
                SubjectId = subId,
                CreateUserId = userId,
                Title = "Test Bank"
            });

            await _context.SaveChangesAsync();

            var req = new AddQuestionRequest
            {
                QuestionBankId = qbId,
                Content = "Explain gravity",
                Type = 0,
                DifficultLevel = 1,
                Point = 5,
                CorrectAnswer = ""
            };

            var result = await _service.AddQuestionAsync(req, userId);

            Assert.False(result.Success);
            Assert.Equal("Essay must have a sample correct answer.", result.Message);
        }

        [Fact]
        public async Task AddQuestionAsync_ShouldFail_WhenMultipleChoiceOptionsTooFew()
        {
            var userId = "u1";
            var qbId = "qb1";
            var subId = "sub1";

            _context.Users.Add(new User
            {
                UserId = userId,
                UserRoles = new List<UserRole> { new UserRole { RoleId = (int)RoleEnum.Lecture, UserId = userId } }
            });

            _context.Subjects.Add(new Subject
            {
                SubjectId = subId,
                SubjectName = "Test Subject",
                SubjectCode = "TS01"
            });

            _context.QuestionBanks.Add(new QuestionBank
            {
                QuestionBankId = qbId,
                SubjectId = subId,
                CreateUserId = userId,
                Title = "Test Bank"
            });

            await _context.SaveChangesAsync();

            var req = new AddQuestionRequest
            {
                QuestionBankId = qbId,
                Content = "Choose?",
                Type = 1,
                DifficultLevel = 1,
                Point = 5,
                Options = new List<string> { "OnlyOne" },
                CorrectAnswer = "OnlyOne"
            };

            var result = await _service.AddQuestionAsync(req, userId);

            Assert.False(result.Success);
            Assert.Equal("Multiple choice must have at least 2 options.", result.Message);
        }

        [Fact]
        public async Task AddQuestionAsync_ShouldFail_WhenCorrectAnswerNotInOptions()
        {
            var userId = "u1";
            var qbId = "qb1";
            var subId = "sub1";

            _context.Users.Add(new User
            {
                UserId = userId,
                UserRoles = new List<UserRole> { new UserRole { RoleId = (int)RoleEnum.Lecture, UserId = userId } }
            });

            _context.Subjects.Add(new Subject
            {
                SubjectId = subId,
                SubjectName = "Test Subject",
                SubjectCode = "TS01"
            });

            _context.QuestionBanks.Add(new QuestionBank
            {
                QuestionBankId = qbId,
                SubjectId = subId,
                CreateUserId = userId,
                Title = "Test Bank"
            });

            await _context.SaveChangesAsync();

            var req = new AddQuestionRequest
            {
                QuestionBankId = qbId,
                Content = "Select?",
                Type = 1,
                DifficultLevel = 1,
                Point = 5,
                Options = new List<string> { "A", "B" },
                CorrectAnswer = "C"
            };

            var result = await _service.AddQuestionAsync(req, userId);

            Assert.False(result.Success);
            Assert.Equal("Correct answer(s) [C] not found in options.", result.Message);
        }

        //EDIT
        //1. Thành công: Lecturer hoặc Admin sửa MultipleChoice hợp lệ
        //2. Không có quyền (không phải Admin hoặc Lecturer)
        //3. Câu hỏi không tồn tại
        //4. Không tìm thấy ngân hàng câu hỏi
        //5. Subject không tồn tại trong DB
        //6. Dữ liệu không hợp lệ: điểm 0 hoặc >10, thiếu content
        //7. Loại câu hỏi sai (Type không thuộc 0 hoặc 1)
        //8. MultipleChoice: CorrectAnswer không có trong danh sách Options

        [Theory]
        [InlineData(RoleEnum.Lecture)]
        [InlineData(RoleEnum.Admin)]
        public async Task EditQuestionAsync_ShouldSucceed_WithValidInput(RoleEnum role)
        {
            var userId = "user01";
            var questionBankId = "qb01";
            var subjectId = "sub01";
            var questionId = "q01";

            var user = new User
            {
                UserId = userId,
                Email = "lecturer@example.com",
                UserRoles = new List<UserRole> {
            new UserRole { RoleId = (int)role, UserId = userId }
        }
            };

            var subject = new Subject { SubjectId = subjectId, SubjectCode = "SC", SubjectName = "Math" };
            var bank = new QuestionBank
            {
                QuestionBankId = questionBankId,
                Title = "QB1",
                SubjectId = subjectId,
                CreateUserId = userId
            };
            var question = new Question
            {
                QuestionId = questionId,
                QuestionBankId = bank.QuestionBankId,
                SubjectId = subjectId,
                Content = "Old Content",
                Type = 1,
                DifficultLevel = 2,
                Point = 5,
                Options = JsonConvert.SerializeObject(new List<string> { "1", "2" }),
                CorrectAnswer = "1",
                CreateUser = userId,
                Explanation = "",
                UpdateUser = ""
            };

            _context.Users.Add(user);
            _context.Subjects.Add(subject);
            _context.QuestionBanks.Add(bank);
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            var req = new EditQuestionRequest
            {
                QuestionId = questionId,
                QuestionBankId = bank.QuestionBankId,
                Content = "What is 3+2?",
                Type = 1,
                DifficultLevel = 3,
                Point = 5,
                Options = new List<string> { "4", "5", "6" },
                CorrectAnswer = "5",
                Explanation = "Basic math"
            };

            // Act
            var result = await _service.EditQuestionAsync(req, userId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Question has been updated successfully.", result.Message);
        }

        [Fact]
        public async Task EditQuestionAsync_ShouldFail_WhenUserIsUnauthorized()
        {
            var userId = "unauthUser";
            var user = new User
            {
                UserId = userId,
                Email = "unauth@example.com",
                UserRoles = new List<UserRole>() // no role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var req = new EditQuestionRequest
            {
                QuestionId = "q01",
                QuestionBankId = "qb01",
                Content = "Test",
                Type = 1,
                DifficultLevel = 1,
                Point = 1,
                Options = new List<string> { "1", "2" },
                CorrectAnswer = "1"
            };

            var result = await _service.EditQuestionAsync(req, userId);

            Assert.False(result.Success);
            Assert.Equal("You do not have permission to perform this action.", result.Message);
        }


        [Fact]
        public async Task EditQuestionAsync_ShouldFail_WhenQuestionNotFound()
        {
            var userId = "user01";
            var user = new User
            {
                UserId = userId,
                Email = "lecturer@example.com",
                UserRoles = new List<UserRole> {
            new UserRole { RoleId = (int)RoleEnum.Lecture, UserId = userId }
        }
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var req = new EditQuestionRequest
            {
                QuestionId = "nonexistent",
                QuestionBankId = "qb01",
                Content = "Test",
                Type = 1,
                DifficultLevel = 1,
                Point = 1,
                Options = new List<string> { "1", "2" },
                CorrectAnswer = "1"
            };

            var result = await _service.EditQuestionAsync(req, userId);

            Assert.False(result.Success);
            Assert.Equal("Question not found.", result.Message);
        }


        [Fact]
        public async Task EditQuestionAsync_ShouldFail_WhenQuestionBankNotFound()
        {
            var userId = "user01";
            var subjectId = "sub01";
            var questionId = "q01";

            var user = new User
            {
                UserId = userId,
                Email = "lecturer@example.com",
                UserRoles = new List<UserRole> {
                    new UserRole { RoleId = (int)RoleEnum.Lecture, UserId = userId }
                }
            };

            var subject = new Subject { SubjectId = subjectId, SubjectCode = "SC", SubjectName = "Math" };
            var bank = new QuestionBank
            {
                QuestionBankId = "bank01",
                SubjectId = subject.SubjectId,
                Title = "Algebra",
                CreateUserId = "user01"
            };
            var question = new Question
            {
                QuestionId = questionId,
                QuestionBankId = "bank01",
                SubjectId = bank.SubjectId,
                Content = "Old",
                Type = 1,
                DifficultLevel = 1,
                Point = 5,
                Options = JsonConvert.SerializeObject(new List<string> { "1", "2" }),
                CorrectAnswer = "1",
                CreateUser = userId,
                Explanation = "",
                UpdateUser = ""
            };

            _context.Users.Add(user);
            _context.Subjects.Add(subject);
            _context.QuestionBanks.Add(bank);
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            var req = new EditQuestionRequest
            {
                QuestionId = questionId,
                QuestionBankId = "invalid-qb",
                Content = "New Content",
                Type = 1,
                DifficultLevel = 1,
                Point = 1,
                Options = new List<string> { "1", "2" },
                CorrectAnswer = "1"
            };

            var result = await _service.EditQuestionAsync(req, userId);

            Assert.False(result.Success);
            Assert.Equal("Question bank not found.", result.Message);
        }


        [Fact]
        public async Task EditQuestionAsync_ShouldFail_WhenSubjectNotFound()
        {
            var userId = "user01";
            var subjectId = "sub01";
            var questionBankId = "qb01";
            var questionId = "q01";

            var user = new User
            {
                UserId = userId,
                Email = "lecturer@example.com",
                UserRoles = new List<UserRole> {
            new UserRole { RoleId = (int)RoleEnum.Lecture, UserId = userId }
        }
            };

            var bank = new QuestionBank
            {
                QuestionBankId = questionBankId,
                Title = "QB1",
                SubjectId = subjectId, // nhưng không seed Subject
                CreateUserId = userId
            };

            var question = new Question
            {
                QuestionId = questionId,
                QuestionBankId = questionBankId,
                SubjectId = subjectId,
                Content = "Test",
                Type = 1,
                DifficultLevel = 1,
                Point = 1,
                Options = JsonConvert.SerializeObject(new List<string> { "1", "2" }),
                CorrectAnswer = "1",
                CreateUser = userId,
                Explanation = "",
                UpdateUser = ""
            };

            _context.Users.Add(user);
            _context.QuestionBanks.Add(bank);
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            var req = new EditQuestionRequest
            {
                QuestionId = questionId,
                QuestionBankId = questionBankId,
                Content = "New content",
                Type = 1,
                DifficultLevel = 1,
                Point = 1,
                Options = new List<string> { "1", "2" },
                CorrectAnswer = "1"
            };

            var result = await _service.EditQuestionAsync(req, userId);

            Assert.False(result.Success);
            Assert.Equal("Subject not found.", result.Message);
        }


        //[Fact]
        //public async Task EditQuestionAsync_ShouldFail_IfContentOrPointInvalid()
        //{
        //    // Arrange
        //    var user = new User { UserId = "user01", FullName = "Test User" };
        //    var role = new Role { Id = (int)RoleEnum.Lecture, Name = "Lecture" };
        //    var userRole = new UserRole { UserId = user.UserId, RoleId = role.Id };

        //    var subject = new Subject { SubjectId = "sub01", SubjectCode = "SC", SubjectName = "Math" };
        //    var bank = new QuestionBank
        //    {
        //        QuestionBankId = "bank01",
        //        SubjectId = subject.SubjectId,
        //        Title = "Algebra",
        //        CreateUserId = "user01"
        //    };

        //    var question = new Question
        //    {
        //        QuestionId = "q01",
        //        QuestionBankId = bank.QuestionBankId,
        //        SubjectId = subject.SubjectId,
        //        Content = "Old content",
        //        Type = 1,
        //        DifficultLevel = 2,
        //        Point = 5,
        //        Options = JsonConvert.SerializeObject(new List<string> { "A", "B" }),
        //        CorrectAnswer = "A",
        //        CreateUser = "user01",
        //        Explanation = "",
        //        UpdateUser = ""
        //    };

        //    _context.Users.Add(user);
        //    _context.Roles.Add(role);
        //    _context.UserRoles.Add(userRole);
        //    _context.Subjects.Add(subject);
        //    _context.QuestionBanks.Add(bank);
        //    _context.Questions.Add(question);
        //    await _context.SaveChangesAsync();

        //    var req = new EditQuestionRequest
        //    {
        //        QuestionId = "q01",
        //        QuestionBankId = "bank01",
        //        Content = " ", // Invalid
        //        Type = 1,
        //        DifficultLevel = 1,
        //        Point = 0,     // Invalid
        //        Options = new List<string> { "A", "B" },
        //        CorrectAnswer = "A"
        //    };

        //    // Act
        //    var result = await _service.EditQuestionAsync(req, "user01");

        //    // Assert
        //    Assert.False(result.Success);
        //    Assert.Equal("Content and point must be provided. Point must be > 0 and <= 10.", result.Message);
        //}

        [Fact]
        public async Task EditQuestionAsync_ShouldFail_InvalidQuestionType()
        {
            // Arrange
            var user = new User { UserId = "user01", FullName = "Test User" };
            var role = new Role { Id = (int)RoleEnum.Admin, Name = "Admin" };
            var userRole = new UserRole { UserId = user.UserId, RoleId = role.Id };

            var subject = new Subject { SubjectId = "sub01", SubjectCode = "SC", SubjectName = "Physics" };
            var bank = new QuestionBank { QuestionBankId = "bank01", SubjectId = subject.SubjectId, Title = "Mechanics", CreateUserId = "user01" };

            var question = new Question
            {
                QuestionId = "q01",
                QuestionBankId = bank.QuestionBankId,
                SubjectId = subject.SubjectId,
                Content = "Initial content",
                Type = 1,
                DifficultLevel = 1,
                Point = 5,
                Options = JsonConvert.SerializeObject(new List<string> { "A", "B" }),
                CorrectAnswer = "A",
                CreateUser = "user01",
                Explanation = "",
                UpdateUser = ""
            };

            _context.Users.Add(user);
            _context.Roles.Add(role);
            _context.UserRoles.Add(userRole);
            _context.Subjects.Add(subject);
            _context.QuestionBanks.Add(bank);
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            var req = new EditQuestionRequest
            {
                QuestionId = "q01",
                QuestionBankId = "bank01",
                Content = "Valid content",
                Type = 3, // Invalid
                DifficultLevel = 1,
                Point = 5,
                Options = new List<string> { "A", "B" },
                CorrectAnswer = "A"
            };

            // Act
            var result = await _service.EditQuestionAsync(req, "user01");

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Invalid question type", result.Message);
        }


        [Fact]
        public async Task EditQuestionAsync_ShouldFail_CorrectAnswerNotInOptions()
        {
            // Arrange
            var user = new User { UserId = "user01", FullName = "Lecturer" };
            var role = new Role { Id = (int)RoleEnum.Lecture, Name = "Lecturer" };
            var userRole = new UserRole { UserId = "user01", RoleId = role.Id };

            var subject = new Subject { SubjectId = "sub01", SubjectCode = "SC", SubjectName = "Chemistry" };
            var bank = new QuestionBank { QuestionBankId = "bank01", SubjectId = subject.SubjectId, Title = "Organic", CreateUserId = "user01" };

            var question = new Question
            {
                QuestionId = "q01",
                QuestionBankId = bank.QuestionBankId,
                SubjectId = subject.SubjectId,
                Content = "Something",
                Type = 1,
                DifficultLevel = 2,
                Point = 5,
                Options = JsonConvert.SerializeObject(new List<string> { "A", "B" }),
                CorrectAnswer = "A",
                CreateUser = "user01",
                Explanation = "",
                UpdateUser = ""
            };

            _context.Users.Add(user);
            _context.Roles.Add(role);
            _context.UserRoles.Add(userRole);
            _context.Subjects.Add(subject);
            _context.QuestionBanks.Add(bank);
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            var req = new EditQuestionRequest
            {
                QuestionId = "q01",
                QuestionBankId = "bank01",
                Content = "Valid",
                Type = 1,
                DifficultLevel = 1,
                Point = 5,
                Options = new List<string> { "A", "B" },
                CorrectAnswer = "C" // Not in options
            };

            // Act
            var result = await _service.EditQuestionAsync(req, "user01");

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Correct answer(s) [C] not found in options.", result.Message);
        }

        //DELETE
        //1. Thành công: Admin hoặc Lecturer xóa câu hỏi hợp lệ
        //2. Không có quyền (không phải Admin hoặc Lecturer)
        //3. Question bank không tồn tại
        //4. Câu hỏi không tồn tại hoặc sai Question bank
        [Theory]
        [InlineData((int)RoleEnum.Admin)]
        [InlineData((int)RoleEnum.Lecture)]
        public async Task DeleteQuestionAsync_ShouldSucceed_WhenUserIsAdminOrLecturer(int roleId)
        {
            // Arrange
            var user = new User { UserId = "user01" };
            var role = new Role { Id = roleId, Name = roleId == (int)RoleEnum.Admin ? "Admin" : "Lecture" };
            var userRole = new UserRole { UserId = user.UserId, RoleId = role.Id };

            var subject = new Subject { SubjectId = "sub01", SubjectCode = "SC", SubjectName = "Math" };
            var bank = new QuestionBank { QuestionBankId = "bank01", SubjectId = subject.SubjectId, Title = "Bank", CreateUserId = "user01" };
            var question = new Question
            {
                QuestionId = "q01",
                QuestionBankId = bank.QuestionBankId,
                SubjectId = subject.SubjectId,
                Content = "Content",
                Type = 1,
                DifficultLevel = 2,
                Point = 5,
                CreateUser = user.UserId,
                Options = JsonConvert.SerializeObject(new List<string> { "A", "B" }),
                CorrectAnswer = "A",
                Explanation = "",
                UpdateUser = ""
            };

            _context.Users.Add(user);
            _context.Roles.Add(role);
            _context.UserRoles.Add(userRole);
            _context.Subjects.Add(subject);
            _context.QuestionBanks.Add(bank);
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.DeleteQuestionAsync(bank.QuestionBankId, question.QuestionId, user.UserId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Question has been deleted successfully.", result.Message);
        }

        [Fact]
        public async Task DeleteQuestionAsync_ShouldFail_WhenUserIsUnauthorized()
        {
            // Arrange
            var user = new User { UserId = "user02" }; // No role
            var subject = new Subject { SubjectId = "sub01", SubjectCode = "SC", SubjectName = "Math" };
            var bank = new QuestionBank { QuestionBankId = "bank02", SubjectId = subject.SubjectId, CreateUserId = "user02", Title = "Title" };
            var question = new Question
            {
                QuestionId = "q02",
                QuestionBankId = bank.QuestionBankId,
                SubjectId = subject.SubjectId,
                Content = "Test Q",
                Type = 1,
                DifficultLevel = 1,
                Point = 2,
                CreateUser = user.UserId,
                Options = JsonConvert.SerializeObject(new List<string> { "A", "B" }),
                CorrectAnswer = "A",
                Explanation = "",
                UpdateUser = ""
            };

            _context.Users.Add(user);
            _context.Subjects.Add(subject);
            _context.QuestionBanks.Add(bank);
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.DeleteQuestionAsync(bank.QuestionBankId, question.QuestionId, user.UserId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("You do not have permission to perform this action.", result.Message);
        }

        [Fact]
        public async Task DeleteQuestionAsync_ShouldFail_WhenQuestionBankNotFound()
        {
            // Arrange
            var user = new User { UserId = "user03" };
            var role = new Role { Id = (int)RoleEnum.Admin, Name = "Admin" };
            var userRole = new UserRole { UserId = user.UserId, RoleId = role.Id };

            _context.Users.Add(user);
            _context.Roles.Add(role);
            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.DeleteQuestionAsync("nonexistent-bank", "any-question-id", user.UserId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Question bank not found or not match with question.", result.Message);
        }

        [Fact]
        public async Task DeleteQuestionAsync_ShouldFail_WhenQuestionNotFoundOrMismatched()
        {
            // Arrange
            var user = new User { UserId = "user04" };
            var role = new Role { Id = (int)RoleEnum.Lecture, Name = "Lecture" };
            var userRole = new UserRole { UserId = user.UserId, RoleId = role.Id };

            var subject = new Subject { SubjectId = "sub02", SubjectCode = "SC", SubjectName = "Science" };
            var bank = new QuestionBank { QuestionBankId = "bank04", SubjectId = subject.SubjectId, CreateUserId = "user04", Title = "Title" };

            _context.Users.Add(user);
            _context.Roles.Add(role);
            _context.UserRoles.Add(userRole);
            _context.Subjects.Add(subject);
            _context.QuestionBanks.Add(bank);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.DeleteQuestionAsync("bank04", "wrong-question-id", user.UserId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Question not found or not match with question bank.", result.Message);
        }
        // GET LIST
        //1. Thành công: Admin hoặc Lecturer lấy danh sách câu hỏi hợp lệ
        //2. Không có quyền (không phải Admin hoặc Lecturer)
        //3. Lọc theo TextSearch
        //4. Lọc theo Status
        //5. Lọc theo IsMyQuestion = true
        //6. Lọc theo DifficultyLevel
        //7. Kết quả không có dữ liệu
        //8. Phân trang
        [Theory]
        [InlineData((int)RoleEnum.Admin)]
        [InlineData((int)RoleEnum.Lecture)]
        public async Task GetListQuestionAsync_ShouldSucceed_WhenUserIsAdminOrLecturer(int roleId)
        {
            // Arrange
            var user = new User { UserId = "user01" };
            var role = new Role { Id = roleId, Name = roleId == (int)RoleEnum.Admin ? "Admin" : "Lecture" };
            var userRole = new UserRole { UserId = user.UserId, RoleId = role.Id };

            var subject = new Subject { SubjectId = "sub01", SubjectCode = "SC", SubjectName = "Math" };
            var bank = new QuestionBank { QuestionBankId = "bank01", Title = "Bank 1", SubjectId = subject.SubjectId, CreateUserId = user.UserId };

            var question = new Question
            {
                QuestionId = "q01",
                QuestionBankId = bank.QuestionBankId,
                SubjectId = subject.SubjectId,
                Content = "This is a sample question",
                Type = 1,
                DifficultLevel = 2,
                Point = 3,
                CreateUser = user.UserId,
                Options = JsonConvert.SerializeObject(new List<string> { "A", "B", "C" }),
                CorrectAnswer = "A",
                Status = 1,
                Explanation = "",
                UpdateUser = ""
            };

            _context.Users.Add(user);
            _context.Roles.Add(role);
            _context.UserRoles.Add(userRole);
            _context.Subjects.Add(subject);
            _context.QuestionBanks.Add(bank);
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            var request = new QuestionRequestVM { CurrentPage = 1, PageSize = 10 };

            // Act
            var result = await _service.GetListQuestionAsync(request, user.UserId);

            // Assert
            Assert.True(result.Success);
            var list = (IEnumerable<QuestionListVM>)result.Item3.Result!;
            Assert.Equal(1, list.Count());
            Assert.Equal("Successfully retrieved question list.", result.Message);
        }

        [Fact]
        public async Task GetListQuestionAsync_ShouldFail_WhenUserIsUnauthorized()
        {
            // Arrange
            var user = new User { UserId = "user02" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var request = new QuestionRequestVM { CurrentPage = 1, PageSize = 10 };

            // Act
            var result = await _service.GetListQuestionAsync(request, user.UserId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("You do not have permission to perform this action.", result.Message);
        }

        [Fact]
        public async Task GetListQuestionAsync_ShouldFilter_ByTextSearch()
        {
            // Arrange
            var user = new User { UserId = "user03" };
            var role = new Role { Id = (int)RoleEnum.Lecture, Name = "Lecture" };
            var userRole = new UserRole { UserId = user.UserId, RoleId = role.Id };

            var subject = new Subject { SubjectId = "sub02", SubjectCode = "SC", SubjectName = "Physics" };
            var bank = new QuestionBank { QuestionBankId = "bank02", Title = "Physics Bank", SubjectId = subject.SubjectId, CreateUserId = user.UserId };
            var question = new Question
            {
                QuestionId = "q02",
                QuestionBankId = bank.QuestionBankId,
                SubjectId = subject.SubjectId,
                Content = "What is velocity?",
                Type = 1,
                DifficultLevel = 1,
                Point = 2,
                CreateUser = user.UserId,
                Options = JsonConvert.SerializeObject(new List<string> { "A", "B" }),
                CorrectAnswer = "B",
                Status = 1,
                Explanation = "",
                UpdateUser = ""
            };

            _context.Users.Add(user);
            _context.Roles.Add(role);
            _context.UserRoles.Add(userRole);
            _context.Subjects.Add(subject);
            _context.QuestionBanks.Add(bank);
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            var request = new QuestionRequestVM
            {
                TextSearch = "velocity",
                CurrentPage = 1,
                PageSize = 10
            };

            // Act
            var result = await _service.GetListQuestionAsync(request, user.UserId);

            // Assert
            Assert.True(result.Success);
            Assert.Single((IEnumerable<QuestionListVM>)result.Item3.Result);
        }

        [Fact]
        public async Task GetListQuestionAsync_ShouldFilter_ByStatus_IsMyQuestion_DifficultyLevel()
        {
            // Arrange
            var user = new User { UserId = "user04" };
            var role = new Role { Id = (int)RoleEnum.Admin, Name = "Admin" };
            var userRole = new UserRole { UserId = user.UserId, RoleId = role.Id };
            var subject = new Subject { SubjectId = "sub04", SubjectCode = "CS", SubjectName = "IT" };
            var bank = new QuestionBank { QuestionBankId = "bank04", Title = "Tech", SubjectId = subject.SubjectId, CreateUserId = user.UserId };
            var question = new Question
            {
                QuestionId = "q04",
                QuestionBankId = bank.QuestionBankId,
                SubjectId = subject.SubjectId,
                Content = "What is a CPU?",
                Type = 1,
                DifficultLevel = 3,
                Point = 4,
                CreateUser = user.UserId,
                Status = 1,
                Options = JsonConvert.SerializeObject(new List<string> { "A", "B" }),
                CorrectAnswer = "A",
                Explanation = "",
                UpdateUser = ""
            };

            _context.Users.Add(user);
            _context.Roles.Add(role);
            _context.UserRoles.Add(userRole);
            _context.Subjects.Add(subject);
            _context.QuestionBanks.Add(bank);
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            var request = new QuestionRequestVM
            {
                Status = (ActiveStatus)1,
                IsMyQuestion = true,
                DifficultyLevel = (DifficultyLevel)3,
                CurrentPage = 1,
                PageSize = 10
            };

            // Act
            var result = await _service.GetListQuestionAsync(request, user.UserId);

            // Assert
            Assert.True(result.Success);
            Assert.Single((IEnumerable<QuestionListVM>)result.Item3.Result);

        }

        [Fact]
        public async Task GetListQuestionAsync_ShouldReturnEmpty_WhenNoQuestionMatched()
        {
            // Arrange
            var user = new User { UserId = "user05" };
            var role = new Role { Id = (int)RoleEnum.Lecture, Name = "Lecture" };
            var userRole = new UserRole { UserId = user.UserId, RoleId = role.Id };

            _context.Users.Add(user);
            _context.Roles.Add(role);
            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            var request = new QuestionRequestVM
            {
                TextSearch = "nonexistent",
                CurrentPage = 1,
                PageSize = 10
            };

            // Act
            var result = await _service.GetListQuestionAsync(request, user.UserId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("No found any question", result.Message);
        }
        [Fact]
        public async Task GetListQuestionAsync_ShouldPaginateResults()
        {
            // Arrange
            var user = new User { UserId = "user06" };
            var role = new Role { Id = (int)RoleEnum.Admin, Name = "Admin" };
            var userRole = new UserRole { UserId = user.UserId, RoleId = role.Id };
            var subject = new Subject { SubjectId = "sub06", SubjectCode = "MATH", SubjectName = "Math" };
            var bank = new QuestionBank { QuestionBankId = "bank06", SubjectId = subject.SubjectId, Title = "Math Bank", CreateUserId = user.UserId };

            _context.Users.Add(user);
            _context.Roles.Add(role);
            _context.UserRoles.Add(userRole);
            _context.Subjects.Add(subject);
            _context.QuestionBanks.Add(bank);

            for (int i = 0; i < 15; i++)
            {
                _context.Questions.Add(new Question
                {
                    QuestionId = $"q{i}",
                    QuestionBankId = bank.QuestionBankId,
                    SubjectId = subject.SubjectId,
                    Content = $"Q{i} content",
                    Type = 1,
                    DifficultLevel = 1,
                    Point = 1,
                    CreateUser = user.UserId,
                    Status = 1,
                    Options = JsonConvert.SerializeObject(new List<string> { "A", "B" }),
                    CorrectAnswer = "A",
                    Explanation = "",
                    UpdateUser = ""
                });
            }

            await _context.SaveChangesAsync();

            var request = new QuestionRequestVM
            {
                CurrentPage = 2,
                PageSize = 10
            };

            // Act
            var result = await _service.GetListQuestionAsync(request, user.UserId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(5, ((IEnumerable<QuestionListVM>)result.Item3.Result!).Count());
            Assert.Equal(2, result.Item3.CurrentPage);
            Assert.Equal(10, result.Item3.PageSize);
        }
        //TOGGLE STATUS
        [Theory]
        [InlineData((int)RoleEnum.Admin, 1, 0, "deactivated")]
        [InlineData((int)RoleEnum.Lecture, 0, 1, "activated")]
        public async Task ToggleStatusAsync_ShouldToggleStatus_WhenUserHasPermission(int roleId, int initialStatus, int expectedStatus, string expectedAction)
        {
            // Arrange
            var userId = $"user_{roleId}";
            var questionId = $"q_{roleId}";
            var bankId = $"bank_{roleId}";

            var user = new User { UserId = userId };
            var role = new Role { Id = roleId, Name = roleId == (int)RoleEnum.Admin ? "Admin" : "Lecturer" };
            var userRole = new UserRole { UserId = userId, RoleId = roleId };

            var bank = new QuestionBank
            {
                QuestionBankId = bankId,
                Title = "Test Bank",
                SubjectId = "subject",
                CreateUserId = userId
            };

            var question = new Question
            {
                QuestionId = questionId,
                QuestionBankId = bankId,
                SubjectId = "subject",
                Content = "Sample Question",
                Type = 1,
                Status = initialStatus,
                CreateUser = userId,
                Options = JsonConvert.SerializeObject(new List<string> { "A", "B" }),
                CorrectAnswer = "A",
                Explanation = "",
                UpdateUser = ""
            };

            _context.Users.Add(user);
            _context.Roles.Add(role);
            _context.UserRoles.Add(userRole);
            _context.QuestionBanks.Add(bank);
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            var req = new ToggleQuestionStatusRequest { QuestionId = questionId };

            // Act
            var result = await _service.ToggleStatusAsync(req, userId);
            var updatedQuestion = await _context.Questions.FindAsync(questionId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal($"Question has been {expectedAction} successfully.", result.Message);
            Assert.Equal(expectedStatus, updatedQuestion.Status);
        }

        [Fact]
        public async Task ToggleStatusAsync_ShouldFail_WhenUserHasNoPermission()
        {
            // Arrange
            var user = new User { UserId = "u03" }; // Không có role
            var question = new Question
            {
                QuestionId = "q03",
                QuestionBankId = "bank03",
                SubjectId = "sub03",
                Content = "Question C",
                Type = 1,
                Status = 1,
                CreateUser = user.UserId,
                Options = JsonConvert.SerializeObject(new List<string> { "A", "B" }),
                CorrectAnswer = "A",
                Explanation = "",
                UpdateUser = ""
            };

            _context.Users.Add(user);
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            var req = new ToggleQuestionStatusRequest { QuestionId = "q03" };

            // Act
            var result = await _service.ToggleStatusAsync(req, user.UserId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("You do not have permission to perform this action.", result.Message);
        }

        [Fact]
        public async Task ToggleStatusAsync_ShouldFail_WhenQuestionNotFound()
        {
            // Arrange
            var user = new User { UserId = "u04" };
            var role = new Role { Id = (int)RoleEnum.Admin, Name = "Admin" };
            var userRole = new UserRole { UserId = user.UserId, RoleId = role.Id };

            _context.Users.Add(user);
            _context.Roles.Add(role);
            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            var req = new ToggleQuestionStatusRequest { QuestionId = "nonexistent" };

            // Act
            var result = await _service.ToggleStatusAsync(req, user.UserId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Question not found.", result.Message);
        }

        //IMPORT QUESTIONS
        [Theory]
        [InlineData(RoleEnum.Lecture)]
        [InlineData(RoleEnum.Admin)]
        public async Task ImportListQuestionAsync_ShouldSucceed_WithValidMultipleChoice(RoleEnum role)
        {
            var userId = "user01";
            var subjectId = "sub01";
            var questionBankId = "qb01";

            _context.Users.Add(new User
            {
                UserId = userId,
                UserRoles = new List<UserRole> { new UserRole { RoleId = (int)role, UserId = userId } }
            });

            _context.Subjects.Add(new Subject
            {
                SubjectId = subjectId,
                SubjectName = "Test Subject",
                SubjectCode = "TS01"
            });

            _context.QuestionBanks.Add(new QuestionBank
            {
                QuestionBankId = questionBankId,
                SubjectId = subjectId,
                CreateUserId = userId,
                Title = "Test Bank"
            });

            await _context.SaveChangesAsync();

            var questions = new List<AddQuestionRequest>
            {
                new AddQuestionRequest
                {
                    QuestionBankId = questionBankId,
                    Content = "What is 2 + 2?",
                    Type = 1,
                    DifficultLevel = 2,
                    Point = 4,
                    Options = new List<string> { "2", "3", "4" },
                    CorrectAnswer = "4",
                    Explanation = "Basic math"
                }
            };

            var result = await _service.ImportListQuestionAsync(questions, userId);

            Assert.True(result.Success);
            Assert.Contains("Imported 1 question(s)", result.Message);
        }

        [Fact]
        public async Task ImportListQuestionAsync_ShouldFail_WhenUserIsNotAuthorized()
        {
            var userId = "userX";

            _context.Users.Add(new User
            {
                UserId = userId,
                UserRoles = new List<UserRole> { new UserRole { RoleId = (int)RoleEnum.Student, UserId = userId } }
            });

            await _context.SaveChangesAsync();

            var list = new List<AddQuestionRequest>
            {
                new AddQuestionRequest
                {
                    QuestionBankId = "any",
                    Content = "Q?",
                    Type = 1,
                    DifficultLevel = 2,
                    Point = 3,
                    Options = new List<string> { "A", "B" },
                    CorrectAnswer = "A"
                }
            };

            var result = await _service.ImportListQuestionAsync(list, userId);

            Assert.False(result.Success);
            Assert.Equal("You do not have permission to perform this action.", result.Message);
        }

        [Fact]
        public async Task ImportListQuestionAsync_ShouldFail_WhenListIsEmpty()
        {
            var result = await _service.ImportListQuestionAsync(new List<AddQuestionRequest>(), "anyUser");

            Assert.False(result.Success);
            Assert.Equal("The question list is empty.", result.Message);
        }

        [Fact]
        public async Task ImportListQuestionAsync_ShouldFail_WhenPointInvalid()
        {
            var userId = "u1";
            var subId = "sub1";
            var qbId = "qb1";

            _context.Users.Add(new User
            {
                UserId = userId,
                UserRoles = new List<UserRole> { new UserRole { RoleId = (int)RoleEnum.Admin, UserId = userId } }
            });

            _context.Subjects.Add(new Subject
            {
                SubjectId = subId,
                SubjectName = "Test",
                SubjectCode = "T01"
            });

            _context.QuestionBanks.Add(new QuestionBank
            {
                QuestionBankId = qbId,
                SubjectId = subId,
                CreateUserId = userId,
                Title = "Test Bank"
            });

            await _context.SaveChangesAsync();

            var list = new List<AddQuestionRequest>
            {
                new AddQuestionRequest
                {
                    QuestionBankId = qbId,
                    Content = "Bad Point?",
                    Type = 1,
                    DifficultLevel = 2,
                    Point = 0,
                    Options = new List<string> { "A", "B" },
                    CorrectAnswer = "A"
                }
            };

            var result = await _service.ImportListQuestionAsync(list, userId);

            Assert.False(result.Success);
            Assert.Contains("Invalid content or point", result.Message);
        }

        [Fact]
        public async Task ImportListQuestionAsync_ShouldFail_WhenCorrectAnswerNotInOptions()
        {
            var userId = "u1";
            var subId = "sub1";
            var qbId = "qb1";

            _context.Users.Add(new User
            {
                UserId = userId,
                UserRoles = new List<UserRole> { new UserRole { RoleId = (int)RoleEnum.Lecture, UserId = userId } }
            });

            _context.Subjects.Add(new Subject { SubjectId = subId, SubjectName = "Sub", SubjectCode = "S01" });

            _context.QuestionBanks.Add(new QuestionBank
            {
                QuestionBankId = qbId,
                SubjectId = subId,
                Title = "Bank",
                CreateUserId = userId
            });

            await _context.SaveChangesAsync();

            var list = new List<AddQuestionRequest>
            {
                new AddQuestionRequest
                {
                    QuestionBankId = qbId,
                    Content = "Choose best",
                    Type = 1,
                    DifficultLevel = 2,
                    Point = 4,
                    Options = new List<string> { "A", "B" },
                    CorrectAnswer = "C"
                }
            };

            var result = await _service.ImportListQuestionAsync(list, userId);

            Assert.False(result.Success);
            Assert.Contains("Invalid correct answers: C", result.Message);
        }

        [Fact]
        public async Task ImportListQuestionAsync_ShouldFail_WhenEssayMissingCorrectAnswer()
        {
            var userId = "u1";
            var subjectId = "sub1";
            var bankId = "qb1";

            _context.Users.Add(new User
            {
                UserId = userId,
                UserRoles = new List<UserRole> { new UserRole { RoleId = (int)RoleEnum.Admin, UserId = userId } }
            });

            _context.Subjects.Add(new Subject
            {
                SubjectId = subjectId,
                SubjectName = "EssaySubject",
                SubjectCode = "ESS01"
            });

            _context.QuestionBanks.Add(new QuestionBank
            {
                QuestionBankId = bankId,
                SubjectId = subjectId,
                Title = "EssayBank",
                CreateUserId = userId
            });

            await _context.SaveChangesAsync();

            var questions = new List<AddQuestionRequest>
            {
                new AddQuestionRequest
                {
                    QuestionBankId = bankId,
                    Content = "Explain polymorphism.",
                    Type = 0, // Essay
                    DifficultLevel = 3,
                    Point = 5,
                    CorrectAnswer = "" // Missing sample answer
                }
            };

            var result = await _service.ImportListQuestionAsync(questions, userId);

            Assert.False(result.Success);
            Assert.Contains("Essay must have a sample answer", result.Message);
        }

        [Fact]
        public async Task ImportListQuestionAsync_ShouldFail_WhenQuestionBankNotFound()
        {
            var userId = "admin1";

            _context.Users.Add(new User
            {
                UserId = userId,
                UserRoles = new List<UserRole> { new UserRole { RoleId = (int)RoleEnum.Admin, UserId = userId } }
            });

            await _context.SaveChangesAsync();

            var questions = new List<AddQuestionRequest>
            {
                new AddQuestionRequest
                {
                    QuestionBankId = "nonexistent-id",
                    Content = "What is inheritance?",
                    Type = 1,
                    DifficultLevel = 2,
                    Point = 4,
                    Options = new List<string> { "A", "B", "C" },
                    CorrectAnswer = "A"
                }
            };

            var result = await _service.ImportListQuestionAsync(questions, userId);

            Assert.False(result.Success);
            Assert.Contains("Question bank not found", result.Message);
        }


        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
