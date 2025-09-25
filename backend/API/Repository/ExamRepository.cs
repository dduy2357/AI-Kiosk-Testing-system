using System.Text.RegularExpressions;
using API.Helper;
using API.Models;
using API.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace API.Repository
{
    public class ExamRepository : IExamRepository
    {
        private readonly Sep490Context _context;

        public ExamRepository(Sep490Context context)
        {
            _context = context;
        }
        public async Task<bool> TitleExistsAsync(string title, string userId)
        {
            //đại diện cho các ký tự khoảng trắng, + "quantifier" (bộ lặp) - > \s+ Một hoặc nhiều ký tự khoảng trắng
            var normalized = Regex.Replace(title.Trim().ToLower(), @"\s+", "");
            return await _context.Exams
                .AnyAsync(e => e.CreateUser == userId && e.Title.Trim().ToLower() == normalized);
        }

        public async Task AddAsync(Exam exam)
        {
            await _context.Exams.AddAsync(exam);
        }

        public async Task<Exam?> GetWithRoomById(string examId)
        {
            return await _context.Exams.Include(e => e.Room).FirstOrDefaultAsync(e => e.ExamId == examId);
        }

        public async Task<List<Exam>> GetAvailableExamsForStudent(string studentId, List<string> submittedExamIds, DateTime now)
        {
            return await _context.Exams.AsNoTracking()
                .Include(e => e.Room!.RoomUsers)
                .Where(e => e.Status == (int)ExamStatus.Published
                    && e.Room!.RoomUsers.Any(ru => ru.UserId == studentId && ru.Status == (int)ActiveStatus.Active)
                    &&
                    (
                        e.StartTime > now ||
                        (e.StartTime <= now && e.EndTime >= now) ||
                        submittedExamIds.Contains(e.ExamId)
                    ))
                .OrderBy(e => e.StartTime)
                .ToListAsync();
        }

        public async Task<Exam?> GetExamWithRoomUsers(string examId)
        {
            return await _context.Exams.AsNoTracking()
                .Include(e => e.Room!.RoomUsers)
                .FirstOrDefaultAsync(e => e.ExamId == examId);
        }

        public async Task<Exam?> GetExamOnGoing(string examId)
        {
            return await _context.Exams.FirstOrDefaultAsync(e => e.ExamId == examId && e.Status != (int)ExamStatus.Draft
                  && e.StartTime <= DateTime.UtcNow && e.EndTime >= DateTime.UtcNow);
        }

        public async Task<Exam?> GetById(string examId)
        {
            return await _context.Exams.AsNoTracking().FirstOrDefaultAsync(e => e.ExamId == examId);
        }

        public async Task<Exam?> GetExamWithQuestionAndRoom(string examId)
        {
            return await _context.Exams.AsNoTracking().Include(e => e.Room).Include(e => e.Creator)
                .Include(e => e.ExamQuestions).ThenInclude(eq => eq.Question).ThenInclude(qb => qb.QuestionBank)
                .FirstOrDefaultAsync(e => e.ExamId == examId);
        }
    }
}
