using API.Builders;
using API.Commons;
using API.Factory;
using API.Helper;
using API.Models;
using API.Repository;
using API.Services.Interfaces;
using API.Validators;
using API.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace API.Services
{
    public class StudentExamService : IStudentExamService
    {
        private readonly Sep490Context _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IScoringStrategyFactory _scoringStrategy;

        public StudentExamService(Sep490Context context, IUnitOfWork unitOfWork, IScoringStrategyFactory scoringStrategy)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _scoringStrategy = scoringStrategy;
        }

        public async Task<(string, object?)> GetList1(string usertoken)
        {
            var now = DateTime.UtcNow;
            var todayStart = DateTime.UtcNow.Date;
            var todayEnd = todayStart.AddDays(1).AddSeconds(-1);

            // Lấy danh sách examId đã thi xong hôm nay
            var submittedExamIdsToday = await _context.StudentExams.AsNoTracking()
                .Where(se => se.StudentId == usertoken && se.Status != (int)StudentExamStatus.NotStarted && se.Status != (int)StudentExamStatus.InProgress
                   && se.SubmitTime >= todayStart && se.SubmitTime <= todayEnd)
                .Select(se => se.ExamId).Distinct()
                .ToListAsync();

            var exams = await _context.Exams.AsNoTracking().Include(e => e.Room).ThenInclude(r => r.RoomUsers)
                .Where(e => e.Status == (int)ExamStatus.Published
                    && e.Room.RoomUsers.Any(ru => ru.UserId == usertoken && ru.Status == (int)ActiveStatus.Active)
                    &&
                    (
                        e.StartTime > now
                        || (e.StartTime <= now && e.EndTime >= now)
                        || submittedExamIdsToday.Contains(e.ExamId)
                    )
                ).OrderBy(e => e.StartTime)
                .ToListAsync();
            if (exams.Count == 0) return ("No exams found.", null);

            var examIds = exams.Select(e => e.ExamId).ToList();
            var studentExams = await _context.StudentExams.AsNoTracking()
                .Where(se => examIds.Contains(se.ExamId) && se.StudentId == usertoken)
                .ToListAsync();

            var studentExamDict = studentExams.ToDictionary(se => se.ExamId, se => se);
            var result = exams.Select(e =>
            {
                studentExamDict.TryGetValue(e.ExamId, out var studentExam);
                var status = studentExam == null
                    ? StudentExamStatus.NotStarted.ToString()
                    : studentExam.Status switch
                    {
                        (int)StudentExamStatus.InProgress => StudentExamStatus.InProgress.ToString(),
                        (int)StudentExamStatus.Failed => StudentExamStatus.Failed.ToString(),
                        (int)StudentExamStatus.Passed => StudentExamStatus.Passed.ToString(),
                        _ => StudentExamStatus.Submitted.ToString(),
                    };
                return new
                {
                    e.ExamId,
                    e.Title,
                    e.StartTime,
                    e.Duration,
                    e.EndTime,
                    e.LiveStatus,
                    e.ExamType,
                    Status = status
                };
            }).ToList();
            return ("", result);
        }

        public async Task<(string, object?)> GetList(string studentId)
        {
            var now = DateTime.UtcNow;
            var todayStart = DateTime.UtcNow.Date;
            var todayEnd = todayStart.AddDays(1).AddSeconds(-1);
            // Lấy danh sách examId đã thi xong hôm nay
            var submittedExamIdsToday = await _unitOfWork.StudentExams.GetSubmittedExamIds(studentId, todayStart, todayEnd);

            var exams = await _unitOfWork.Exams.GetAvailableExamsForStudent(studentId, submittedExamIdsToday, now);
            if (exams.Count == 0) return ("No exams found.", null);

            var examIds = exams.Select(e => e.ExamId).ToList();
            var studentExams = await _unitOfWork.StudentExams.GetByExamIds(studentId, examIds);
            var studentExamDict = studentExams.ToDictionary(se => se.ExamId, se => se);
            var result = exams.Select(e =>
            {
                studentExamDict.TryGetValue(e.ExamId, out var studentExam);
                var status = studentExam == null
                    ? StudentExamStatus.NotStarted.ToString()
                    : studentExam.Status switch
                    {
                        (int)StudentExamStatus.InProgress => StudentExamStatus.InProgress.ToString(),
                        (int)StudentExamStatus.Failed => StudentExamStatus.Failed.ToString(),
                        (int)StudentExamStatus.Passed => StudentExamStatus.Passed.ToString(),
                        _ => StudentExamStatus.Submitted.ToString(),
                    };
                return new
                {
                    e.ExamId,
                    e.Title,
                    e.StartTime,
                    e.Duration,
                    e.EndTime,
                    e.LiveStatus,
                    e.ExamType,
                    Status = status,
                    e.VerifyCamera
                };
            }).ToList();
            return ("", result);
        }

        public async Task<(string, object?)> AccessExam(StudentExamRequest otp, string userToken, HttpContext context)
        {
            var now = DateTime.UtcNow;
            var exam = await _unitOfWork.Exams.GetExamWithRoomUsers(otp.ExamId);
            if (exam == null) return ("Exam not found.", null);

            var msg = AccessExamValidator.Validate(exam, now, userToken);
            if (msg.Length > 0) return (msg, null);

            var validOtp = await _unitOfWork.ExamOTPs.IsOtpValid(otp.ExamId, otp.OtpCode, now);
            if (!validOtp) return ("Invalid or expired OTP.", null);

            var existingStudentExam = await _unitOfWork.StudentExams.GetByExamAndStudent(otp.ExamId, userToken);
            StudentExam studentExam;
            if (existingStudentExam != null)
            {
                if (Common.IsCompleted(existingStudentExam.Status))
                    return ("You have already submitted this exam and cannot access it again.", null);
                if (existingStudentExam.SubmitTime < now)
                    return ("You cannot resume the exam. Your duration time has ended.", null);

                // Resume in-progress
                existingStudentExam.Status = (int)StudentExamStatus.InProgress;
                existingStudentExam.IpAddress = Utils.GetClientIpAddress(context);
                existingStudentExam.BrowserInfo = Utils.GetClientBrowser(context);
                existingStudentExam.UpdatedAt = now;

                studentExam = existingStudentExam;
                _unitOfWork.StudentExams.Update(studentExam);
            }
            else
            {
                studentExam = new StudentExamBuilder()
                    .WithExam(exam)
                    .WithStudent(userToken)
                    .WithHttpContext(context)
                    .AsNew()
                    .Build();
                await _unitOfWork.StudentExams.Add(studentExam);
            }

            await _unitOfWork.SaveChangesAsync();
            return ("", new
            {
                StudentExamId = studentExam.StudentExamId,
                ExamId = otp.ExamId,
                StartTime = studentExam.StartTime,
            });
        }

        public async Task<(string, ExamDetailVM?)> GetExamDetail(string examId)
        {
            var exam = await _unitOfWork.Exams.GetById(examId);
            if (exam == null) return ("Exam not found.", null);

            var isEssay = exam.ExamType == (int)QuestionType.Essay;
            var questions = _unitOfWork.ExamQuestions.GetQuestionsForExam(examId, exam.TotalQuestions, !isEssay);

            var questionVMs = await questions.Select(q => new ExamQuestionVM
            {
                QuestionId = q.Question!.QuestionId,
                Content = q.Question.Content ?? "",
                QuestionType = ((QuestionTypeChoose)q.Question.Type).ToString(),
                Point = q.Question.Point,
                Options = isEssay ? new List<string>() : JsonHandler.SafeDeserializeOptions(q.Question.Options),
                DifficultLevel = q.Question.DifficultLevel
            }).ToListAsync();

            return ("", new ExamDetailVM
            {
                ExamId = exam.ExamId,
                Title = exam.Title ?? "",
                ExamType = exam.ExamType,
                Duration = exam.Duration,
                StartTime = exam.StartTime,
                EndTime = exam.EndTime,
                TotalQuestions = exam.TotalQuestions,
                Questions = questionVMs
            });
        }

        public async Task<string> SubmitExam1(SubmitExamRequest request, string usertoken, HttpContext context)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Kiểm tra bài thi của sinh viên
                var now = DateTime.UtcNow;
                var studentExam = await _context.StudentExams.Include(x => x.Exam)
                    .FirstOrDefaultAsync(se => se.ExamId == request.ExamId && se.StudentExamId == request.StudentExamId &&
                        se.StudentId == usertoken);
                if (studentExam == null) return "You have not started this exam or it has already been submitted.";
                if (studentExam.Exam!.EndTime < now) return "Exam has ended, you cannot submit answers.";
                if (Common.IsSubmittedOrFinished(studentExam.Status)) return "You have already submitted this exam.";

                // 2. Lưu các câu trả lời mới nhất
                var questionIds = request.Answers.Select(a => a.QuestionId).Where(q => q != null).ToList();
                var existingAnswers = await _context.StudentAnswers
                    .Where(sa => sa.StudentExamId == studentExam.StudentExamId && questionIds.Contains(sa.QuestionId))
                    .ToListAsync();

                var toUpdate = new List<StudentAnswer>();
                var toAdd = new List<StudentAnswer>();
                foreach (var input in request.Answers)
                {
                    if (input.QuestionId.IsEmpty()) continue;

                    var existing = existingAnswers.FirstOrDefault(a => a.QuestionId == input.QuestionId);
                    if (existing != null)
                    {
                        if (existing.UserAnswer == input.UserAnswer) continue;

                        existing.UserAnswer = input.UserAnswer;
                        existing.UpdatedAt = now;
                        toUpdate.Add(existing);
                    }
                    else
                    {
                        toAdd.Add(new StudentAnswer
                        {
                            StudentAnswerId = Guid.NewGuid().ToString(),
                            StudentExamId = studentExam.StudentExamId,
                            QuestionId = input.QuestionId!,
                            UserAnswer = input.UserAnswer,
                            CreatedAt = now,
                            UpdatedAt = now
                        });
                    }
                }
                if (toUpdate.Any()) _context.StudentAnswers.UpdateRange(toUpdate);
                if (toAdd.Any()) await _context.StudentAnswers.AddRangeAsync(toAdd);
                await _context.SaveChangesAsync();

                // 3. Chấm điểm nếu không phải essay
                decimal totalScore = 0;
                if (studentExam.Exam?.ExamType != null && studentExam.Exam?.ExamType != (int)QuestionTypeChoose.Essay)
                {
                    var allAnswers = await _context.StudentAnswers
                        .Where(sa => sa.StudentExamId == studentExam.StudentExamId)
                        .ToListAsync();

                    var examQuestions = await _context.ExamQuestions.Include(eq => eq.Question)
                        .Where(eq => eq.ExamId == studentExam.ExamId && allAnswers.Select(a => a.QuestionId).Contains(eq.QuestionId))
                        .ToDictionaryAsync(eq => eq.QuestionId, eq => eq);

                    foreach (var answer in allAnswers)
                    {
                        if (examQuestions.TryGetValue(answer.QuestionId, out var examQuestion))
                        {
                            var question = examQuestion.Question!;
                            var studentAns = answer.UserAnswer?.Trim().ToLowerInvariant();
                            var correctAns = question.CorrectAnswer?.Trim().ToLowerInvariant();

                            bool isCorrect = studentAns == correctAns;
                            answer.IsCorrect = isCorrect;
                            answer.PointsEarned = isCorrect ? examQuestion.Points : 0;
                            answer.UpdatedAt = now;

                            totalScore += answer.PointsEarned ?? 0;
                        }
                    }
                    _context.StudentAnswers.UpdateRange(allAnswers);

                    studentExam.Status = totalScore > 0 ? (int)StudentExamStatus.Passed : (int)StudentExamStatus.Failed;
                }
                // 4. Cập nhật bài thi
                else studentExam.Status = (int)StudentExamStatus.Submitted;
                studentExam.Score = totalScore;
                studentExam.SubmitTime = now;
                studentExam.UpdatedAt = now;
                studentExam.IpAddress = Utils.GetClientIpAddress(context);
                studentExam.BrowserInfo = Utils.GetClientBrowser(context);
                studentExam.TotalQuestions = request.Answers.Count(a => a.QuestionId != null);  

                _context.StudentExams.Update(studentExam);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync(); // ✅ commit nếu mọi thứ ổn
                return "";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return "An error occurred during submission: " + ex.Message;
            }
        }

        public async Task<string> SubmitExam(SubmitExamRequest request, string userToken, HttpContext context)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 1. Kiểm tra bài thi của sinh viên
                var now = DateTime.UtcNow;
                var studentExam = await _unitOfWork.StudentExams.GetByIdWithExam(request.StudentExamId, request.ExamId, userToken);

                var validationMessage = SubmitExamValidator.Validate(studentExam, now);
                if (validationMessage.Length > 0) return validationMessage;

                // 2. Lưu các câu trả lời mới nhất
                var questionIds = request.Answers.Select(a => a.QuestionId).Where(q => q != null).ToList();
                var existingAnswers = await _unitOfWork.StudentAnswers.GetByStudentExamAndQuestionIds(studentExam!.StudentExamId, questionIds);

                var toUpdate = new List<StudentAnswer>();
                var toAdd = new List<StudentAnswer>();

                foreach (var input in request.Answers)
                {
                    if (string.IsNullOrEmpty(input.QuestionId)) continue;
                    var existing = existingAnswers.FirstOrDefault(a => a.QuestionId == input.QuestionId);
                    if (existing != null)
                    {
                        if (existing.UserAnswer == input.UserAnswer) continue;

                        existing.UserAnswer = input.UserAnswer;
                        existing.UpdatedAt = now;
                        toUpdate.Add(existing);
                    }
                    else
                    {
                        var newAnswer = new StudentAnswerBuilder()
                            .WithStudentExamId(studentExam!.StudentExamId)
                            .WithQuestionId(input.QuestionId)
                            .WithUserAnswer(input.UserAnswer)
                            .AsNew()
                            .Build();
                        toAdd.Add(newAnswer);
                    }
                }
                if (toUpdate.Any()) _unitOfWork.StudentAnswers.UpdateRange(toUpdate);
                if (toAdd.Any()) await _unitOfWork.StudentAnswers.AddRangeAsync(toAdd);
                await _unitOfWork.SaveChangesAsync();

                // 3. Chấm điểm
                var allAnswers = await _unitOfWork.StudentAnswers.GetByStudentExamId(studentExam!.StudentExamId);
                var examQuestions = await _unitOfWork.ExamQuestions.GetByExamAndQuestionIds(studentExam.ExamId, allAnswers.Select(a => a.QuestionId).ToList());

                var strategy = _scoringStrategy.GetStrategy((QuestionTypeChoose)studentExam.Exam!.ExamType);
                var (totalScore, status) = await strategy.ScoreAnswers(allAnswers, examQuestions, now, _unitOfWork.StudentAnswers);

                // 4. Cập nhật bài thi
                studentExam.Score = totalScore;
                studentExam.SubmitTime = now;
                studentExam.UpdatedAt = now;
                studentExam.IpAddress = Utils.GetClientIpAddress(context);
                studentExam.BrowserInfo = Utils.GetClientBrowser(context);
                studentExam.TotalQuestions = request.Answers.Count(a => a.QuestionId != null);

                _unitOfWork.StudentExams.Update(studentExam);
                await _unitOfWork.SaveChangesAsync();

                await transaction.CommitAsync();
                return "";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return "An error occurred during submission: " + ex.Message;
            }
        }

        public async Task<string> SaveAnswerTemporary(SubmitExamRequest input, string userToken)
        {
            var now = DateTime.UtcNow;
            var studentExam = await _unitOfWork.StudentExams
                .GetByStudentExamId(input.StudentExamId, input.ExamId, userToken);
            if (studentExam == null) return "Student is not found in this exam.";

            var msg = AnswerValidator.Validate(studentExam, now);
            if (msg.Length > 0) return msg;

            var questionIds = input.Answers.Select(a => a.QuestionId).ToList();
            var existingAnswers = await _unitOfWork.StudentAnswers.GetByStudentExamAndQuestionIds(input.StudentExamId, questionIds);

            var toUpdate = new List<StudentAnswer>();
            var toAdd = new List<StudentAnswer>();

            foreach (var answer in input.Answers)
            {
                if (answer.QuestionId.IsEmpty()) continue;
                var existing = existingAnswers.FirstOrDefault(x => x.QuestionId == answer.QuestionId);
                if (existing != null)
                {
                    if (existing.UserAnswer == answer.UserAnswer) continue;
                    existing.UserAnswer = answer.UserAnswer;
                    existing.UpdatedAt = now;
                    toUpdate.Add(existing);
                }
                else
                {
                    var newAnswer = new StudentAnswerBuilder()
                        .WithStudentExamId(input.StudentExamId)
                        .WithQuestionId(answer.QuestionId)
                        .WithUserAnswer(answer.UserAnswer!)
                        .AsNew()
                        .Build();
                    toAdd.Add(newAnswer);
                }
            }

            if (toUpdate.Any()) _unitOfWork.StudentAnswers.UpdateRange(toUpdate);
            if (toAdd.Any()) await _unitOfWork.StudentAnswers.AddRangeAsync(toAdd);

            await _unitOfWork.SaveChangesAsync();
            return "";
        }

        public async Task<(string, SearchResult?)> GetHistoryExams(SearchStudentExamVM search, string usertoken)
        {
            var query = _context.StudentExams.Where(se => se.StudentId == usertoken && se.Status != (int)StudentExamStatus.InProgress && se.Status != (int)StudentExamStatus.NotStarted)
                .Include(se => se.Exam).AsNoTracking().AsQueryable();

            if (search.StartDate.HasValue)
                query = query.Where(se => se.CreatedAt >= search.StartDate.Value);

            if (search.EndDate.HasValue)
                query = query.Where(se => se.CreatedAt <= search.EndDate.Value);

            if (!search.TextSearch.IsEmpty())
                query = query.Where(se => se.Exam!.Title!.ToLower().Contains(search.TextSearch!.ToLower()));

            var totalCount = await query.CountAsync();
            var totalPage = (int)Math.Ceiling(totalCount / (double)search.PageSize);

            if (query.IsNullOrEmpty()) return ("No exams found for you.", null);
            var data = await query
                .OrderByDescending(se => se.SubmitTime)
                .Skip((search.CurrentPage - 1) * search.PageSize)
                .Take(search.PageSize)
                .Select(se => new StudentExamResultVM
                {
                    StudentExamId = se.StudentExamId,
                    ExamId = se.ExamId,
                    ExamTitle = se.Exam != null ? se.Exam.Title : "",
                    Score = se.Exam!.IsShowResult == true ? (se.Score ?? 0) : 0,
                    ExamDate = se.CreatedAt,
                    SubmitTime = se.SubmitTime,
                    DurationSpent = se.StartTime.HasValue && se.SubmitTime.HasValue
                        ? (int)(se.SubmitTime.Value - se.StartTime.Value).TotalSeconds
                        : 0,
                }).ToListAsync();

            return ("", new SearchResult
            {
                Result = data,
                TotalPage = totalPage,
                CurrentPage = search.CurrentPage,
                PageSize = search.PageSize,
                Total = totalCount
            });
        }

        public async Task<(string, StudentExamDetailVM?)> GetHistoryExamDetail(string studentExamId, string usertoken)
        {
            var studentExam = await _context.StudentExams.AsNoTracking().Include(se => se.Exam).Include(se => se.User)
                .FirstOrDefaultAsync(se => se.StudentExamId == studentExamId && se.StudentId == usertoken
                    && se.Status != (int)StudentExamStatus.InProgress && se.Status != (int)StudentExamStatus.NotStarted);
            if (studentExam == null) return ("Exam is not found.", null);

            var answers = await _context.StudentAnswers.Include(a => a.Question)
                .Where(sa => sa.StudentExamId == studentExam.StudentExamId)
                .ToListAsync();

            return ("", new StudentExamDetailVM
            {
                StudentName = studentExam.User?.FullName ?? "",
                StudentCode = studentExam.User?.UserCode ?? "",
                ExamTitle = studentExam.Exam?.Title ?? "",
                Score = studentExam.Score ?? 0,
                TotalQuestions = studentExam.TotalQuestions ?? 0,
                SubmitTime = studentExam.SubmitTime,
                StartTime = studentExam.StartTime,
                DurationSpent = studentExam.StartTime.HasValue && studentExam.SubmitTime.HasValue
                    ? (int)(studentExam.SubmitTime.Value - studentExam.StartTime.Value).TotalMinutes
                    : 0,
                TotalCorrectAnswers = answers.Count(a => a.IsCorrect == true),
                TotalWrongAnswers = answers.Count(a => a.IsCorrect == false),

                Answers = answers != null && studentExam.Exam?.IsShowResult == true
                ? answers.Select(a => new AnswerDetail
                {
                    QuestionId = a.QuestionId,
                    QuestionContent = a.Question?.Content ?? "",
                    CorrectAnswer = a.Question?.CorrectAnswer,
                    UserAnswer = a.UserAnswer,
                    IsCorrect = a.IsCorrect,
                    PointsEarned = a.PointsEarned ?? 0,
                    TimeSpent = a.TimeSpent,
                    Options = a.Question?.Options
                }).ToList() : null,
            });
        }

        public async Task<(string, List<StudentAnswerVM>?)> GetSavedAnswers(string examId, string usertoken)
        {
            var studentExam = await _unitOfWork.StudentExams.GetExamInProgress(examId, usertoken);
            if (studentExam == null)
                return ("You have not started this exam yet or it has already been submitted.", null);
            if (studentExam.Exam?.EndTime <= DateTime.UtcNow)
                return ("Exam has ended, you cannot retrieve answers.", null);

            var answers = await _context.StudentAnswers
                .Where(sa => sa.StudentExamId == studentExam.StudentExamId)
                .Select(sa => new StudentAnswerVM
                {
                    QuestionId = sa.QuestionId,
                    UserAnswer = sa.UserAnswer
                }).ToListAsync();

            return ("", answers);
        }

        public async Task<(string, object?)> GetEssayExam(string studentExamId, string examId, string usertoken)
        {
            var studentExam = await _context.StudentExams.AsNoTracking()
                .Include(x => x.User).Include(se => se.Exam.Room.Subject)
                .FirstOrDefaultAsync(se => se.StudentExamId == studentExamId && se.ExamId == examId
                         && se.Status != (int)StudentExamStatus.InProgress && se.Status != (int)StudentExamStatus.NotStarted);
            if (studentExam == null) return ("Student exam not found or not submitted.", null);
            if (studentExam.Exam?.ExamType != (int)QuestionTypeChoose.Essay) return ("This exam is not an Essay exam.", null);

            var isAdmin = await _context.Users.Include(u => u.UserRoles).AsNoTracking()
                 .AnyAsync(u => u.UserId == usertoken && u.UserRoles.Any(r => r.RoleId == (int)RoleEnum.Admin));

            var hasPermission = await _context.ExamSupervisors.AsNoTracking().AnyAsync(es => es.ExamId == examId
                     && (isAdmin || es.SupervisorId == usertoken || es.CreatedBy == usertoken));
            if (!hasPermission) return ("You do not have permission to mark this exam essay.", null);

            var examQuestions = await _context.ExamQuestions.Include(eq => eq.Question).Where(eq => eq.ExamId == examId).AsNoTracking().ToListAsync();
            var studentAnswers = await _context.StudentAnswers.Where(sa => sa.StudentExamId == studentExamId).AsNoTracking().ToListAsync();

            var result = examQuestions.Select(eq =>
            {
                var studentAns = studentAnswers.FirstOrDefault(a => a.QuestionId == eq.QuestionId);
                return new EssayAnswerVM
                {
                    QuestionId = eq.QuestionId,
                    QuestionContent = eq.Question?.Content,
                    CorrectAnswer = eq.Question?.CorrectAnswer,
                    UserAnswer = studentAns?.UserAnswer,
                    MaxPoints = eq.Points,
                    PointsEarned = studentAns?.PointsEarned ?? 0
                };
            }).ToList();

            var studentExamVM = new StudentEssayAnswerVM
            {
                StudentExamId = studentExam.StudentExamId,
                ExamId = studentExam.ExamId,
                ExamType = studentExam.Exam?.ExamType ?? 0,
                StudentExamStatus = studentExam.Status ?? (int)StudentExamStatus.NotStarted,
                ExamTitle = studentExam.Exam?.Title,
                SubjectName = studentExam.Exam?.Room.Subject?.SubjectName ?? "",
                RoomCode = studentExam.Exam?.Room?.RoomCode ?? "",
                ExamDate = studentExam.StartTime,
                SubmitTime = studentExam.SubmitTime,
                DurationSpent = studentExam.StartTime.HasValue && studentExam.SubmitTime.HasValue
                    ? (int)(studentExam.SubmitTime.Value - studentExam.StartTime.Value).TotalMinutes
                    : 0,
                Score = studentExam.Score,
                StudentName = studentExam.User?.FullName ?? "",
                StudentCode = studentExam.User?.UserCode ?? "",
                StudentAvatar = studentExam.User?.AvatarUrl ?? "",
                IsMarked = studentExam.Status == (int)StudentExamStatus.Failed || studentExam.Status == (int)StudentExamStatus.Passed,
                TotalQuestions = examQuestions.Count,
                Answers = result,
            };
            return ("", studentExamVM);
        }

        public async Task<string> MarkEssay(MarkEssayRequest input, string usertoken)
        {
            using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                var studentExam = await _context.StudentExams
                   .FirstOrDefaultAsync(se => se.StudentExamId == input.StudentExamId && se.ExamId == input.ExamId
                            && se.Status != (int)StudentExamStatus.InProgress && se.Status != (int)StudentExamStatus.NotStarted);
                if (studentExam == null) return "Student exam not found or not submitted.";

                //check permission
                var isAdmin = await _context.Users.Include(u => u.UserRoles).AsNoTracking()
                .AnyAsync(u => u.UserId == usertoken && u.UserRoles.Any(r => r.RoleId == (int)RoleEnum.Admin));

                var hasPermission = await _context.ExamSupervisors.AsNoTracking().AnyAsync(es => es.ExamId == input.ExamId
                         && (isAdmin || es.SupervisorId == usertoken || es.CreatedBy == usertoken));
                if (!hasPermission) return ("You do not have permission to mark this exam essay.");

                // Lấy examQuestions để biết điểm tối đa mỗi câu
                var examQuestions = await _context.ExamQuestions.Where(eq => eq.ExamId == input.ExamId)
                    .ToDictionaryAsync(eq => eq.QuestionId, eq => eq.Points);

                var questionIds = input.Scores.Select(s => s.QuestionId).ToList();
                if (questionIds.IsObjectEmpty()) return "Please grade the test before submitting.";

                var answers = await _context.StudentAnswers
                    .Where(sa => sa.StudentExamId == input.StudentExamId && questionIds.Contains(sa.QuestionId))
                    .ToListAsync();

                decimal totalScore = 0;
                foreach (var score in input.Scores)
                {
                    var answer = answers.FirstOrDefault(a => a.QuestionId == score.QuestionId);
                    if (answer == null) continue;

                    // Check max point không vượt giới hạn
                    var maxPoint = examQuestions.TryGetValue(score.QuestionId, out decimal value) ? value : 0;
                    var pointEarned = Math.Min(score.PointsEarned, maxPoint);

                    answer.PointsEarned = pointEarned;
                    answer.IsCorrect = null; // Tự luận ko chấm đúng sai tự động
                    answer.UpdatedAt = DateTime.UtcNow;

                    totalScore += pointEarned;
                }
                _context.StudentAnswers.UpdateRange(answers);

                studentExam.Score = totalScore;
                studentExam.UpdatedAt = DateTime.UtcNow;
                studentExam.Status = totalScore > 0 ? (int)StudentExamStatus.Passed : (int)StudentExamStatus.Failed;

                _context.StudentExams.Update(studentExam);
                await _context.SaveChangesAsync();

                //var msg = await _log.WriteActivity(new AddUserLogVM
                //{
                //    ActionType = "Mark essay",
                //    Description = 
                //})

                await trans.CommitAsync();
                return "";
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                return "An error occurred while marking essay exam: " + ex.Message;
            }
        }

    }
}
