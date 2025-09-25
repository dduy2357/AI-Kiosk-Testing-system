using API.ViewModels;

namespace API.Services.Interfaces
{
    public interface IMonitoringService
    {
        public Task<(string, SearchResult?)> GetExamOverview(MonitorExamSearchVM search, string usertoken);
        public Task<(string, SearchResult?)> GetExamMonitorDetail(MonitorExamDetailSearchVM search, string usertoken);
        public Task<string> AddStudentExtraTime(StudentExamExtraTime time, string usertoken);
        public Task<string> AddExamExtraTime(ExamExtraTime time, string usertoken);
        public Task<string> FinishExam(FinishExam finish, string usertoken);
        public Task<string> FinishStudentExam(FinishStudentExam finish, string usertoken);
        public Task<(string, object?)> ReAssignExam(ReAssignExam assignExam, string usertoken);
        public Task<string> ReAssignStudent(ReAssignStudent assignStudent, string usertoken);

    }
}
