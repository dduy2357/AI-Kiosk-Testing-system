using API.ViewModels;

namespace API.Services.Interfaces
{
    public interface IStudentExamService
    {
        public Task<(string, object?)> GetList(string usertoken);
        public Task<(string, object?)> AccessExam(StudentExamRequest otp, string usertoken, HttpContext context);
        public Task<string> SubmitExam(SubmitExamRequest request, string usertoken, HttpContext context);
        public Task<string> SaveAnswerTemporary(SubmitExamRequest input, string usertoken);
        public Task<(string, StudentExamDetailVM?)> GetHistoryExamDetail(string studentExamId, string usertoken);
        public Task<(string, SearchResult?)> GetHistoryExams(SearchStudentExamVM search, string usertoken);
        public Task<(string, List<StudentAnswerVM>?)> GetSavedAnswers(string examId, string usertoken);
        public Task<(string, ExamDetailVM?)> GetExamDetail(string examId);
        public Task<(string, object?)> GetEssayExam(string studentExamId, string examId, string usertoken);
        public Task<string> MarkEssay(MarkEssayRequest input, string usertoken);

    }
}
