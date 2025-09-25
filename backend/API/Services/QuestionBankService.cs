using API.Commons;
using API.Helper;
using API.Models;
using API.Services.Interfaces;
using API.Utilities;
using API.ViewModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace API.Services
{
    public class QuestionBankService : IQuestionBankService
    {
        private readonly Sep490Context _context;
        private readonly ILogger<QuestionBankService> _logger;

        public QuestionBankService(Sep490Context context, ILogger<QuestionBankService> logger)
        {
            _context = context;
            _logger = logger;
        }

        //Add
        public async Task<(bool Success, string Message)> AddQuestionBankAsync(AddQuestionBankRequest request, string userId)
        {
            var userExist = await _context.Users
           .Include(u => u.UserRoles)
               .AnyAsync(u => u.UserId == userId && u.UserRoles.Any(r =>
                   r.RoleId == (int)RoleEnum.Lecture || r.RoleId == (int)RoleEnum.Admin));
            if (!userExist)
                return (false, "You do not have permission to perform this action.");

            if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.SubjectId))
                return (false, "Missing required fields: Title or SubjectId"); 

            var subjectExists = await _context.Subjects.AnyAsync(s => s.SubjectId == request.SubjectId);
            if (!subjectExists)
                return (false, "Subject not found. Please select a valid subject."); 

            var newQb = new QuestionBank
            {
                QuestionBankId = Guid.NewGuid().ToString(),
                Title = request.Title.Trim(),
                SubjectId = request.SubjectId,
                Description = request.Description,
                CreateUserId = userId,
                Status = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.QuestionBanks.Add(newQb);
            await _context.SaveChangesAsync();
            return (true, "Question bank has been created successfully."); // Created successfully

        }
        //Edit
        public async Task<(bool Success, string Message)> EditQuestionBankAsync(string questionBankId, string title, string subjectId, string? description, string userId)
        {
            var userExist = await _context.Users
                .Include(u => u.UserRoles)
                .AnyAsync(u => u.UserId == userId && u.UserRoles.Any(r =>
                    r.RoleId == (int)RoleEnum.Lecture || r.RoleId == (int)RoleEnum.Admin));
            if (!userExist)
                return (false, "You do not have permission to perform this action.");

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(subjectId))
                return (false, "Title and SubjectId are required.");

            var subjectExists = await _context.Subjects.AnyAsync(s => s.SubjectId == subjectId);
            if (!subjectExists)
                return (false, "Subject not found.");

            var questionBank = await _context.QuestionBanks.FirstOrDefaultAsync(q => q.QuestionBankId == questionBankId);
            if (questionBank == null)
                return (false, "Question bank not found.");

            if (questionBank.CreateUserId != userId)
            {
                // If not creator, check if it was shared with user with write access
                var share = await _context.QuestionShares
                    .FirstOrDefaultAsync(qs => qs.QuestionBankId == questionBankId && qs.SharedWithUserId == userId);

                if (share == null)
                    return (false, "You are not allowed to edit this question bank.");

                if (share.AccessMode != 1) 
                    return (false, "You only have view permission and cannot edit this question bank.");
            }

            questionBank.Title = title.Trim();
            questionBank.SubjectId = subjectId;
            questionBank.Description = description;
            questionBank.UpdatedAt = DateTime.UtcNow;

            _context.QuestionBanks.Update(questionBank);
            await _context.SaveChangesAsync();

            return (true, "Question bank updated successfully.");
        }
        //Deactive/Active
        public async Task<(bool Success, string Message)> ToggleQuestionBankStatusAsync(string questionBankId, string userId)
        {
            var userExist = await _context.Users
            .Include(u => u.UserRoles)
                .AnyAsync(u => u.UserId == userId && u.UserRoles.Any(r =>
                    r.RoleId == (int)RoleEnum.Lecture || r.RoleId == (int)RoleEnum.Admin));
            if (!userExist)
                return (false, "You do not have permission to perform this action."); // No permission or user not found

            var questionBank = await _context.QuestionBanks.FirstOrDefaultAsync(q => q.QuestionBankId == questionBankId);

            if (questionBank == null)
                return (false, "Question bank not found.");

            if (questionBank.CreateUserId != userId)
                return (false, "You are not the creator of this question bank, so you cannot Deactive/Active it.");

            // Toggle status
            questionBank.Status = questionBank.Status == 1 ? 0 : 1;
            questionBank.UpdatedAt = DateTime.UtcNow;

            _context.QuestionBanks.Update(questionBank);
            await _context.SaveChangesAsync();

            return (true, $"Question bank change to {(ActiveStatus)questionBank.Status} successfully.");
        }
        //View Question Bank List
        public async Task<(string, QuestionBankListResponse?)> GetList(QuestionBankFilterVM input, string userId)
        {
            var userExist = await _context.Users
            .Include(u => u.UserRoles)
                .AnyAsync(u => u.UserId == userId && u.UserRoles.Any(r =>
                    r.RoleId == (int)RoleEnum.Lecture || r.RoleId == (int)RoleEnum.Admin));
            if (!userExist)
                return ("You do not have permission to perform this action.", null); // No permission or user not found

            // Get a list of Question Bank IDs that have been shared with users
            var questionShareQuery = _context.QuestionShares.Include(qs => qs.SharedWithUser).AsQueryable();

            // Get list of User Id (creator) from shared Question Banks
            var sharedQbIds = await questionShareQuery.Where(qs => qs.SharedWithUserId == userId).Select(qs => qs.QuestionBankId).ToListAsync();
            var sharedCreatorIds = await _context.QuestionBanks
                .Where(qb => sharedQbIds.Contains(qb.QuestionBankId))
                .Select(qb => qb.CreateUserId)
                .Distinct()
                .ToListAsync();

            var nameSharers = await _context.Users
                .Where(u => sharedCreatorIds.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId, u => u.FullName);
      
            var query = _context.QuestionBanks
                .Include(qb => qb.Subject)
                .Include(qb => qb.CreatedByUser)
                .AsQueryable();
            var questionBankIds = await query.Select(q => q.QuestionBankId).ToListAsync();

            var questionShares = await questionShareQuery.Where(qs => questionBankIds.Contains(qs.QuestionBankId)).ToListAsync();
            var sharedWithMap = questionShares
                .GroupBy(qs => qs.QuestionBankId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(qs => qs.SharedWithUser.Email).ToList()
                );

            if (!string.IsNullOrEmpty(input.TextSearch))
            {
                var keyword = input.TextSearch.ToLower();
                query = query.Where(qb => qb.Title.ToLower().Contains(keyword) ||
                     (qb.Subject != null && qb.Subject.SubjectName.ToLower().Contains(keyword)));
            }

            if (!string.IsNullOrEmpty(input.filterSubject))
            {
                var subjectKeyword = input.filterSubject.ToLower();
                query = query.Where(qb => qb.Subject != null && qb.Subject.SubjectName.ToLower().Contains(subjectKeyword));
            }

            if (input.Status.HasValue)
                query = query.Where(qb => qb.Status == (int)input.Status.Value);

            if (input.IsMyQuestion.HasValue && input.IsMyQuestion.Value)
            {
                query = query.Where(qb => qb.CreateUserId == userId || sharedQbIds.Contains(qb.QuestionBankId));
            }
            //else
            //{
            //    query = query.Where(qb => qb.CreateUserId == userId || sharedQbIds.Contains(qb.QuestionBankId));
            //}

            var totalCount = await query.CountAsync();
            var totalPage = (int)Math.Ceiling(totalCount / (double)input.PageSize);
            var ids = query.Select(qb => qb.QuestionBankId).ToList();
            var counts = await _context.Questions
                .Where(q => ids.Contains(q.QuestionBankId))
                .GroupBy(q => q.QuestionBankId)
                .Select(g => new
                {
                    QuestionBankId = g.Key,
                    Total = g.Count(),
                    MultipleChoice = g.Count(q => q.Type == (int)QuestionTypeChoose.MultipleChoice),
                    Essay = g.Count(q => q.Type == (int)QuestionTypeChoose.Essay)
                    //TrueFalse = g.Count(q => q.Type == (int)QuestionTypeChoose.TrueFalse),
                    //FillInTheBlank = g.Count(q => q.Type == (int)QuestionTypeChoose.FillInTheBlank),
                    //ShortAnswer = g.Count(q => q.Type == (int)QuestionTypeChoose.ShortAnswer),
                    //Matching = g.Count(q => q.Type == (int)QuestionTypeChoose.Matching)
                }).ToListAsync();
            var countMap = counts.ToDictionary(x => x.QuestionBankId);
            int totalQuestionsQB = counts.Sum(c => c.Total);
            int totalSubjects = await _context.Subjects.Select(q => q.SubjectId).Distinct().CountAsync();
            int totalSharedQB = await query.CountAsync(q => q.CreateUserId != userId);
            int totalQuestionBanks = ids.Count;

            var data = await query
                .OrderByDescending(qb => qb.CreatedAt)
                .Skip((input.CurrentPage - 1) * input.PageSize)
                .Take(input.PageSize)
                .Select(qb => new QuestionBankListVM
                {
                    QuestionBankId = qb.QuestionBankId,
                    Title = qb.Title,
                    CreateBy = qb.CreatedByUser.UserId,
                    SubjectName = qb.Subject != null ? qb.Subject.SubjectName : "",
                    TotalQuestions = countMap.ContainsKey(qb.QuestionBankId) ? countMap[qb.QuestionBankId].Total : 0,
                    MultipleChoiceCount = countMap.ContainsKey(qb.QuestionBankId) ? countMap[qb.QuestionBankId].MultipleChoice : 0,
                    EssayCount = countMap.ContainsKey(qb.QuestionBankId) ? countMap[qb.QuestionBankId].Essay : 0,
                    //TrueFalseCount = countMap.ContainsKey(qb.QuestionBankId) ? countMap[qb.QuestionBankId].TrueFalse : 0,
                    //FillInTheBlank = countMap.ContainsKey(qb.QuestionBankId) ? countMap[qb.QuestionBankId].FillInTheBlank : 0,
                    Status = qb.Status,
                    SharedByName = sharedQbIds.Contains(qb.QuestionBankId) ? (nameSharers.ContainsKey(qb.CreateUserId) ? nameSharers[qb.CreateUserId] : "Unknown") : "",
                    SharedWithUsers = sharedWithMap.ContainsKey(qb.QuestionBankId) ? sharedWithMap[qb.QuestionBankId] : new List<string>()

                }).ToListAsync();

            if (data.IsObjectEmpty()) return ("No found any question bank", null);

            return ("", new QuestionBankListResponse
            {
                TotalQuestionBanks = totalQuestionBanks,
                TotalQuestionsQB = totalQuestionsQB,
                TotalSubjects = totalSubjects,
                TotalSharedQB = totalSharedQB,
                Result = data,
                TotalPage = totalPage,
                CurrentPage = input.CurrentPage,
                PageSize = input.PageSize

            });
        }
        //Delete Question Bank 
        public async Task<(bool Success, string Message)> DeleteQuestionBankAsync(string questionBankId, string subjectId, string userId)
        {
            var userExist = await _context.Users
            .Include(u => u.UserRoles)
                .AnyAsync(u => u.UserId == userId && u.UserRoles.Any(r =>
                    r.RoleId == (int)RoleEnum.Lecture || r.RoleId == (int)RoleEnum.Admin));
            if (!userExist)
                return (false, "You do not have permission to perform this action.");
            var questionBank = await _context.QuestionBanks
                .FirstOrDefaultAsync(qb => qb.QuestionBankId == questionBankId && qb.SubjectId == subjectId);
            if (questionBank == null)
                return (false, "Question bank not found or not match subjectId.");
            if (questionBank.CreateUserId != userId)
                return (false, "You are not the creator of this Question Bank and therefore cannot delete it.");
            // Check if there are any questions in the question bank
            var hasQuestions = await _context.Questions.AnyAsync(q => q.QuestionBankId == questionBankId);
            if (hasQuestions)
                return (false, "Cannot delete question bank with existing questions.");
            _context.QuestionBanks.Remove(questionBank);
            await _context.SaveChangesAsync();
            return (true, "Question bank deleted successfully.");
        }
        //View detail questionbank
        public async Task<(bool Success, string Message, QuestionBankDetailVM? Data)> GetQuestionBankDetailAsync(string questionBankId, string userId)
        {
            // Validate user permission
            var userExist = await _context.Users
            .Include(u => u.UserRoles)
                .AnyAsync(u => u.UserId == userId && u.UserRoles.Any(r =>
                    r.RoleId == (int)RoleEnum.Lecture || r.RoleId == (int)RoleEnum.Admin));

            if (!userExist)
                return (false, "You do not have permission to perform this action.", null);

            var questionBank = await _context.QuestionBanks
                .Include(qb => qb.Subject)
                .Include(qb => qb.CreatedByUser)
                .Include(qb => qb.Questions)
                    .ThenInclude(q => q.CreatedByUser)
                .FirstOrDefaultAsync(qb => qb.QuestionBankId == questionBankId);

            if (questionBank == null)
                return (false, "Question bank not found.", null);

            var totalQuestions = questionBank.Questions.Count;

            var detail = new QuestionBankDetailVM
            {
                QuestionBankId = questionBank.QuestionBankId,
                QuestionBankName = questionBank.Title,
                CreateBy = questionBank.CreatedByUser.FullName,
                Description = questionBank.Description,
                SubjectId = questionBank.SubjectId,
                SubjectName = questionBank.Subject.SubjectName,
                TotalQuestions = totalQuestions,
                MultipleChoiceCount = questionBank.Questions.Count(q => q.Type == (int)QuestionTypeChoose.MultipleChoice),
                EssayCount = questionBank.Questions.Count(q => q.Type == (int)QuestionTypeChoose.Essay),
                Status = questionBank.Status,
                Questions = questionBank.Questions.Select(q => new QuestionVM
                {
                    QuestionId = q.QuestionId,
                    SubjectId = q.SubjectId,
                    Content = q.Content,
                    Type = q.Type,
                    DifficultLevel = q.DifficultLevel,
                    Point = q.Point,
                    Options = JsonConvert.DeserializeObject<List<string>>(q.Options),
                    CorrectAnswer = q.CorrectAnswer,
                    Explanation = q.Explanation,
                    ObjectFile = q.ObjectFile,
                    Status = q.Status,
                    CreatorId = q.CreateUser
                }).ToList()
            };

            return (true, "Success", detail);
        }
        //share question bank
        public async Task<(bool Success, string Message)> ShareQuestionBankAsync(string questionBankId, string targetUserEmail, string sharerUserId, int accessMode)
        {
            var userQuery = _context.Users.Include(u => u.UserRoles);

            // Validate sharer permission & get sharer name
            var sharer = await userQuery.FirstOrDefaultAsync(u => u.UserId == sharerUserId);
            if (sharer == null || !sharer.UserRoles.Any(r => r.RoleId == (int)RoleEnum.Lecture || r.RoleId == (int)RoleEnum.Admin))
                return (false, "You do not have permission to perform this action.");

            // Check question bank
            var questionBank = await _context.QuestionBanks.FirstOrDefaultAsync(qb => qb.QuestionBankId == questionBankId);
            if (questionBank == null)
                return (false, "Question bank not found.");

            if (questionBank.CreateUserId != sharerUserId)
                return (false, "You do not have permission to share this question bank.");

            // Check target user & role
            var targetUser = await userQuery.FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == targetUserEmail.ToLower());
            if (targetUser == null)
                return (false, "The user you are trying to share with does not exist.");

            if (!targetUser.UserRoles.Any(r => r.RoleId == (int)RoleEnum.Lecture || r.RoleId == (int)RoleEnum.Admin))
                return (false, "The user you are trying to share with does not have permission to receive shared question banks.");

            // Check if already shared
            bool alreadyShared = await _context.QuestionShares.AnyAsync(qs =>
                qs.QuestionBankId == questionBankId && qs.SharedWithUserId == targetUser.UserId);

            if (alreadyShared)
                return (false, "This question bank has already been shared with this user.");

            // Save share
            var share = new QuestionShare
            {
                QuestionShareId = Guid.NewGuid().ToString(),
                QuestionBankId = questionBankId,
                SharedWithUserId = targetUser.UserId,
                AccessMode = accessMode
            };

            _context.QuestionShares.Add(share);
            await _context.SaveChangesAsync();

            await EmailHandler.SendSharedNotificationAsync(targetUserEmail, questionBank.Title, sharer.FullName);

            return (true, "Question bank shared successfully.");
        }
        //Change Access Mode
        public async Task<(bool Success, string Message)> ChangeAccessModeAsync(string questionBankId, string targetUserEmail, string sharerUserId, int newAccessMode)
        {
            if (newAccessMode != 0 && newAccessMode != 1)
                return (false, "Invalid access mode. Only 0 (ViewOnly) and 1 (CanEdit) are allowed.");

            var userHasPermission = await _context.Users
                .Include(u => u.UserRoles)
                .AnyAsync(u => u.UserId == sharerUserId && u.UserRoles.Any(r =>
                    r.RoleId == (int)RoleEnum.Lecture || r.RoleId == (int)RoleEnum.Admin));

            if (!userHasPermission)
                return (false, "You do not have permission to perform this action.");

            // Check if the question bank exists and if the sharer is the creator
            var questionBank = await _context.QuestionBanks.FirstOrDefaultAsync(qb => qb.QuestionBankId == questionBankId);
            if (questionBank == null)
                return (false, "Question bank not found.");

            if (questionBank.CreateUserId != sharerUserId)
                return (false, "You do not have permission to modify this share.");
            // Check target user & role
            var targetUser = await _context.Users.Include(u => u.UserRoles).FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == targetUserEmail.ToLower());
            if (targetUser == null)
                return (false, "The user you are trying to share with does not exist.");

            // Check if the share record exists
            var questionShare = await _context.QuestionShares
                .FirstOrDefaultAsync(qs => qs.QuestionBankId == questionBankId && qs.SharedWithUserId == targetUser.UserId);

            if (questionShare == null)
                return (false, "Share record not found.");

            questionShare.AccessMode = newAccessMode;

            _context.QuestionShares.Update(questionShare);
            await _context.SaveChangesAsync();

            return (true, "Access mode updated successfully.");
        }
        //Export question bank
        public async Task<(string, MemoryStream?)> ExportQuestionBankReportAsync(string questionBankId, string userId)
        {
            // Check if user is a lecturer or admin
            var hasPermission = await _context.Users
                .Include(u => u.UserRoles)
                .AnyAsync(u => u.UserId == userId && u.UserRoles.Any(r => r.RoleId == (int)RoleEnum.Lecture || r.RoleId == (int)RoleEnum.Admin));

            if (!hasPermission)
                return ("You do not have permission to export question bank report.", null);

            // Fetch QuestionBank with questions
            var questionBank = await _context.QuestionBanks
                .Include(qb => qb.Subject)
                .Include(qb => qb.Questions)
                .FirstOrDefaultAsync(qb => qb.QuestionBankId == questionBankId);

            if (questionBank == null)
                return ("Question bank not found.", null);

            if (questionBank.Questions == null || !questionBank.Questions.Any())
                return ("This question bank has no questions.", null);

            // Prepare data
            var exportList = questionBank.Questions.Select(q => new
            {
                Content = q.Content,
                Type = ((QuestionTypeChoose)q.Type).ToString(),
                Difficulty = (DifficultyLevel)q.DifficultLevel,
                Point = q.Point.ToString("0.00"),
                Options = string.Join(" | ", JsonConvert.DeserializeObject<List<string>>(q.Options ?? "[]")),
                CorrectAnswer = q.CorrectAnswer,
                Explanation = q.Explanation ?? "",
                Status = q.Status == 1 ? "Active" : "Inactive"
            }).ToList();

            var file = FileHandler.GenerateExcelFile(exportList);
            if (file == null)
                return ("Failed to export question bank report.", null);

            return ("", file);
        }


    }
}
