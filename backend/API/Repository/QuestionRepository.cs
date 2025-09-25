using API.Models;
using API.Repository.Interface;

namespace API.Repository
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly Sep490Context _context;
        public QuestionRepository(Sep490Context context)
        {
            _context = context;
        }

        public async Task AddQuestionAsync(Question question)
        {
            await _context.Questions.AddAsync(question);
        }

    }
}
