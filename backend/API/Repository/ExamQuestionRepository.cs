using API.Models;
using API.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace API.Repository
{
    public class ExamQuestionRepository : IExamQuestionRepository
    {
        private readonly Sep490Context _context;

        public ExamQuestionRepository(Sep490Context context)
        {
            _context = context;
        }

        public async Task AddAsync(ExamQuestion examQuestion)
        {
            await _context.ExamQuestions.AddAsync(examQuestion);
        }

        public IQueryable<ExamQuestion> GetQuestionsForExam(string examId, int total, bool randomize)
        {
            var query = _context.ExamQuestions
                .Include(eq => eq.Question)
                .Where(eq => eq.ExamId == examId)
                .AsNoTracking();

            if (randomize) query = query.OrderBy(_ => Guid.NewGuid());
            return query.Take(total);
        }
        public async Task<Dictionary<string, ExamQuestion>> GetByExamAndQuestionIds(string examId, List<string> questionIds)
        {
            return await _context.Set<ExamQuestion>()
                .Include(eq => eq.Question)
                .Where(eq => eq.ExamId == examId && questionIds.Contains(eq.QuestionId))
                .ToDictionaryAsync(eq => eq.QuestionId, eq => eq);
        }

    }
}
