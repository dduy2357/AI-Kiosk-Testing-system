using API.Models;

namespace API.Repository.Interface
{
    public interface IQuestionRepository
    {
        Task AddQuestionAsync(Question question);
        //Task<Question?> GetQuestionByIdAsync(string questionId);
    }
}
