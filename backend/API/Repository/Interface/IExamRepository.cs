using API.Models;

namespace API.Repository.Interface
{
    public interface IExamRepository
    {
        Task AddAsync(Exam exam);
        Task<Exam?> GetWithRoomById(string examId);
        Task<List<Exam>> GetAvailableExamsForStudent(string studentId, List<string> submittedExamIds, DateTime now);
        Task<Exam?> GetExamWithRoomUsers(string examId);
        Task<Exam?> GetById(string examId);
        Task<Exam?> GetExamWithQuestionAndRoom(string examId);
        Task<Exam?> GetExamOnGoing(string examId);
        Task<bool> TitleExistsAsync(string title, string userId);
    }
}
