using API.Models;

namespace API.Repository.Interface
{
    public interface IStudentAnswerRepository
    {
        Task<List<StudentAnswer>> GetByStudentExamAndQuestionIds(string studentExamId, List<string> questionIds, bool asNoTracking = false);
        Task<List<StudentAnswer>> GetByStudentExamId(string studentExamId, bool asNoTracking = false);

        void UpdateRange(List<StudentAnswer> answers);
        Task AddRangeAsync(List<StudentAnswer> answers);
        void RemoveRangeAsync(List<StudentAnswer> answers);
    }
}
