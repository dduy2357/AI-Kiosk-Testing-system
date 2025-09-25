using API.Helper;
using API.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace API.Services.Interfaces
{
    public interface IQuestionBankService
    {
        Task<(bool Success, string Message)> AddQuestionBankAsync(AddQuestionBankRequest request, string userTokenId);
        Task<(bool Success, string Message)> EditQuestionBankAsync(string questionBankId,string title,string subjectId,string? description,string userId);
        Task<(bool Success, string Message)> ToggleQuestionBankStatusAsync(string questionBankId, string userId);
        public Task<(string, QuestionBankListResponse?)> GetList(QuestionBankFilterVM input, string userTokenId);
        Task<(bool Success, string Message, QuestionBankDetailVM? Data)> GetQuestionBankDetailAsync(string questionBankId, string userId);
        Task<(bool Success, string Message)> DeleteQuestionBankAsync(string questionBankId, string subjectId, string userId);
        Task<(bool Success, string Message)> ShareQuestionBankAsync(string questionBankId, string targetUserEmail, string sharerUserId, int accessMode);
        Task<(string, MemoryStream?)> ExportQuestionBankReportAsync(string questionBankId, string userId);
        Task<(bool Success, string Message)> ChangeAccessModeAsync(string questionBankId, string targetUserEmail, string sharerUserId, int newAccessMode);
    }
}
