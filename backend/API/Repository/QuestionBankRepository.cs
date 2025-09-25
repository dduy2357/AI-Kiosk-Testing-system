using API.Models;
using API.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace API.Repository
{
    public class QuestionBankRepository : IQuestionBankRepository
    {
        private readonly Sep490Context _context;

        public QuestionBankRepository(Sep490Context context)
        {
            _context = context;
        }

        public async Task<QuestionBank?> GetQuestionBankByIdAsync(string questionBankId)
        {
            return await _context.QuestionBanks
                .Include(qb => qb.Questions)
                .FirstOrDefaultAsync(qb => qb.QuestionBankId.ToLower() == questionBankId.ToLower());
        }

        public async Task<bool> IsQuestionBankIdExist(string questionBankId, bool IsTracking = false)
        {
            var query = _context.QuestionBanks.AsQueryable();
            if (!IsTracking) query = query.AsNoTracking();

            return await query.AnyAsync(qb => qb.QuestionBankId == questionBankId);
        }
    }
}
