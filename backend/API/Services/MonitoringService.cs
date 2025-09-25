using API.Commons;
using API.Factory;
using API.Helper;
using API.Hubs;
using API.Models;
using API.Repository;
using API.Services.Interfaces;
using API.Subjects;
using API.ViewModels;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class MonitoringService : IMonitoringService
    {
        private readonly Sep490Context _context;
        private readonly IHubContext<ExamHub> _examHub;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAmazonS3Service _s3Service;
        private readonly IMonitoringSubject _subject;
        private readonly ILog _log;
        private readonly IScoringStrategyFactory _scoringStrategy;

        public MonitoringService(Sep490Context context, ILog log, IHubContext<ExamHub> examHub, IAmazonS3Service s3Service,
            IUnitOfWork unitOfWork, IMonitoringSubject monitoringSubject, IScoringStrategyFactory scoringStrategy)
        {
            _context = context;
            _log = log;
            _unitOfWork = unitOfWork;
            _examHub = examHub;
            _subject = monitoringSubject;
            _s3Service = s3Service;
            _scoringStrategy = scoringStrategy;
        }

        public async Task<(string, SearchResult?)> GetExamOverview(MonitorExamSearchVM search, string usertoken)
        {
            var isAdmin = await _context.Users.AsNoTracking().Where(u => u.UserId == usertoken).SelectMany(u => u.UserRoles)
                    .AnyAsync(r => r.RoleId == (int)RoleEnum.Admin);

            var permittedExamIds = await _context.ExamSupervisors.Where(x => (isAdmin || x.SupervisorId == usertoken || x.CreatedBy == usertoken))
                 .Select(x => x.ExamId).Distinct().AsNoTracking().ToListAsync();
            if (!permittedExamIds.Any()) return ("You do not have the right to supervise any exams.", null);

            var query = _context.Exams
                .Include(e => e.Creator)
                .Include(e => e.Room).ThenInclude(r => r.RoomUsers)
                .Include(e => e.Room).ThenInclude(r => r.Class)
                .Include(e => e.Room).ThenInclude(r => r.Subject)
                .Where(e => permittedExamIds.Contains(e.ExamId) && e.Status != (int)ExamStatus.Draft)
                .AsSplitQuery()
                .AsNoTracking()
                .AsQueryable();

            if (!search.SubjectId.IsEmpty())
                query = query.Where(e => e.Room!.SubjectId == search.SubjectId);

            if (search.ExamStatus.HasValue)
            {
                query = search.ExamStatus switch
                {
                    ExamStatus.Published => query.Where(e => e.StartTime <= DateTime.UtcNow && e.EndTime >= DateTime.UtcNow),
                    ExamStatus.Finished => query.Where(e => e.EndTime < DateTime.UtcNow || e.Status == (int)ExamStatus.Finished),
                    _ => query
                };
            }
            if (!search.TextSearch.IsEmpty())
            {
                var keywords = search.TextSearch!
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(x => x.Trim().ToLower())
                    //.Where(k => !string.IsNullOrEmpty(k))
                    //.Take(5) // Giới hạn 5 từ khóa
                    .ToList();

                foreach (var keyword in keywords)
                {
                    var keywordPattern = $"%{keyword}%";
                    query = query.Where(e =>
                        EF.Functions.Like((e.Title ?? "").ToLower(), keywordPattern) ||
                        EF.Functions.Like((e.Room!.RoomCode ?? "").ToLower(), keywordPattern) ||
                        EF.Functions.Like((e.Room.Class!.ClassCode ?? "").ToLower(), keywordPattern) ||
                        EF.Functions.Like((e.Room.Subject!.SubjectName ?? "").ToLower(), keywordPattern) ||
                        EF.Functions.Like((e.Creator!.Email ?? "").ToLower(), keywordPattern) ||
                        EF.Functions.Like((e.Creator.FullName ?? "").ToLower(), keywordPattern)
                    );
                }
            }
            if (!query.Any()) return ("No found any exam.", null);

            var totalCount = await query.CountAsync();
            var totalPage = (int)Math.Ceiling(totalCount / (double)search.PageSize);

            var exams = await query.OrderByDescending(e => e.StartTime)
                .Skip((search.CurrentPage - 1) * search.PageSize)
                .Take(search.PageSize).ToListAsync();

            // Tập hợp tất cả ExamIds trong trang để truy vấn StudentExams 1 lần
            var examIds = exams.Select(e => e.ExamId).ToList();

            var studentExams = await _context.StudentExams.AsNoTracking().Where(se => examIds.Contains(se.ExamId)).ToListAsync();
            //if (studentExams.IsObjectEmpty())
            //    return ("There are no StudentExam in those filtered exams.", null);
            int studentDoingCount = 0;
            int studentCompletedCount = 0;
            var result = exams.Select(e =>
            {
                var roomUsers = e.Room?.RoomUsers.Where(ru => ru.Status == (int)ActiveStatus.Active).ToList() ?? new List<RoomUser>();
                if (!studentExams.IsObjectEmpty())
                {
                    studentDoingCount = roomUsers.Count(ru => studentExams.Any(se => se.ExamId == e.ExamId &&
                            se.StudentId == ru.UserId && se.Status == (int)StudentExamStatus.InProgress));
                    studentCompletedCount = roomUsers.Count(ru => studentExams.Any(se => se.ExamId == e.ExamId &&
                          se.StudentId == ru.UserId && se.Status == (int)StudentExamStatus.Submitted));
                }
                return new MonitorExamsVM
                {
                    ExamId = e.ExamId,
                    RoomId = e.Room?.RoomId ?? "",
                    RoomCode = e.Room?.RoomCode ?? "",
                    ClassId = e.Room?.ClassId ?? "",
                    ClassCode = e.Room?.Class?.ClassCode ?? "",
                    SubjectId = e.Room?.SubjectId ?? "",
                    SubjectName = e.Room?.Subject?.SubjectName ?? "",

                    Title = e.Title,
                    Description = e.Description,
                    Duration = e.Duration,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    ExamType = e.ExamType,
                    Status = (e.StartTime <= DateTime.UtcNow && e.EndTime >= DateTime.UtcNow)
                                ? (int)ExamStatus.Published
                                : (e.EndTime < DateTime.UtcNow || e.Status == (int)ExamStatus.Finished)
                                    ? (int)ExamStatus.Finished
                                    : (int)ExamStatus.Draft,
                    IsCompleted = e.EndTime < DateTime.UtcNow || e.Status == (int)ExamStatus.Finished,

                    CreateUserId = e.Creator?.UserId ?? "",
                    CreateUserName = e.Creator?.FullName ?? "",
                    CreateEmail = e.Creator?.Email ?? "",
                    MaxCapacity = roomUsers.Count,
                    StudentDoing = studentDoingCount,
                    StudentCompleted = studentCompletedCount,
                    StudentIds = roomUsers.Select(ru => ru.UserId).ToList()
                };
            }).ToList();
            return ("", new SearchResult
            {
                Result = result,
                TotalPage = totalPage,
                CurrentPage = search.CurrentPage,
                PageSize = search.PageSize,
                Total = totalCount
            });
        }
        public async Task<(string, SearchResult?)> GetExamMonitorDetail(MonitorExamDetailSearchVM search, string usertoken)
        {
            var isAdmin = await _context.Users.Include(u => u.UserRoles).AsNoTracking()
                .AnyAsync(u => u.UserId == usertoken && u.UserRoles.Any(r => r.RoleId == (int)RoleEnum.Admin));

            var hasPermission = await _context.ExamSupervisors.AnyAsync(es =>
                es.ExamId == search.ExamId && (isAdmin || es.SupervisorId == usertoken || es.CreatedBy == usertoken));
            if (!hasPermission)
                return ("You do not have permission to view this exam.", null);

            var query = _context.StudentExams.Include(se => se.User)
                 .Include(se => se.Exam).ThenInclude(e => e.Room).ThenInclude(r => r.Subject)
                 .Include(se => se.Exam).ThenInclude(e => e.Room).ThenInclude(r => r.RoomUsers)
                 .Include(se => se.Exam).ThenInclude(e => e.Creator)
                 .Where(se => se.ExamId == search.ExamId)
                 .AsSplitQuery()
                 .AsNoTracking();

            if (search.StudentExamStatus.HasValue)
                query = query.Where(se => se.Status == (int)search.StudentExamStatus.Value);

            if (!search.TextSearch.IsEmpty())
            {
                var text = search.TextSearch!.ToLower().Trim();
                query = query.Where(c => (c.User.UserCode!.ToLower() ?? "").Contains(text)
                || (c.User.FullName!.ToLower() ?? "").Contains(text)
                || (c.User.Email!.ToLower() ?? "").Contains(text));
            }

            var totalCount = await query.CountAsync();
            var totalPage = (int)Math.Ceiling(totalCount / (double)search.PageSize);

            var studentExams = await query
                .OrderByDescending(se => se.StartTime)
                .Skip((search.CurrentPage - 1) * search.PageSize)
                .Take(search.PageSize)
                .ToListAsync();
            var firstExam = studentExams.FirstOrDefault()?.Exam;
            if (firstExam == null) return ("No students are taking the exam yet..", null);

            var studentExamIds = studentExams.Select(se => se.StudentExamId).ToList();
            var answeredCounts = await _context.StudentAnswers.AsNoTracking()
                .Where(sa => studentExamIds.Contains(sa.StudentExamId) && !string.IsNullOrEmpty(sa.UserAnswer))
                .GroupBy(sa => sa.StudentExamId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);

            var violationCounts = await _context.StudentViolations.AsNoTracking()
                .Where(sv => studentExamIds.Contains(sv.StudentExamId))
                .GroupBy(sv => sv.StudentExamId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);

            var warningCounts = await _context.FaceCaptures.AsNoTracking()
                .Where(sw => studentExamIds.Contains(sw.StudentExamId))
                .GroupBy(sw => sw.StudentExamId)
                .Select(g => new { g.Key, Count = g.Count(x => x.IsDetected == false) })
                .ToDictionaryAsync(x => x.Key, x => x.Count);

            var monitorExamVm = new MonitorExamVM
            {
                ExamId = firstExam.ExamId,
                SubjectName = firstExam.Room?.Subject?.SubjectName ?? "",
                RoomCode = firstExam.Room?.RoomCode ?? "",
                Title = firstExam.Title ?? "",
                Duration = firstExam.Duration,
                ExamStartTime = firstExam.StartTime,
                ExamEndTime = firstExam.EndTime,
                ExamType = firstExam.ExamType,
                ExamLive = firstExam.LiveStatus.ToString(),
                CreateUserName = firstExam.Creator?.FullName ?? "",
                CreateEmail = firstExam.Creator?.Email ?? "",
                MaxCapacity = firstExam.Room?.RoomUsers?.Count(ru => ru.Status == (int)ActiveStatus.Active) ?? 0,
                StudentDoing = studentExams.Count(se => se.Status == (int)StudentExamStatus.InProgress),
                StudentCompleted = studentExams.Count(se => se.Status == (int)StudentExamStatus.Submitted),
                Students = studentExams.Select(se => new MonitorExamDetailVM
                {
                    StudentExamId = se.StudentExamId,
                    UserId = se.StudentId,
                    FullName = se.User?.FullName ?? "",
                    UserCode = se.User?.UserCode ?? "",
                    Email = se.User?.Email ?? "",
                    IpAddress = se.IpAddress,
                    BrowserInfo = se.BrowserInfo,
                    StartTime = se.StartTime ?? DateTime.UtcNow,
                    SubmitTime = se.SubmitTime,
                    StudentExamStatus = se.Status ?? (int)StudentExamStatus.InProgress,
                    TotalQuestions = se.Exam?.TotalQuestions ?? 0,
                    AnsweredQuestions = answeredCounts.TryGetValue(se.StudentExamId, out var count) ? count : 0,
                    Score = se.Score,
                    ViolinCount = violationCounts.TryGetValue(se.StudentExamId, out var vCount) ? vCount : 0,
                    WarningCount = warningCounts.TryGetValue(se.StudentExamId, out var wCount) ? wCount : 0,
                }).ToList()
            };

            return ("", new SearchResult
            {
                Result = monitorExamVm,
                TotalPage = totalPage,
                CurrentPage = search.CurrentPage,
                PageSize = search.PageSize,
                Total = totalCount
            });
        }

        public async Task<string> AddStudentExtraTime(StudentExamExtraTime time, string usertoken)
        {
            using var trans = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var studentExam = await _unitOfWork.StudentExams.GetStudentExamWithExamUser(time.StudentExamId, StudentExamStatus.InProgress);
                if (studentExam == null) return "Student is not taking in exam.";

                studentExam.ExtraTimeMinutes = (studentExam.ExtraTimeMinutes ?? 0) + time.ExtraMinutes;

                var newSubmitTime = studentExam.StartTime?.AddMinutes(studentExam.Exam!.Duration + studentExam.ExtraTimeMinutes.Value);
                studentExam.SubmitTime = newSubmitTime;
                studentExam.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.StudentExams.Update(studentExam);
                await _unitOfWork.SaveChangesAsync();

                await _subject.Notify(time, studentExam, usertoken);
                await _unitOfWork.CommitTransactionAsync();
                return ("");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ($"Error add extra time for student: {ex.Message}");
            }
        }

        public async Task<string> AddExamExtraTime(ExamExtraTime time, string usertoken)
        {
            using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                var now = DateTime.UtcNow;
                var studentExams = await _context.StudentExams.Include(se => se.Exam).Include(se => se.User)
                    .Where(se => se.ExamId == time.ExamId && se.Exam!.RoomId == time.RoomId &&
                    se.Status == (int)StudentExamStatus.InProgress && se.StartTime > now && se.SubmitTime < now)
                    .ToListAsync();
                if (!studentExams.Any()) return "No student is currently taking this exam.";

                foreach (var se in studentExams)
                {
                    se.ExtraTimeMinutes = (se.ExtraTimeMinutes ?? 0) + time.ExtraMinutes;
                    var newSubmitTime = se.StartTime?.AddMinutes(se.Exam!.Duration + se.ExtraTimeMinutes.Value);
                    se.SubmitTime = newSubmitTime;
                    se.UpdatedAt = now;

                    if (newSubmitTime.HasValue)
                    {
                        await _examHub.Clients.Group(se.StudentExamId)
                            .SendAsync(ExamHub.RECEIVE_EXAM_EXTRA_TIME, new
                            {
                                StudentExamId = se.StudentExamId,
                                NewSubmitTime = newSubmitTime,
                                ExtraMinutes = time.ExtraMinutes
                            });
                    }
                }
                _context.StudentExams.UpdateRange(studentExams);
                await _context.SaveChangesAsync();
                var msg = await _log.WriteActivity(new AddUserLogVM
                {
                    ActionType = "AddExtraTime",
                    UserId = usertoken,
                    Description = $"Added {time.ExtraMinutes} minutes of extra time to all student for exam {studentExams?.FirstOrDefault()?.Exam?.Title}",
                    Metadata = "RoomId: " + time.RoomId.ToString() + "ExamId: " + time.ExamId.ToString() + " \n " + time.ExtraMinutes + "minutes",
                    ObjectId = time.ExamId,
                    Status = (int)LogStatus.Success
                });
                if (msg.Length > 0) return (msg);

                await trans.CommitAsync();
                return "";
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                return $"Error add extra time for students: {ex.Message}";
            }
        }
        public async Task<string> FinishExam(FinishExam finish, string usertoken)
        {
            using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                var now = DateTime.UtcNow;
                var studentExams = await _context.StudentExams.Include(se => se.Exam)
                    .Where(se => se.ExamId == finish.ExamId && se.Status == (int)StudentExamStatus.InProgress)
                    .ToListAsync();
                if (!studentExams.Any())
                {
                    foreach (var studentExam in studentExams)
                    {
                        var allAnswers = await _context.StudentAnswers
                            .Where(sa => sa.StudentExamId == studentExam.StudentExamId)
                            .ToListAsync();

                        decimal totalScore = 0;
                        if (studentExam.Exam?.ExamType != (int)QuestionTypeChoose.Essay)
                        {
                            var examQuestions = await _context.ExamQuestions.Include(eq => eq.Question)
                                .Where(eq => eq.ExamId == studentExam.ExamId && allAnswers.Select(a => a.QuestionId).Contains(eq.QuestionId))
                                .ToDictionaryAsync(eq => eq.QuestionId, eq => eq);

                            foreach (var answer in allAnswers)
                            {
                                if (examQuestions.TryGetValue(answer.QuestionId, out var eq))
                                {
                                    var question = eq.Question!;
                                    var studentAns = answer.UserAnswer?.Trim().ToLowerInvariant();
                                    var correctAns = question.CorrectAnswer?.Trim().ToLowerInvariant();

                                    var isCorrect = studentAns == correctAns;
                                    answer.IsCorrect = isCorrect;
                                    answer.PointsEarned = isCorrect ? eq.Points : 0;
                                    answer.UpdatedAt = now;

                                    totalScore += answer.PointsEarned ?? 0;
                                }
                            }
                            _context.StudentAnswers.UpdateRange(allAnswers);
                        }
                        studentExam.Status = (int)StudentExamStatus.Submitted;
                        studentExam.Score = totalScore;
                        studentExam.SubmitTime = now;
                        studentExam.UpdatedAt = now;
                        studentExam.TotalQuestions = allAnswers.Count(a => !a.QuestionId.IsEmpty());

                        _context.StudentExams.Update(studentExam);
                    }
                }
                var exam = await _context.Exams.FindAsync(finish.ExamId);
                if (exam != null)
                {
                    exam.Status = (int)ExamStatus.Finished;
                    exam.UpdatedAt = now;
                    _context.Exams.Update(exam);
                }
                var msg = await _log.WriteActivity(new AddUserLogVM
                {
                    ActionType = "FinishExam",
                    UserId = usertoken,
                    Description = $"Finished exam {exam?.Title} with {studentExams.Count} student(s).",
                    Metadata = finish.ExamId.ToString(),
                    ObjectId = finish.ExamId,
                    Status = (int)LogStatus.Success
                });
                if (msg.Length > 0) return (msg);

                var result = await _context.SaveChangesAsync();
                await trans.CommitAsync();
                await _examHub.Clients.Group(finish.ExamId).SendAsync(ExamHub.FINISH_EXAM, new
                {
                    ExamId = finish.ExamId,
                    Success = result != 0,
                });
                return "";
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                return ($"Error during finish exam: {ex.Message}");
            }
        }

        public async Task<string> FinishStudentExam1(FinishStudentExam finish, string usertoken)
        {
            using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                var now = DateTime.UtcNow;
                var studentExam = await _context.StudentExams.Include(se => se.Exam)
                    .FirstOrDefaultAsync(se => se.StudentExamId == finish.StudentExamId && se.ExamId == finish.ExamId
                        && se.Status == (int)StudentExamStatus.InProgress);
                if (studentExam == null) return "Student exam not found or already submitted.";

                var allAnswers = await _context.StudentAnswers
                    .Where(sa => sa.StudentExamId == studentExam.StudentExamId)
                    .ToListAsync();

                decimal totalScore = 0;
                if (studentExam.Exam?.ExamType != (int)QuestionTypeChoose.Essay)
                {
                    var examQuestions = await _context.ExamQuestions.Include(eq => eq.Question)
                        .Where(eq => eq.ExamId == studentExam.ExamId && allAnswers.Select(a => a.QuestionId).Contains(eq.QuestionId))
                        .ToDictionaryAsync(eq => eq.QuestionId, eq => eq);

                    foreach (var answer in allAnswers)
                    {
                        if (examQuestions.TryGetValue(answer.QuestionId, out var eq))
                        {
                            var question = eq.Question!;
                            var studentAns = answer.UserAnswer?.Trim().ToLowerInvariant();
                            var correctAns = question.CorrectAnswer?.Trim().ToLowerInvariant();

                            var isCorrect = studentAns == correctAns;
                            answer.IsCorrect = isCorrect;
                            answer.PointsEarned = isCorrect ? eq.Points : 0;
                            answer.UpdatedAt = now;

                            totalScore += answer.PointsEarned ?? 0;
                        }
                    }
                    _context.StudentAnswers.UpdateRange(allAnswers);
                }
                studentExam.Status = (int)StudentExamStatus.Submitted;
                studentExam.Score = totalScore;
                studentExam.SubmitTime = now;
                studentExam.UpdatedAt = now;
                studentExam.TotalQuestions = allAnswers.Count(a => !a.QuestionId.IsEmpty());

                _context.StudentExams.Update(studentExam);
                await _context.SaveChangesAsync();

                var msg = await _log.WriteActivity(new AddUserLogVM
                {
                    ActionType = "FinishStudentExam",
                    UserId = usertoken,
                    Description = $"Student {studentExam.User?.UserCode} finished Exam: {studentExam.Exam?.Title}.",
                    Metadata = studentExam.StudentId.ToString(),
                    ObjectId = studentExam.StudentExamId,
                    Status = (int)LogStatus.Success
                });
                if (msg.Length > 0) return (msg);
                await trans.CommitAsync();
                await _examHub.Clients.Group(studentExam.StudentExamId).SendAsync(ExamHub.FINISH_STUDENT_EXAM, new
                {
                    ExamId = studentExam.ExamId,
                    StudentExamId = studentExam.StudentExamId,
                    Success = true
                });
                return "";
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                return ($"Error during finish student exam: {ex.Message}");
            }
        }
        public async Task<string> FinishStudentExam(FinishStudentExam finish, string userToken)
        {
            using var trans = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var now = DateTime.UtcNow;
                var studentExam = await _unitOfWork.StudentExams.GetStudentExamWithExamUser(finish.StudentExamId, StudentExamStatus.InProgress);
                if (studentExam == null) return "Student exam not found or already submitted.";

                var allAnswers = await _unitOfWork.StudentAnswers.GetByStudentExamId(studentExam.ExamId);
                var examQuestions = await _unitOfWork.ExamQuestions.GetByExamAndQuestionIds(studentExam.ExamId, allAnswers.Select(a => a.QuestionId).ToList());

                var strategy = _scoringStrategy.GetStrategy((QuestionTypeChoose)studentExam.Exam!.ExamType);
                var (totalScore, status) = await strategy.ScoreAnswers(allAnswers, examQuestions, now, _unitOfWork.StudentAnswers);

                studentExam.Status = (int)StudentExamStatus.Submitted;
                studentExam.Score = totalScore;
                studentExam.SubmitTime = now;
                studentExam.UpdatedAt = now;
                studentExam.TotalQuestions = allAnswers.Count(a => !a.QuestionId.IsEmpty());

                _unitOfWork.StudentExams.Update(studentExam);
                await _unitOfWork.SaveChangesAsync();

                await _subject.Notify(finish, studentExam, userToken);
                await _unitOfWork.CommitTransactionAsync();
                return "";
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return "An error occurred during submission: " + ex.Message;
            }
        }
        public async Task<(string, object?)> ReAssignExam(ReAssignExam assignExam, string usertoken)
        {
            using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                var now = DateTime.UtcNow;
                var exam = await _context.Exams.FirstOrDefaultAsync(e => e.ExamId == assignExam.ExamId && e.Status != (int)ExamStatus.Draft && e.StartTime <= now && e.EndTime >= now);
                if (exam == null) return ("Exam not found or not ongoing.", null);

                // Lấy các StudentExam hiện tại
                var studentIds = assignExam.StudentIds.ToList();
                var existingExams = await _context.StudentExams
                    .Where(se => se.ExamId == assignExam.ExamId && studentIds.Contains(se.StudentId)).ToListAsync();

                var cannotReassign = new List<string>();
                var answersToDelete = new List<StudentAnswer>();
                var examsToDelete = new List<StudentExam>();
                foreach (var studentId in assignExam.StudentIds)
                {
                    var existingExam = existingExams.FirstOrDefault(se => se.StudentId == studentId);
                    if (existingExam == null)
                    {
                        cannotReassign.Add(studentId);
                        continue;
                    }
                    // Thu thập câu trả lời và bài thi cần xóa
                    var answers = await _context.StudentAnswers.Where(a => a.StudentExamId == existingExam.StudentExamId).ToListAsync();
                    answersToDelete.AddRange(answers);
                    examsToDelete.Add(existingExam);
                }
                // Xóa và thêm hàng loạt
                _context.StudentAnswers.RemoveRange(answersToDelete);
                _context.StudentExams.RemoveRange(examsToDelete);
                await _context.SaveChangesAsync();

                var msg = await _log.WriteActivity(new AddUserLogVM
                {
                    ActionType = "ReAssignExam",
                    UserId = usertoken,
                    Description = $"Reassigned exam {exam.Title} to {assignExam.StudentIds.Count} student(s).",
                    Metadata = string.Join(", ", assignExam.StudentIds),
                    ObjectId = exam.ExamId,
                    Status = (int)LogStatus.Success
                });
                if (msg.Length > 0) return (msg, null);

                await trans.CommitAsync();
                var msgCannnotReassign = cannotReassign.Count == 0 ? ""
                   : $"Reassign completed. However, {cannotReassign.Count} student(s) could not be reassigned because they have not joined the exam yet.";
                return ("", msgCannnotReassign);
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                return ($"Error during reassign: {ex.Message}", new List<string>());
            }
        }

        public async Task<string> ReAssignStudent(ReAssignStudent reAssign, string userToken)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var now = DateTime.UtcNow;
                var exam = await _unitOfWork.Exams.GetExamOnGoing(reAssign.ExamId);
                if (exam == null) return ("Exam not found or not ongoing.");

                var existingExam = await _unitOfWork.StudentExams.GetByExamAndStudent(reAssign.ExamId, reAssign.StudentId);
                if (existingExam == null) return ($"Student has not joined the exam yet and cannot be reassigned.");

                var answersToDelete = await _unitOfWork.StudentAnswers.GetByStudentExamId(existingExam.StudentExamId);
                if (!answersToDelete.IsObjectEmpty())
                    _unitOfWork.StudentAnswers.RemoveRangeAsync(answersToDelete);

                var faceCaptures = await _unitOfWork.FaceCaptures.GetByStudentExamId(existingExam.StudentExamId);
                if (!faceCaptures.IsObjectEmpty())
                {
                    foreach (var capture in faceCaptures)
                    {
                        if (!capture.ImageUrl.IsEmpty())
                        {
                            var s3Key = Common.ExtractKeyFromUrl(capture.ImageUrl);
                            if (!s3Key.IsEmpty())
                                await _s3Service.DeleteFileAsync(s3Key);
                        }
                    }
                    _unitOfWork.FaceCaptures.DeleteRange(faceCaptures);
                }
                _unitOfWork.StudentExams.Remove(existingExam);
                await _unitOfWork.SaveChangesAsync();

                await _subject.Notify(reAssign, exam!, userToken, existingExam.StudentExamId);
                await transaction.CommitAsync();
                return "";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return $"Error during reassign: {ex.Message}";
            }
        }
    }
}
