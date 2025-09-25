using API.Helper;
using API.Models;
using API.Services.Interfaces;
using API.Utilities;
using API.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace API.Commons
{
    public interface ILog
    {
        public Task<string> WriteActivity(AddUserLogVM log);
        public Task<string> WriteActivity(AddExamLogVM log);
        public Task<(string, SearchResult?)> GetListUserLog(UserLogFilterVM filter);
        public Task<(string, SearchResult?)> GetListExamLog(ExamLogFilterVM filter);
        public Task<(string, UserLogVM?)> GetLogUserById(string logUserId);
        public Task<(string, ExamLogVM?)> GetLogExamById(string logExamId);
        public Task<string> DeleteUserLog(List<string> logIds);
        public Task<string> DeleteExamLog(List<string> logIds);
        public Task<(string, MemoryStream?)> ExportLog(string usertoken, List<string> logIds); 
        public Task<(string, MemoryStream?)> ExportExamLog(string usertoken, string studentExamId);
    }

    public class Log : ILog
    {
        private readonly Sep490Context _context;
        private readonly IAmazonS3Service _s3Service;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public Log(Sep490Context context, IAmazonS3Service s3Service, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _s3Service = s3Service;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> WriteActivity(AddUserLogVM log)
        {
            if (log == null) return "";
            try
            {
                var userLog = new UserLog
                {
                    LogId = Guid.NewGuid().ToString(),
                    UserId = log.UserId,
                    ActionType = log.ActionType,
                    ObjectId = log.ObjectId,
                    IpAddress = Utils.GetClientIpAddress(_httpContextAccessor.HttpContext),
                    BrowserInfo = Utils.GetClientBrowser(_httpContextAccessor.HttpContext),
                    Description = log.Description,
                    Status = log.Status,
                    Metadata = log.Metadata,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.UserLogs.AddAsync(userLog);
                await _context.SaveChangesAsync();
                return "";
            }
            catch (Exception ex)
            {
                return $"Update data success, but error writing log: {ex.Message}";
            }
        }

        public async Task<string> WriteActivity(AddExamLogVM log)
        {
            if (log == null) return "";
            var studentExam = await _context.StudentExams.Include(x=> x.Exam).Include(x=> x.User).AsNoTracking()
                .FirstOrDefaultAsync(se => se.StudentExamId == log.StudentExamId && se.StudentId == log.UserId && se.Status == (int)StudentExamStatus.InProgress);
            if (studentExam == null) return "Student is not in a exam process.";
            try
            {
                var uploadedUrls = "";
                if (log.ScreenshotPath != null)
                {
                    string key = $"{UrlS3.Log}ExamTitle:_{studentExam.Exam.Title}/{studentExam.User.UserCode}/{log.ScreenshotPath.FileName}";
                    uploadedUrls = await _s3Service.UploadFileAsync(key, log.ScreenshotPath);
                }
                var userLog = new ExamLog
                {
                    ExamLogId = Guid.NewGuid().ToString(),
                    StudentExamId = log.StudentExamId,
                    UserId = log.UserId,
                    ActionType = log.ActionType,
                    Description = log.Description,
                    ScreenshotPath = uploadedUrls,
                    DeviceId = log.DeviceId,
                    DeviceUsername = log.DeviceUsername,
                    IpAddress = studentExam.IpAddress,
                    BrowserInfo = studentExam.BrowserInfo,
                    LogType = (int)log.LogType,
                    MetaData = log.Metadata,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.ExamLogs.AddAsync(userLog);
                await _context.SaveChangesAsync();
                return "";
            }
            catch (Exception ex)
            {
                return $"Update data success, but error writing log: {ex.Message}";
            }
        }

        public async Task<(string, UserLogVM?)> GetLogUserById(string logUserId)
        {
            if (logUserId.IsEmpty()) return ("Log ID cannot be null or empty", null);

            var log = await _context.UserLogs.Select(x => new UserLogVM
            {
                LogId = x.LogId,
                UserId = x.UserId,
                UserCode = x.User.UserCode ?? "",
                FullName = x.User.FullName,
                Email = x.User.Email!,
                LastLogin = x.User.LastLogin,
                ObjectId = x.ObjectId,
                Status = x.Status,
                ActionType = x.ActionType,
                Description = x.Description,
                IpAddress = x.IpAddress,
                BrowserInfo = x.BrowserInfo,
                Metadata = x.Metadata,
                CreatedAt = x.CreatedAt,
            }).FirstOrDefaultAsync(x => x.LogId == logUserId);
            if (log == null) return ("Log not found", null);
            return ("", log);
        }

        public async Task<(string, SearchResult?)> GetListUserLog(UserLogFilterVM filter)
        {
            var query = _context.UserLogs.Include(x => x.User).ThenInclude(x => x.UserRoles).AsNoTracking().AsQueryable();
            if (!filter.TextSearch.IsEmpty())
            {
                var keywords = filter.TextSearch!.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(x => x.ToLower()).ToList();
                if (keywords.Any())
                {
                    var param = Expression.Parameter(typeof(UserLog), "x");

                    Expression? finalExpr = null;
                    foreach (var kw in keywords)
                    {
                        var keywordExpr =
                            PredicateBuilder.ContainsIgnoreCase(param, "User.UserCode", kw)
                            .OrElse(PredicateBuilder.ContainsIgnoreCase(param, "User.FullName", kw))
                            .OrElse(PredicateBuilder.ContainsIgnoreCase(param, "Description", kw))
                            .OrElse(PredicateBuilder.ContainsIgnoreCase(param, "ActionType", kw));

                        finalExpr = finalExpr == null
                            ? keywordExpr
                            : Expression.AndAlso(finalExpr, keywordExpr);
                    }
                    var lambda = Expression.Lambda<Func<UserLog, bool>>(finalExpr!, param);
                    query = query.Where(lambda);
                }
            }
            if (!filter.UserId.IsEmpty())
                query = query.Where(x => x.UserId == filter.UserId);

            if (filter.RoleId.HasValue)
                query = query.Where(x => x.User.UserRoles.Any(ur => ur.RoleId == (int)filter.RoleId.Value));

            if (filter.LogStatus.HasValue)
                query = query.Where(x => x.Status == (int)filter.LogStatus.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(x => x.CreatedAt >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(x => x.CreatedAt <= filter.ToDate.Value);

            var totalCount = await query.CountAsync();
            var totalPage = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

            var logs = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((filter.CurrentPage - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(x => new UserLogListVM
                {
                    LogId = x.LogId,
                    FullName = x.User.FullName,
                    UserCode = x.User.UserCode ?? "",
                    ActionType = x.ActionType,
                    Description = x.Description,
                    CreatedAt = x.CreatedAt,
                }).ToListAsync();
            if (logs.IsObjectEmpty()) return ("No found any log", null);

            return ("", new SearchResult
            {
                Result = logs,
                TotalPage = totalPage,
                PageSize = filter.PageSize,
                CurrentPage = filter.CurrentPage,
                Total = totalCount
            });
        }
        public async Task<string> DeleteUserLog(List<string> logIds)
        {
            if (logIds == null || !logIds.Any()) return "No log ids provided.";

            var logs = await _context.UserLogs.Where(x => logIds.Contains(x.LogId)).ToListAsync();
            if (logs.IsObjectEmpty()) return "No logs found to delete.";

            _context.UserLogs.RemoveRange(logs);
            await _context.SaveChangesAsync();

            return "";
        }

        public async Task<(string, MemoryStream?)> ExportLog(string usertoken, List<string> logIds)
        {
            if (logIds == null || !logIds.Any()) return ("No log ids provided.", null);

            var list = await _context.UserLogs.Include(x => x.User)
                    .Where(x => logIds.Contains(x.LogId))
                    .Select(x => new UserLogVM
                    {
                        LogId = x.LogId,
                        UserId = x.UserId,
                        FullName = x.User.FullName,
                        UserCode = x.User.UserCode,
                        Email = x.User.Email,
                        ActionType = x.ActionType,
                        ObjectId = x.ObjectId,
                        IpAddress = x.IpAddress,
                        BrowserInfo = x.BrowserInfo,
                        Description = x.Description,
                        Status = x.Status,
                        Metadata = x.Metadata,
                        CreatedAt = x.CreatedAt,
                    }).ToListAsync();
            if (list.IsObjectEmpty()) return ("No logs found to export.", null);

            var file = FileHandler.GenerateExcelFile(list);
            if (file == null) return ("Error export data.", null);

            string msg = await WriteActivity(new AddUserLogVM
            {
                UserId = usertoken,
                ActionType = "Export log",
                Description = $"Log user data has been exported.",
                Status = (int)LogStatus.Success,
                Metadata = $"Exported {list.Count} user log"
            });
            if (msg.Length > 0) return (msg, null);
            return ("", file);
        }
        public async Task<(string, MemoryStream?)> ExportExamLog(string usertoken, string studentExamId)
        {
            var list = await _context.ExamLogs.Include(x => x.User)
                    .Where(x => x.StudentExamId == studentExamId)
                    .Select(x => new
                    {
                        LogId = x.ExamLogId,
                        UserId = x.UserId,
                        FullName = x.User.FullName,
                        UserCode = x.User.UserCode,
                        Email = x.User.Email,
                        ActionType = x.ActionType,
                        IpAddress = x.IpAddress,
                        BrowserInfo = x.BrowserInfo,
                        x.DeviceId,
                        x.DeviceUsername,
                        Description = x.Description,
                        x.ScreenshotPath,
                        Metadata = x.MetaData,
                        CreatedAt = x.CreatedAt,
                    }).ToListAsync();
            if (list.IsObjectEmpty()) return ("No logs found to export.", null);

            var file = FileHandler.GenerateExcelFile(list);
            if (file == null) return ("Error export data.", null);

            string msg = await WriteActivity(new AddUserLogVM
            {
                UserId = usertoken,
                ActionType = "Export",
                Description = $"Log exam data log has been exported.",
                Status = (int)LogStatus.Success,
                Metadata = $"Exported {list.Count} user exam data log"
            });
            if (msg.Length > 0) return (msg, null);
            return ("", file);
        }

        public async Task<(string, ExamLogVM?)> GetLogExamById(string logExamId)
        {
            if (logExamId.IsEmpty()) return ("Log Exam ID cannot be null or empty", null);

            var log = await _context.ExamLogs.Select(x => new ExamLogVM
            {
                ExamLogId = x.ExamLogId,
                UserId = x.UserId,
                StudentExamId = x.StudentExamId,
                UserCode = x.User.UserCode,
                FullName = x.User.FullName,
                ScreenshotPath = x.ScreenshotPath,
                LogType = x.LogType,
                DeviceId = x.DeviceId,
                DeviceUsername = x.DeviceUsername,
                Metadata = x.MetaData,
                ActionType = x.ActionType,
                Description = x.Description,
                IpAddress = x.IpAddress,
                BrowserInfo = x.BrowserInfo,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            }).FirstOrDefaultAsync(x => x.ExamLogId == logExamId);
            if (log == null) return ("Log not found", null);
            return ("", log);
        }

        public async Task<(string, SearchResult?)> GetListExamLog(ExamLogFilterVM filter)
        {
            var query = _context.ExamLogs.Include(x => x.User).ThenInclude(x => x.UserRoles)
              .Where(x => string.IsNullOrEmpty(filter.StudentExamId) || x.StudentExamId == filter.StudentExamId)
              .AsNoTracking().AsQueryable();

            if (!filter.TextSearch.IsEmpty())
            {
                var keywords = filter.TextSearch.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                     .Select(x => x.ToLower()).ToList();
                query = query.Where(x => keywords.All(kw =>
                    (x.Description != null && x.Description.ToLower().Contains(kw)) ||
                    (x.ActionType != null && x.ActionType.ToLower().Contains(kw)) ||
                    (x.MetaData != null && x.MetaData.ToLower().Contains(kw)) 
                ));
            }
            var totalCount = await query.CountAsync();
            var totalPage = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

            var logs = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((filter.CurrentPage - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(x => new ExamLogListVM
                {
                    ExamLogId = x.ExamLogId,
                    UserCode = x.User.UserCode,
                    FullName = x.User.FullName,
                    ActionType = x.ActionType,
                    Description = x.Description,
                    Metadata = x.MetaData,
                    LogType = x.LogType,
                    CreatedAt = x.CreatedAt,
                }).ToListAsync();
            if (totalCount == 0) return ("No found any log", null);

            return ("", new SearchResult
            {
                Result = logs,
                TotalPage = totalPage,
                PageSize = filter.PageSize,
                Total = totalCount,
                CurrentPage = filter.CurrentPage
            });
        }

        public async Task<string> DeleteExamLog(List<string> logIds)
        {
            var logs = await _context.ExamLogs.Where(x => logIds.Contains(x.ExamLogId)).ToListAsync();
            if (logs.IsObjectEmpty()) return "No logs found to delete.";

            foreach (var log in logs)
            {
                if (!string.IsNullOrEmpty(log.ScreenshotPath))
                {
                    string s3Key = Helper.Common.ExtractKeyFromUrl(log.ScreenshotPath);
                    if (!string.IsNullOrEmpty(s3Key))
                    {
                        await _s3Service.DeleteFileAsync(s3Key);
                    }
                }
            }
            _context.ExamLogs.RemoveRange(logs);
            await _context.SaveChangesAsync();

            return "";
        }

    }
}
