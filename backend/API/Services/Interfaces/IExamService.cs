using API.Helper;
using API.ViewModels;

namespace API.Services.Interfaces
{
    public interface IExamService
    {
        Task<(bool Success, string Message, List<SelectedQuestionDto>? Questions)> AddExamAsync(AddExamRequest request, string userId);
        Task<(string, ExamOtpVM?)> AssignOTP(CreateExamOtpVM input, string usertoken);
        Task<(bool Success, string Message, SearchResult?)> GetExamListAsync(ExamListRequest request, string userId);
        Task<(bool Success, string Message, ExamResultVM? Data)> GetExamResultReportAsync(string examId, string userId);
        Task<(bool Success, string Message, ExamDetail? Exam)> GetExamDetailAsync(string examId, string userId);
        Task<(bool Success, string Message)> UpdateExamAsync(UpdateExamRequest request, string userId);
        Task<(bool Success, string Message, string? GuideLines)> GetExamGuideLinesAsync(string examId, string userId);
        Task<(string, MemoryStream?)> ExportStudentExamResultReport(string examId, string userId);
        Task<(bool Success, string Message, StudentExamDetailDto? Data)> GetStudentExamDetailAsync(string studentExamId, string userId);
        Task<(bool Success, string Message)> ChangeExamStatusAsync(string examId, int newStatus, string userId);
        //Task<string> Handle(AddExamRequest request, string userId);
        //Task<(bool Success, string Message, List<SelectedQuestionDto>? Questions)> AddExamBuilderPatternAsync(AddExamRequest request, string userId);
    }
}
