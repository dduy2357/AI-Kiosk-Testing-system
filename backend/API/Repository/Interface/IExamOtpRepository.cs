using API.Models;

namespace API.Repository.Interface
{
    public interface IExamOtpRepository
    {
        Task<ExamOtp?> GetByExamId(string examId);
        Task AddAsync(ExamOtp otp);
        void Update(ExamOtp otp);
        Task<bool> IsOtpValid(string examId, int otpCode, DateTime now);
    }
}
