using API.ViewModels;

namespace API.Services.Interfaces
{
    public interface IQuestionService
    {
        Task<(bool Success, string Message)> AddQuestionAsync(AddQuestionRequest req, string userTokenId);
        Task<(bool Success, string Message)> ToggleStatusAsync(ToggleQuestionStatusRequest req, string userTokenId);
        Task<(bool Success, string Message)> EditQuestionAsync(EditQuestionRequest req, string usertokenId);
        Task<(bool Success, string Message)> ImportListQuestionAsync(List<AddQuestionRequest> listQs, string userTokenId);
        Task<(bool Success, string Message, QuestionListResponse)> GetListQuestionAsync(QuestionRequestVM request, string userTokenId);
        Task<(bool Success, string Message)> AddQuestionBPAsync(AddQuestionRequest req, string userId);
        Task<(bool Success, string Message)> DeleteQuestionAsync(string questionBankId, string questionId, string userTokenId);
        Task<(string, List<AddQuestionRequest>?)> FormatListQuestion(FormatQuestionList file);
    }
}
