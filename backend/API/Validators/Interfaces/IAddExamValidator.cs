using API.Models;
using API.ViewModels;

namespace API.Validators.Interfaces
{
    public interface IAddExamValidator
    {
        Task<(bool IsValid, string ErrorMessage, List<Question>? SelectedQuestions)> ValidateAsync(AddExamRequest request, string userId);
    }
}
