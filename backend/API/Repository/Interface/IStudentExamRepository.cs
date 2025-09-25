using API.Helper;
using API.Models;

namespace API.Repository.Interface
{
    public interface IStudentExamRepository
    {
        Task<List<string>> GetSubmittedExamIds(string studentId, DateTime from, DateTime to);
        Task<List<StudentExam>> GetByExamIds(string studentId, List<string> examIds);
        Task<StudentExam?> GetByExamAndStudent(string examId, string studentId);
        Task<StudentExam?> GetByIdWithExam(string studentExamId, string examId, string studentId, bool asNoTracking = false);
        Task<StudentExam?> GetByStudentExamId(string studentExamId, string examId, string studentId);
        Task<StudentExam?> GetExamInProgress(string examId, string usertoken);
        Task<StudentExam?> GetStudentExamWithExamUser(string studentExamId, StudentExamStatus? status = null, bool asNoTracking = false);
        Task Add(StudentExam studentExam);
        void Update(StudentExam studentExam);
        void Detach(StudentExam studentExam);
        void Remove(StudentExam studentExam);
    }
}
