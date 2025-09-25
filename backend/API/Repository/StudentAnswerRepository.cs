using API.Models;
using API.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace API.Repository
{
    public class StudentAnswerRepository : IStudentAnswerRepository
    {
        private readonly Sep490Context _context;

        public StudentAnswerRepository(Sep490Context context)
        {
            _context = context;
        }

        public async Task<List<StudentAnswer>> GetByStudentExamAndQuestionIds(string studentExamId, List<string> questionIds, bool asNoTracking = false)
        {
            var query = _context.StudentAnswers.Where(sa => sa.StudentExamId == studentExamId && questionIds.Contains(sa.QuestionId));
            if (asNoTracking) query = query.AsNoTracking();

            return await query.ToListAsync();
        }

        public async Task<List<StudentAnswer>> GetByStudentExamId(string studentExamId, bool asNoTracking = false)
        {
            var query = _context.StudentAnswers.Where(sa => sa.StudentExamId == studentExamId);
            if (asNoTracking) query = query.AsNoTracking();

            return await query.ToListAsync();
        }

        public void UpdateRange(List<StudentAnswer> answers)
        {
            _context.StudentAnswers.UpdateRange(answers);
        }

        public async Task AddRangeAsync(List<StudentAnswer> answers)
        {
            await _context.StudentAnswers.AddRangeAsync(answers);
        }

        public void RemoveRangeAsync(List<StudentAnswer> answers)
        {
            _context.StudentAnswers.RemoveRange(answers);
        }
   
    }
}
