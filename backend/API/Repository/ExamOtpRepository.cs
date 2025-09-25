using API.Migrations;
using API.Models;
using API.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace API.Repository
{
    public class ExamOtpRepository : IExamOtpRepository
    {
        private readonly Sep490Context _context;
        public ExamOtpRepository(Sep490Context context)
        {
            _context = context;
        }

        public async Task<ExamOtp?> GetByExamId(string examId)
        {
            return await _context.ExamOtps.FirstOrDefaultAsync(o => o.ExamId == examId);
        }

        public async Task AddAsync(ExamOtp otp)
        {
            await _context.ExamOtps.AddAsync(otp);
        }

        public void Update(ExamOtp otp)
        {
            _context.ExamOtps.Update(otp);
        }

        public async Task<bool> IsOtpValid(string examId, int otpCode, DateTime now)
        {
            return await _context.ExamOtps
                .AnyAsync(o => o.ExamId == examId && o.OtpCode == otpCode && o.ExpiredAt > now);
        }
    }
}
