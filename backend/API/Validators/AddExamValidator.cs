using API.Helper;
using API.Models;
using API.Repository;
using API.Validators.Interfaces;
using API.ViewModels;

namespace API.Validators
{
    public class AddExamValidator : IAddExamValidator
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddExamValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public static async Task<(bool, string)> ValidateAsync(AddExamRequest request, IUnitOfWork unitOfWork)
        {
            if (request.EndTime <= request.StartTime)
                return (false, "EndTime must be later than StartTime.");

            if (!await unitOfWork.Rooms.ExistsAsync(request.RoomId))
                return (false, "Selected room does not exist.");

            if (request.QuestionIds?.Count < 2)
                return (false, "At least two questions are required.");

            if (request.QuestionIds.Distinct().Count() != request.QuestionIds.Count)
                return (false, "Duplicate QuestionIds are not allowed.");

            return (true, string.Empty);
        }

        public async Task<(bool IsValid, string ErrorMessage, List<Question>? SelectedQuestions)> ValidateAsync(AddExamRequest request, string userId)
        {
            // Check if user is allowed (assuming a UserRepository exists or method to validate role)
            var user = await _unitOfWork.Users.GetUserByIdAsync(userId);
            if (user == null || !user.UserRoles.Any(r => r.RoleId == (int)RoleEnum.Lecture || r.RoleId == (int)RoleEnum.Admin))
                return (false, "User does not have permission to add exam.", null);

            if (request.EndTime <= request.StartTime)
                return (false, "EndTime must be later than StartTime.", null);

            if (!await _unitOfWork.Rooms.ExistsAsync(request.RoomId))
                return (false, "The selected room does not exist.", null);

            if (await _unitOfWork.Exams.TitleExistsAsync(request.Title, userId))
                return (false, "An exam with the same title already exists.", null);

            var QuestionBank = await _unitOfWork.QuestionBanks.GetQuestionBankByIdAsync(request.QuestionBankId);
            if (QuestionBank == null)
                return (false, "Question bank not found.", null);

            if (QuestionBank.Questions?.Any() != true)
                return (false, "Question bank has no questions.", null);

            if (request.QuestionIds == null || request.QuestionIds.Any(id => string.IsNullOrWhiteSpace(id)))
                return (false, "Some Question are empty or invalid.", null);

            if (request.QuestionIds.Distinct().Count() != request.QuestionIds.Count)
                return (false, "Duplicate QuestionIds are not allowed.", null);

            var invalidIds = request.QuestionIds
                .Where(id => QuestionBank.Questions.All(q => q.QuestionId != id))
                .ToList();
            if (invalidIds.Any())
                return (false, "Some selected questions do not belong to the selected question bank.", null);

            var selectedQuestions = QuestionBank.Questions
                .Where(q => request.QuestionIds.Contains(q.QuestionId) && q.Status == 1)
                .ToList();

            if (selectedQuestions.Count != request.QuestionIds.Count)
                return (false, "Some selected questions are either inactive or do not exist.", null);

            var distinctTypes = selectedQuestions.Select(q => q.Type).Distinct().ToList();
            if (distinctTypes.Count > 1)
                return (false, "All selected questions must be of the same type.", null);

            if (distinctTypes.First() != request.ExamType)
                return (false, $"ExamType mismatch. Selected questions are of type {(QuestionTypeChoose)distinctTypes.First()}, but ExamType is {(QuestionTypeChoose)request.ExamType}.", null);

            var totalPoints = selectedQuestions.Sum(q => q.Point);
            if (totalPoints == 0)
                return (false, "Total point of selected questions must be greater than 0.", null);

            return (true, string.Empty, selectedQuestions);
        }
    }
}
