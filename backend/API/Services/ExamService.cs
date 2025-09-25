using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using API.Builders;
using API.Commons;
using API.Helper;
using API.Models;
using API.Repository;
using API.Services.Interfaces;
using API.Subjects;
using API.Utilities;
using API.Validators;
using API.Validators.Interfaces;
using API.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class ExamService : IExamService
    {
        private readonly Sep490Context _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExamSubject _subject;
        private readonly IAddExamValidator _validator;

        public ExamService(Sep490Context context, IUnitOfWork unitOfWork)
        {
            _context = context;
            _unitOfWork = unitOfWork;
        }

        //UpdateExam
        public async Task<(bool Success, string Message)> UpdateExamAsync(UpdateExamRequest request, string userId)
        {
            // Check if user has permission
            var userExist = await _context.Users
                .Include(u => u.UserRoles)
                .AnyAsync(u => u.UserId == userId && u.UserRoles.Any(r => r.RoleId == (int)RoleEnum.Lecture || r.RoleId == (int)RoleEnum.Admin));
            if (!userExist)
                return (false, "User does not have permission to update exams.");

            // Check exam existence
            var exam = await _context.Exams
                .Include(e => e.ExamQuestions)
                .FirstOrDefaultAsync(e => e.ExamId == request.ExamId);

            if (exam == null)
                return (false, "Exam not found.");

            // Only creator can update
            if (exam.CreateUser != userId)
                return (false, "You can only update exams you created.");

            // Only allow updates if exam is Inactive
            //if (exam.LiveStatus != ExamLiveStatus.Inactive)
            //    return (false, "Only inactive exams can be updated.");

            // Only allow updates if exam is Draft
            if ((ExamStatus)exam.Status != ExamStatus.Draft)
                return (false, "Only exams is Draft can be updated.");

            //if (exam.VerifyCamera == request.VerifyCamera)
            //    return (false, $"Exam already has VerifyCamera = {request.VerifyCamera}.");

            if (request.EndTime <= request.StartTime)
                return (false, "EndTime must be later than StartTime.");

            // Validate room
            var roomExists = await _context.Rooms.AnyAsync(r => r.RoomId == request.RoomId);
            if (!roomExists)
                return (false, "The selected room does not exist.");
            //var normalized = Regex.Replace(request.Title.Trim().ToLower(), @"\s+", "");
            //return await _context.Exams
            //    .AnyAsync(e => e.CreateUser == userId && e.Title.Trim().ToLower() == normalized);
            //var titleExists = await _context.Exams.AnyAsync(e => Regex.Replace(e.Title.Trim().ToLower(), @"\s+", "") == normalized && e.CreateUser == userId);
            
            var normalized = request.Title.Trim().ToLower()
                                                           .Replace(" ", "")
                                                           .Replace("\t", "")
                                                           .Replace("\n", "");
            var titleExists = await _context.Exams
            .Where(e => e.CreateUser == userId && e.ExamId != request.ExamId)
            .AnyAsync(e => e.Title.Trim().ToLower()
                    .Replace(" ", "")
                    .Replace("\t", "")
                    .Replace("\n", "") == normalized);
            if (titleExists)
                return (false, "An exam with the same title already exists.");
            var bankExists = await _context.QuestionBanks.AnyAsync(qb => qb.QuestionBankId == request.QuestionBankId);
            if (!bankExists)
                return (false, "Question bank does not exist.");

            if (request.QuestionIds?.Any() != true)
                return (false, "No questions selected.");

            if (request.QuestionIds.Distinct().Count() != request.QuestionIds.Count)
                return (false, "Duplicate QuestionIds are not allowed.");

            var selectedQuestions = await _context.Questions.Where(q => request.QuestionIds.Contains(q.QuestionId) &&
                q.QuestionBankId == request.QuestionBankId &&
                q.Status == 1).ToListAsync(); ;

            if (selectedQuestions.Count != request.QuestionIds.Count)
                return (false, "Some selected questions do not belong to the bank or are inactive.");

            var distinctTypes = selectedQuestions.Select(q => q.Type).Distinct().ToList();
            if (distinctTypes.Count > 1)
                return (false, "All selected questions must be of the same type.");

            if (distinctTypes.First() != request.ExamType)
                return (false, $"ExamType mismatch. Selected questions are of type {(QuestionTypeChoose)distinctTypes.First()}, but ExamType is {(QuestionTypeChoose)request.ExamType}.");

            var totalPoints = selectedQuestions.Sum(q => q.Point);
            if (totalPoints == 0)
                return (false, "Total point of selected questions must be greater than 0.");

            var scale = 10.0m / totalPoints;

            // Update exam fields
            exam.Title = request.Title.Trim();
            exam.Description = request.Description;
            exam.RoomId = request.RoomId;
            exam.StartTime = request.StartTime;
            exam.EndTime = request.EndTime;
            exam.Duration = request.Duration;
            exam.IsShowResult = request.IsShowResult;
            exam.IsShowCorrectAnswer = request.IsShowCorrectAnswer;
            exam.Status = request.Status;
            exam.UpdatedAt = DateTime.UtcNow;
            exam.ExamType = request.ExamType;
            exam.TotalQuestions = selectedQuestions.Count;
            exam.TotalPoints = 10;
            exam.GuildeLines = request.GuideLines;
            exam.VerifyCamera = request.VerifyCamera;
            // Remove old exam questions
            _context.ExamQuestions.RemoveRange(exam.ExamQuestions);

            // Add updated questions
            foreach (var question in selectedQuestions)
            {
                var examQuestion = new ExamQuestion
                {
                    ExamQuestionId = Guid.NewGuid().ToString(),
                    ExamId = exam.ExamId,
                    QuestionId = question.QuestionId,
                    Points = Math.Round(question.Point * scale, 2),
                    CreatedAt = DateTime.UtcNow
                };
                _context.ExamQuestions.Add(examQuestion);
            }

            await _context.SaveChangesAsync();

            return (true, $"Exam {(ExamStatus)request.Status} updated successfully.");
        }
        //View Exam Detail
        public async Task<(bool Success, string Message, ExamDetail? Exam)> GetExamDetailAsync(string examId, string userId)
        {
            //var userExist = await _context.Users
            //    .Include(u => u.UserRoles)
            //    .AnyAsync(u => u.UserId == userId && u.UserRoles.Any(r => r.RoleId == (int)RoleEnum.Lecture || r.RoleId == (int)RoleEnum.Admin));

            //if (!userExist)
            //    return (false, "User does not have permission to view exam detail.", null);

            //var exam = await _unitOfWork.Exams.GetExamWithQuestionAndRoom(examId);
            var exam = await _context.Exams.AsNoTracking().Include(e => e.Room).Include(e => e.Creator)
                .Include(e => e.ExamQuestions).ThenInclude(eq => eq.Question).ThenInclude(qb => qb.QuestionBank)
                .FirstOrDefaultAsync(e => e.ExamId == examId);
            if (exam == null) return (false, "Exam not found.", null);

            var examDetail = new ExamDetail
            {
                ExamId = exam.ExamId,
                Title = exam.Title,
                Description = exam.Description,
                RoomName = exam.Room?.RoomCode ?? "(No room)",
                RoomId = exam.RoomId,
                TotalQuestions = exam.TotalQuestions,
                TotalPoints = exam.TotalPoints,
                Duration = exam.Duration,
                StartTime = exam.StartTime,
                EndTime = exam.EndTime,
                CreatedBy = exam.Creator?.FullName ?? "(Unknown)",
                Status = exam.Status,
                IsShowResult = exam.IsShowResult,
                IsShowCorrectAnswer = exam.IsShowCorrectAnswer,
                ExamType = exam.ExamType,
                GuideLines = exam.GuildeLines,
                LiveStatus = exam.LiveStatus.ToString(),
                verifyCamera = exam.VerifyCamera,
                Questions = exam.ExamQuestions.Select(eq => new SelectedQuestionDto
                {
                    QuestionId = eq.Question.QuestionId,
                    Content = eq.Question.Content,
                    Difficulty = (DifficultyLevel)eq.Question.DifficultLevel,
                    Type = (QuestionTypeChoose)eq.Question.Type,
                    QuestionBankId = eq.Question.QuestionBankId,
                    QuestionBankName = eq.Question.QuestionBank?.Title ?? "(Unknown)"
                }).ToList()
            };

            return (true, "Success", examDetail);
        }
        //public async Task<string> Handle(AddExamRequest request, string userId)
        //{
        //    var (isValid, error) = await AddExamValidator.ValidateAsync(request, _unitOfWork);
        //    if (!isValid)
        //        return error;

        //    var bank = await _unitOfWork.QuestionBanks.GetQuestionBankByIdAsync(request.QuestionBankId);
        //    if (bank?.Questions == null || !bank.Questions.Any())
        //        return "Invalid or empty question bank.";

        //    var selectedQuestions = bank.Questions
        //        .Where(q => request.QuestionIds.Contains(q.QuestionId) && q.Status == 1)
        //        .ToList();

        //    if (selectedQuestions.Count() != request.QuestionIds.Count)
        //        return "Some selected questions were not found.";

        //    var distinctTypes = selectedQuestions.Select(q => q.Type).Distinct().ToList();
        //    if (distinctTypes.Count() > 1 || distinctTypes.First() != request.ExamType)
        //        return "All questions must be of the same type and match ExamType.";

        //    var totalPoints = selectedQuestions.Sum(q => q.Point);
        //    if (totalPoints == 0)
        //        return "Total points must be greater than 0.";

        //    var scale = 10.0m / totalPoints;
        //    var examId = Guid.NewGuid().ToString();

        //    await _unitOfWork.BeginTransactionAsync();
        //    try
        //    {
        //        var exam = new ExamBuilder()
        //            .WithBasicInfo(examId, request, userId)
        //            .Build();
        //        await _unitOfWork.Exams.AddAsync(exam);

        //        foreach (var q in selectedQuestions)
        //        {
        //            var examQuestion = new ExamQuestionBuilder()
        //                .SetExamId(examId)
        //                .SetQuestion(q, scale)
        //                .Build();
        //            await _unitOfWork.ExamQuestions.AddAsync(examQuestion);
        //        }

        //        var examSupervisor = new ExamSupervisorBuilder()
        //            .SetExamId(examId)
        //            .SetSupervisorId(null)
        //            .SetCreatedBy(userId)
        //            .Build();
        //        await _unitOfWork.ExamSupervisors.AddAsync(examSupervisor);

        //        await _unitOfWork.SaveChangesAsync();
        //        await _unitOfWork.CommitTransactionAsync();
        //        await _subject.Notify(exam, userId);

        //        return string.Empty;
        //    }
        //    catch (Exception ex)
        //    {
        //        await _unitOfWork.RollbackTransactionAsync();
        //        return $"Error adding exam: {ex.Message}";
        //    }
        //}

        //Add Exam with design pattern
        //public async Task<(bool Success, string Message, List<SelectedQuestionDto>? Questions)> AddExamBuilderPatternAsync(AddExamRequest request, string userId)
        //{
        //    var (isValid, errorMessage, selectedQuestions) = await _validator.ValidateAsync(request, userId);

        //    if (!isValid)
        //        return (false, errorMessage, null);

        //    var totalPoints = selectedQuestions.Sum(q => q.Point);
        //    var scale = 10.0m / totalPoints;
        //    var examId = Guid.NewGuid().ToString();

        //    using var transaction = await _context.Database.BeginTransactionAsync();
        //    try
        //    {
        //        var exam = new ExamBuilder()
        //            .WithBasicInfo(examId, request, userId)
        //            .Build();
        //        await _unitOfWork.Exams.AddAsync(exam);

        //        var selectedDtos = new List<SelectedQuestionDto>();
        //        foreach (var question in selectedQuestions)
        //        {
        //            var examQuestion = new ExamQuestionBuilder()
        //                .SetExamId(examId)
        //                .SetQuestion(question, scale)
        //                .Build();
        //            await _unitOfWork.ExamQuestions.AddAsync(examQuestion);
        //            selectedDtos.Add(new SelectedQuestionDto
        //            {
        //                QuestionId = question.QuestionId,
        //                Content = question.Content.Trim(),
        //                Difficulty = (DifficultyLevel)question.DifficultLevel,
        //                Type = (QuestionTypeChoose)question.Type,
        //                QuestionBankId = question.QuestionBankId,
        //                QuestionBankName = question.QuestionBank?.Title ?? "(Unknown)"
        //            });
        //        }

        //        var examSupervisor = new ExamSupervisorBuilder()
        //            .SetExamId(examId)
        //            .SetSupervisorId(null)
        //            .SetCreatedBy(userId)
        //            .Build();
        //        await _unitOfWork.ExamSupervisors.AddAsync(examSupervisor);
        //        await _unitOfWork.SaveChangesAsync();
        //        await transaction.CommitAsync();
        //        await _subject.Notify(exam, userId);

        //        return (true, $"Exam {(ExamStatus)request.Status} successfully.", selectedDtos);
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        return (false, $"Error adding exam: {ex.Message}", null);
        //    }
        //}
        //List Exams
        public async Task<(bool Success, string Message, SearchResult?)> GetExamListAsync(ExamListRequest request, string userId)
        {
            var userExist = await _context.Users.Include(u => u.UserRoles).AsNoTracking()
                .AnyAsync(u => u.UserId == userId && u.UserRoles.Any(r => r.RoleId == (int)RoleEnum.Lecture || r.RoleId == (int)RoleEnum.Admin));
            if (!userExist) return (false, "User does not a lecture to have permission to view list exams.", null);

            var query = _context.Exams.Include(e => e.Room).Include(e => e.Creator).AsNoTracking().AsQueryable();
            // Search by keyword (by title, room code or creator name)
            if (!string.IsNullOrEmpty(request.TextSearch))
            {
                var search = request.TextSearch.ToLower();
                query = query.Where(e =>
                    (e.Title != null && e.Title.ToLower().Contains(search)) ||
                    (e.Room != null && e.Room.RoomCode.ToLower().Contains(search)) ||
                    (e.Creator != null && e.Creator.FullName.ToLower().Contains(search))
                );
            }
            if (request.Status.HasValue)
                query = query.Where(qb => qb.Status == (int)request.Status);

            if (request.IsMyQuestion.HasValue && request.IsMyQuestion == true)
                query = query.Where(qb => qb.CreateUser == userId);

            if (request.IsExamResult.HasValue && request.IsExamResult == true)
                query = query.Where(e => e.Status == (int)ExamStatus.Finished || e.EndTime <= DateTime.UtcNow);

            var totalCount = await query.CountAsync();
            var totalPage = (int)Math.Ceiling(totalCount / (double)request.PageSize);
            var data = await query
                .OrderByDescending(e => e.CreatedAt)
                .Skip((request.CurrentPage - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(e => new ExamListVM
                {
                    ExamId = e.ExamId,
                    Title = e.Title,
                    Description = e.Description,
                    RoomName = e.Room.RoomCode ?? "(No room)",
                    TotalQuestions = e.TotalQuestions,
                    TotalPoints = e.TotalPoints,
                    Duration = e.Duration,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    CreatedBy = e.Creator.FullName ?? "(Unknown)",
                    CreatedById = e.CreateUser,
                    Status = e.Status,
                    IsShowResult = e.IsShowResult,
                    IsShowCorrectAnswer = e.IsShowCorrectAnswer,
                    ExamType = e.ExamType,
                    GuideLines = e.GuildeLines,
                    LiveStatus = (int)e.LiveStatus,
                    verifyCamera = e.VerifyCamera
                }).ToListAsync();

            return (true, "Success", new SearchResult
            {
                Result = data,
                TotalPage = totalPage,
                CurrentPage = request.CurrentPage,
                PageSize = request.PageSize,
                Total = totalCount
            });
        }
        //Add Exam
        public async Task<(bool Success, string Message, List<SelectedQuestionDto>? Questions)> AddExamAsync(AddExamRequest request, string userId)
        {
            var userExist = await _context.Users
                .Include(u => u.UserRoles)
                .AnyAsync(u => u.UserId == userId && u.UserRoles.Any(r => r.RoleId == (int)RoleEnum.Lecture || r.RoleId == (int)RoleEnum.Admin));

            if (!userExist) return (false, "User does not have permission to add exam.", null);
            var start = request.StartTime;
            var end = request.EndTime;
            if (end <= start)
                return (false, "EndTime must be later than StartTime.", null);
            var roomExists = await _context.Rooms.AnyAsync(r => r.RoomId == request.RoomId);
            if (!roomExists)
                return (false, "The selected room does not exist.", null);
            var normalized = request.Title.Trim().ToLower()
                                                           .Replace(" ", "")
                                                           .Replace("\t", "")
                                                           .Replace("\n", "");
            //return await _context.Exams
            //    .AnyAsync(e => e.CreateUser == userId && e.Title.Trim().ToLower() == normalized);
            //var titleExists = await _context.Exams.AnyAsync(e => Regex.Replace(e.Title.Trim().ToLower(), @"\s+", "") == normalized && e.CreateUser == userId);
            //var titleExists = await _context.Exams.AnyAsync(e => e.CreateUser == userId&& e.Title.Trim().ToLower().Replace(" ", "") == normalized);
            var titleExists = await _context.Exams
            .Where(e => e.CreateUser == userId)
            .AnyAsync(e => e.Title.Trim().ToLower()
                    .Replace(" ", "")
                    .Replace("\t", "")
                    .Replace("\n", "") == normalized);
            if (titleExists)
                return (false, "An exam with the same title already exists.", null);
            List<Models.Question> selectedQuestions;
            var bank = await _context.QuestionBanks
                    .Include(qb => qb.Questions)
                    .FirstOrDefaultAsync(qb => qb.QuestionBankId == request.QuestionBankId);

            if (bank == null)
                return (false, "Question bank not found.", null);

            if (bank.Questions == null || !bank.Questions.Any())
                return (false, "Question bank has no questions.", null);

            // If you choose a bank, only select questions from it.
            var invalidIds = request.QuestionIds
                    .Where(id => bank.Questions.All(q => q.QuestionId != id))
                    .ToList();

            if (invalidIds.Any())
                return (false, "Some selected questions do not belong to the selected question bank.", null);

            selectedQuestions = bank.Questions
                .Where(q => request.QuestionIds.Contains(q.QuestionId) && q.Status == 1)
                .ToList();
            if (request.QuestionIds?.Any() != true)
                return (false, "No questions selected.", null);

            if (request.QuestionIds.Distinct().Count() != request.QuestionIds.Count)
                return (false, "Duplicate QuestionIds are not allowed.", null);

            if (selectedQuestions.Count != request.QuestionIds.Count)
                return (false, "Some selected questions were not found.", null);

            var distinctTypes = selectedQuestions.Select(q => q.Type).Distinct().ToList();
            if (distinctTypes.Count > 1)
                return (false, "All selected questions must be of the same type.", null);

            if (distinctTypes.First() != request.ExamType)
                return (false, $"ExamType mismatch. Selected questions are of type {(QuestionTypeChoose)distinctTypes.First()}, but ExamType is {(QuestionTypeChoose)request.ExamType}. (0: Essay, 1: MultipleChoice)", null);

            var totalPoints = selectedQuestions.Sum(q => q.Point);
            if (totalPoints <= 0)
                return (false, "Total point of selected questions must be greater than 0.", null);

            var scale = 10.0m / totalPoints;
            var examId = Guid.NewGuid().ToString();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var newExam = new Exam
                {
                    ExamId = examId,
                    RoomId = request.RoomId,
                    Title = request.Title.Trim(),
                    Description = request.Description,
                    Duration = request.Duration,
                    TotalPoints = 10,
                    StartTime = start,
                    EndTime = end,
                    TotalQuestions = selectedQuestions.Count,
                    IsShowResult = request.IsShowResult,
                    IsShowCorrectAnswer = request.IsShowCorrectAnswer,
                    Status = request.Status,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreateUser = userId,
                    GuildeLines = request.GuideLines,
                    ExamType = request.ExamType,
                    VerifyCamera = request.VerifyCamera
                };

                _context.Exams.Add(newExam);
                var selectedDtos = new List<SelectedQuestionDto>();
                foreach (var question in selectedQuestions)
                {
                    var examQuestion = new ExamQuestion
                    {
                        ExamQuestionId = Guid.NewGuid().ToString(),
                        ExamId = examId,
                        QuestionId = question.QuestionId,
                        Points = Math.Round(question.Point * scale, 2),
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.ExamQuestions.Add(examQuestion);
                    selectedDtos.Add(new SelectedQuestionDto
                    {
                        QuestionId = question.QuestionId,
                        Content = question.Content,
                        Difficulty = (DifficultyLevel)question.DifficultLevel,
                        Type = (QuestionTypeChoose)question.Type,
                        QuestionBankId = question.QuestionBankId,
                        QuestionBankName = question.QuestionBank?.Title ?? "(Unknown)"
                    });
                }

                var newSupervisor = new ExamSupervisor
                {
                    ExamSupervisorId = Guid.NewGuid().ToString(),
                    ExamId = examId,
                    SupervisorId = null,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };
                _context.ExamSupervisors.Add(newSupervisor);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, $"Exam {(ExamStatus)request.Status} successfully.", selectedDtos);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Error adding exam: {ex.Message}", null);
            }
        }
        //Exam Result Report
        public async Task<(bool Success, string Message, ExamResultVM? Data)> GetExamResultReportAsync(string examId, string userId)
        {
            // Check permission: user must be lecturer or admin
            var userExist = await _context.Users.AsNoTracking().Include(u => u.UserRoles)
                .AnyAsync(u => u.UserId == userId && u.UserRoles.Any(r => r.RoleId == (int)RoleEnum.Lecture || r.RoleId == (int)RoleEnum.Admin));
            if (!userExist) return (false, "User does not have permission to view exam results.", null);

            var now = DateTime.UtcNow;
            var exam = await _context.Exams.Include(e => e.Room).Include(e => e.Creator).AsNoTracking()
                .FirstOrDefaultAsync(e => e.ExamId == examId);
            if (exam == null) return (false, "Exam not found or not finished.", null);

            var studentExams = await _context.StudentExams
                .Include(se => se.User)
                .ThenInclude(u => u.RoomUsers)
                .ThenInclude(ru => ru.Room)
                    .Include(se => se.Exam)
                .Where(se => se.ExamId == examId)
                .ToListAsync();

            int totalStudents = studentExams.Count;
            int totalCompleted = studentExams.Count(se => se.Status != (int)StudentExamStatus.NotStarted && se.Status != (int)StudentExamStatus.InProgress);
            int totalGraded = studentExams.Count(se => se.Score.HasValue);
            decimal averageScore = totalGraded > 0 ? Math.Round(studentExams.Where(se => se.Score.HasValue).Average(se => se.Score.Value), 2) : 0;
            var averageTimeMinutes = studentExams.Where(se => se.StartTime.HasValue && se.SubmitTime.HasValue)
                .Select(se => (se.SubmitTime.Value - se.StartTime.Value).TotalMinutes)
                .DefaultIfEmpty(0)
                .Average();
            bool isMultipleChoice = exam.ExamType == (int)QuestionTypeChoose.MultipleChoice;

            var studentResultList = studentExams.Select(se => new StudentExamResult
            {
                StudentExamId = se.StudentExamId,
                IsMarked = se.Status == (int)StudentExamStatus.Failed || se.Status == (int)StudentExamStatus.Passed || (isMultipleChoice && se.Status == (int)StudentExamStatus.Submitted),
                FullName = se.User?.FullName ?? "(Unknown)",
                ClassName = se.User?.RoomUsers.FirstOrDefault(r => r.RoomId == se.Exam.RoomId)?.Room?.RoomCode ?? "(N/A)",
                Score = se.Score,
                SubmitTime = se.SubmitTime?.ToString("HH:mm") ?? "In Progress",
                Status = Common.IsCompleted(se.Status) ? "Submitted" : "Not Submitted",
                WorkingTime = (se.SubmitTime.HasValue && se.StartTime.HasValue) ? $"{Math.Round((se.SubmitTime.Value - se.StartTime.Value).TotalMinutes)} minutes" : "In Progress",
            }).ToList();

            var examResult = new ExamResultVM
            {
                SubjectName = exam.Room?.RoomCode ?? "(No room)",
                ExamDate = exam.StartTime.ToString("dd/MM/yyyy"),
                DurationMinutes = exam.Duration,
                QuestionType = ((QuestionTypeChoose)exam.ExamType).ToString(),
                TotalPoints = exam.TotalPoints,
                CreatedBy = exam.Creator?.FullName ?? "(Unknown)",
                TotalStudents = totalStudents,
                AverageScore = averageScore,
                AverageDuration = $"{Math.Round(averageTimeMinutes)} minutes",
                StudentResults = studentResultList,
                LiveStatus = exam.LiveStatus.ToString(),
            };

            return (true, "Success", examResult);
        }
        //Export Exam Result Report
        public async Task<(string, MemoryStream?)> ExportStudentExamResultReport(string examId, string userId)
        {
            var userExist = await _context.Users.AsNoTracking().Include(u => u.UserRoles)
                .AnyAsync(u => u.UserId == userId && u.UserRoles.Any(r => r.RoleId == (int)RoleEnum.Lecture || r.RoleId == (int)RoleEnum.Admin));
            if (!userExist) return ("User does not have permission to export exam result report.", null);

            var studentExams = await _context.StudentExams
                .Include(se => se.User).ThenInclude(u => u.RoomUsers).ThenInclude(ru => ru.Room)
                .Include(se => se.Exam)
                .Where(se => se.ExamId == examId)
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync();

            var studentResultList = studentExams.Select(se => new StudentExamResult
            {
                StudentExamId = se.StudentExamId,
                IsMarked = se.Status == (int)StudentExamStatus.Failed || se.Status == (int)StudentExamStatus.Passed || (se.Exam.ExamType == (int)QuestionTypeChoose.MultipleChoice && se.Status == (int)StudentExamStatus.Submitted),
                FullName = se.User?.FullName ?? "(Unknown)",
                ClassName = se.User?.RoomUsers.FirstOrDefault(r => r.RoomId == se.Exam.RoomId)?.Room?.RoomCode ?? "(N/A)",
                Score = se.Score ?? 0,
                SubmitTime = se.SubmitTime?.ToString("HH:mm") ?? "In Progress",
                Status = Common.IsCompleted(se.Status) ? "Submitted" : "Not Submitted",
                WorkingTime = (se.SubmitTime.HasValue && se.StartTime.HasValue) ? $"{Math.Round((se.SubmitTime.Value - se.StartTime.Value).TotalMinutes)} minutes" : "In Progress",

            }).ToList();

            if (studentResultList == null || !studentResultList.Any())
                return ("No student exam results found.", null);

            var file = FileHandler.GenerateExcelFile(studentResultList);
            if (file == null) return ("Error exporting exam results.", null);

            return ("", file);
        }
        //Student Exam Detail
        public async Task<(bool Success, string Message, StudentExamDetailDto? Data)> GetStudentExamDetailAsync(string studentExamId, string userId)
        {
            var isAuthorized = await _context.Users
                .Include(u => u.UserRoles)
                .AnyAsync(u => u.UserId == userId && u.UserRoles.Any(r => r.RoleId == (int)RoleEnum.Admin || r.RoleId == (int)RoleEnum.Lecture));

            if (!isAuthorized)
                return (false, "User does not have permission to student exam detail.", null);

            var studentExam = await _context.StudentExams.AsNoTracking()
                .Include(se => se.User)
                .ThenInclude(u => u.RoomUsers)
                .ThenInclude(ru => ru.Room)
                .Include(se => se.Exam)
                .Include(se => se.StudentAnswers).ThenInclude(sa => sa.Question)
                .FirstOrDefaultAsync(se => se.StudentExamId == studentExamId);

            if (studentExam == null)
                return (false, "Student exam not found", null);

            var correctCount = studentExam.StudentAnswers.Count(a => a.IsCorrect == true);
            var wrongCount = studentExam.StudentAnswers.Count(a => a.IsCorrect == false);
            var timeTaken = $"{Math.Round((studentExam.SubmitTime.Value - studentExam.StartTime.Value).TotalMinutes)} minutes";
            var answerDetails = studentExam.StudentAnswers.Select(a => new StudentAnswerDetailDto
            {
                QuestionContent = a.Question.Content,
                Options = JsonSerializer.Deserialize<List<string>>(a.Question.Options) ?? new(),
                UserAnswer = a.UserAnswer ?? "",
                CorrectAnswer = a.Question.CorrectAnswer,
                IsCorrect = a.IsCorrect ?? false,
                PointsEarned = a.PointsEarned ?? 0,
                TimeSpent = a.TimeSpent ?? 0,
                Explanation = a.Question.Explanation
            }).ToList();

            var dto = new StudentExamDetailDto
            {
                StudentName = studentExam.User?.FullName ?? "(Unknown)",
                ClassName = studentExam.User?.RoomUsers.FirstOrDefault(r => r.RoomId == studentExam.Exam.RoomId)?.Room?.RoomCode ?? "(N/A)",
                StudentCode = studentExam.User?.UserCode ?? "(Unknown)",
                ExamTitle = studentExam.Exam?.Title ?? "Unknown)",
                ExamDate = studentExam.Exam?.StartTime ?? DateTime.UtcNow,
                TotalQuestions = studentExam.TotalQuestions ?? 0,
                TotalPoints = studentExam.Exam?.TotalPoints ?? 10,
                Duration = studentExam.Exam?.Duration ?? 0,
                Score = studentExam.Score ?? 0,
                CorrectAnswers = correctCount,
                WrongAnswers = wrongCount,
                TimeTaken = timeTaken,
                Answers = answerDetails
            };

            return (true, "Success", dto);
        }
        //Change exam status
        public async Task<(bool Success, string Message)> ChangeExamStatusAsync(string examId, int newStatus, string userId)
        {
            var userExist = await _context.Users
                .Include(u => u.UserRoles)
                .AnyAsync(u => u.UserId == userId && u.UserRoles.Any(r =>
                    r.RoleId == (int)RoleEnum.Lecture || r.RoleId == (int)RoleEnum.Admin));

            if (!userExist)
                return (false, "User does not have permission to change exam status.");

            var exam = await _context.Exams.FirstOrDefaultAsync(e => e.ExamId == examId);
            if (exam == null)
                return (false, "Exam not found.");

            var liveStatus = exam.LiveStatus;
            var currentStatus = (ExamStatus)exam.Status;

            if (liveStatus == ExamLiveStatus.Ongoing)
                return (false, "Cannot change status while the exam is ongoing.");

            if (liveStatus == ExamLiveStatus.Completed && (ExamStatus)newStatus == ExamStatus.Published)
                return (false, "Cannot publish an exam that has already been completed.");

            //if (currentStatus == ExamStatus.Finished && (ExamStatus)newStatus == ExamStatus.Published)
            //    return (false, "Cannot publish a finished exam.");

            if (currentStatus == (ExamStatus)newStatus)
                return (false, $"Exam is already in status: {(ExamStatus)newStatus}.");

            exam.Status = newStatus;
            exam.UpdatedAt = DateTime.UtcNow;
            exam.UpdateUser = userId;

            await _context.SaveChangesAsync();
            return (true, $"Exam status changed to {(ExamStatus)newStatus} successfully.");
        }

        //Get Guideline
        public async Task<(bool Success, string Message, string? GuideLines)> GetExamGuideLinesAsync(string examId, string userId)
        {
            //var userExist = await _context.Users
            //    .Include(u => u.UserRoles)
            //    .AnyAsync(u => u.UserId == userId && u.UserRoles.Any(r => r.RoleId == (int)RoleEnum.Lecture));

            //if (!userExist)
            //    return (false, "User does not have permission to view exam guide lines.", null);

            //var exam = await _unitOfWork.Exams.GetById(examId);
            var exam = await _context.Exams.AsNoTracking().FirstOrDefaultAsync(e => e.ExamId.ToLower() == examId.ToLower());
            if (exam == null) return (false, "Exam not found.", null);

            return (true, "Success", exam.GuildeLines ?? "");
        }
        public async Task<(string, ExamOtpVM?)> AssignOTP(CreateExamOtpVM input, string userId)  // add builder
        {
            var now = DateTime.UtcNow;
            var exam = await _unitOfWork.Exams.GetWithRoomById(input.ExamId);
            if (exam == null || exam.Status != (int)ExamStatus.Published) return ("Exam not found or not active.", null);
            if (exam.EndTime < now) return ("Exam has already ended.", null);

            var otp = await _unitOfWork.ExamOTPs.GetByExamId(input.ExamId);
            var newOtpCode = Utils.Generate6Number();

            if (otp == null)
            {
                otp = new ExamOtp
                {
                    ExamOtpId = Guid.NewGuid().ToString(),
                    ExamId = exam.ExamId,
                    CreatedBy = userId
                };
                await _unitOfWork.ExamOTPs.AddAsync(otp);
            }
            else
            {
                otp.OtpCode = newOtpCode;
                otp.CreatedAt = now;
                otp.ExpiredAt = now.AddSeconds(input.TimeValid);
                otp.TimeValid = input.TimeValid;
                _unitOfWork.ExamOTPs.Update(otp);
            }
            await _unitOfWork.SaveChangesAsync();
            return ("", new ExamOtpVM
            {
                ExamOtpId = otp.ExamOtpId,
                ExamId = otp.ExamId,
                OtpCode = newOtpCode.ToString(),
                TimeValid = otp.TimeValid,
                ExpiredAt = otp.ExpiredAt
            });
        }
    }
}
