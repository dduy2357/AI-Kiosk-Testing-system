using API.Models;

namespace API.Repository.Interface
{
    public interface IQuestionBankRepository
    {
        Task<QuestionBank?> GetQuestionBankByIdAsync(string questionBankId);
        Task<bool> IsQuestionBankIdExist(string questionBankId, bool IsTracking = false);
    }
}
