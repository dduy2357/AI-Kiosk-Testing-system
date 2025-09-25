using API.Commons;
using API.Helper;
using API.Models;
using API.Services.Interfaces;
using API.Utilities;
using API.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class StudentViolationService : IStudentViolationService
    {
        private readonly Sep490Context _context;
        private readonly ILog _logger;
        private readonly IAmazonS3Service _s3Service;

        public StudentViolationService(Sep490Context context, ILog logger, IAmazonS3Service s3Service)
        {
            _context = context;
            _logger = logger;
            _s3Service = s3Service;
        }

        public async Task<(string, SearchResult?)> GetAll(SearchStudentViolation search)
        {
            if (search.ExamId.IsEmpty()) return ("ExamId is required.", null);
            var exam = await _context.Exams.AsNoTracking().FirstOrDefaultAsync(e => e.ExamId == search.ExamId);
            if (exam == null) return ("Exam not found.", null);

            var query = _context.StudentViolations.Include(sv => sv.StudentExam!.User)
                .Include(sv => sv.CreatedUser)
                .AsNoTracking()
                .AsQueryable();

            if (!search.StudentExamId.IsEmpty())
                query = query.Where(sv => sv.StudentExamId == search.StudentExamId);   // Lọc theo 1 student cụ thể nếu có
            else
                query = query.Where(sv => sv.StudentExam!.ExamId == search.ExamId);  // Lọc tất cả vi phạm của exam

            if (!search.TextSearch.IsEmpty())
            {
                var text = search.TextSearch!.ToLower();
                query = query.Where(sv => sv.ViolationName.ToLower() == text || sv.Message.ToLower() == text);
            }
            var totalCount = await query.CountAsync();
            var totalPage = (int)Math.Ceiling(totalCount / (double)search.PageSize);
            if (totalCount == 0) return ("No student violations found.", null);

            var violations = await query
                .OrderByDescending(sv => sv.CreatedAt)
                .Skip((search.CurrentPage - 1) * search.PageSize)
                .Take(search.PageSize)
                .Select(sv => new StudentViolationVM
                {
                    ViolationId = sv.Id,
                    StudentExamId = sv.StudentExamId,
                    Message = sv.Message,
                    ScreenshotPath = sv.ScreenshotPath,
                    CreatedAt = sv.CreatedAt,
                    ViolationName = sv.ViolationName,
                    StudentName = sv.StudentExam!.User!.FullName,
                    StudentCode = sv.StudentExam.User.UserCode,
                    StudentEmail = sv.StudentExam.User.Email,
                    CreatorName = sv.CreatedUser!.FullName,
                    CreatorEmail = sv.CreatedUser.Email,
                    CreatorCode = sv.CreatedUser.UserCode,
                }).ToListAsync();

            return ("", new SearchResult
            {
                Result = violations,
                TotalPage = totalPage,
                PageSize = search.PageSize,
                CurrentPage = search.CurrentPage,
                Total = totalCount
            });
        }

        public async Task<(string, StudentViolationVM?)> GetById(string id)
        {
            var violation = await _context.StudentViolations.AsNoTracking().Include(sv => sv.StudentExam!.User)
                .Include(x => x.CreatedUser)
                .FirstOrDefaultAsync(sv => sv.Id == id);
            if (violation == null) return ("No student violation found", null);

            return ("", new StudentViolationVM
            {
                ViolationId = violation.Id,
                CreatorCode = violation.CreatedUser?.UserCode ?? "",
                CreatorName = violation.CreatedUser?.FullName ?? "",
                CreatorEmail = violation.CreatedUser?.Email ?? "",
                StudentExamId = violation.StudentExamId,
                Message = violation.Message,
                ScreenshotPath = violation.ScreenshotPath,
                CreatedAt = violation.CreatedAt,
                ViolationName = violation.ViolationName,
                StudentName = violation.StudentExam.User!.FullName,
                StudentCode = violation.StudentExam.User.UserCode,
                StudentEmail = violation.StudentExam.User.Email
            });
        }

        public async Task<string> Create(SendStudentViolationVM send, string usertoken)
        {
            if (send == null) return "Student violation data is null";
            var studentExam = await _context.StudentExams.Include(se => se.User).Include(se => se.Exam).Include(x => x.User)
                .FirstOrDefaultAsync(se => se.StudentExamId == send.StudentExamId);
            if (studentExam == null) return "Student exam not found";

            var uploadedUrls = "";
            if (send.ScreenshotPath != null)
            {
                var newCapId = Guid.NewGuid().ToString();
                string key = $"{UrlS3.Violations}{studentExam.User.UserCode ?? studentExam.User.FullName}/{studentExam.Exam.Title.RemoveWhitespace()}/{send.ScreenshotPath.FileName.RemoveWhitespace()}";
                uploadedUrls = await _s3Service.UploadFileAsync(key, send.ScreenshotPath);
            }

            var studentViolation = new StudentViolation
            {
                Id = Guid.NewGuid().ToString(),
                StudentExamId = send.StudentExamId,
                Message = send.Message,
                ViolationName = send.ViolateName,
                ScreenshotPath = uploadedUrls,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = usertoken,
            };
            await _context.StudentViolations.AddAsync(studentViolation);
            await _context.SaveChangesAsync();

            var msgMail = "";
            if (send.IsSendMail)
            {
                if (string.IsNullOrEmpty(studentExam.User?.Email))
                    return "Student email not found.";

                msgMail = await EmailHandler.SendEmailAsync(studentExam.User.Email!, "Exam Violation Notification",
                   $@"
                   You have violated the exam rules. Violation Type: {send.ViolateName}.<br/>
                   Exam: {studentExam.Exam.Title} - {studentExam.Exam.StartTime:dd/MM/yyyy HH:mm:ss}<br/><br/>
                   Message: {send.Message}<br/>
                   Image: <br/><img src='{uploadedUrls}' alt='Violation Image' width='400' /><br/> 
                   Link: {uploadedUrls}<br/><br/>  
                   "
                );
            }
            var msg = await _logger.WriteActivity(new AddUserLogVM
            {
                UserId = usertoken,
                ObjectId = send.StudentExamId,
                ActionType = "Send Violation Notification",
                Description = msgMail.Length < 0 ? $"Violation notification sent to {studentExam.User.Email} for Exam: {studentExam.ExamId}." : $"Error when sent mail: {msgMail}",
                Metadata = $"Violation Type: {send.ViolateName}, Message: {send.Message}, Screenshot: {uploadedUrls}",
                Status = (int)LogStatus.Success
            });
            if (msg.Length > 0) return msg;
            return "";
        }

        public async Task<string> Delete(string id, string usertoken)
        {
            if (id.IsEmpty()) return "Violation ID is required.";
            var violation = await _context.StudentViolations.FindAsync(id);
            if (violation == null) return "Violation not found.";

            if (!string.IsNullOrEmpty(violation.ScreenshotPath))
            {
                string s3Key = Helper.Common.ExtractKeyFromUrl(violation.ScreenshotPath);
                if (!string.IsNullOrEmpty(s3Key))
                {
                    await _s3Service.DeleteFileAsync(s3Key);
                }
            }
            _context.StudentViolations.Remove(violation);
            await _context.SaveChangesAsync();

            var msg = await _logger.WriteActivity(new AddUserLogVM
            {
                UserId = usertoken,
                ObjectId = id,
                ActionType = "Delete Violation",
                Description = $"Deleted violation for StudentExamId: {violation.StudentExamId}",
                Metadata = $"Violation Name: {violation.ViolationName}, Message: {violation.Message}, Screenshot: {violation.ScreenshotPath ?? ""}",
                Status = (int)LogStatus.Success
            });
            if (msg.Length > 0) return msg;
            return "";
        }


    }
}
