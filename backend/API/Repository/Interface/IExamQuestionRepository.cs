using API.Models;

namespace API.Repository.Interface
{
    public interface IExamQuestionRepository
    {
        Task AddAsync(ExamQuestion examQuestion);
        IQueryable<ExamQuestion> GetQuestionsForExam(string examId, int total, bool randomize);
        Task<Dictionary<string, ExamQuestion>> GetByExamAndQuestionIds(string examId, List<string> questionIds);
    }
}
