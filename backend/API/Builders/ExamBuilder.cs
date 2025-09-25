using API.Models;
using API.ViewModels;

namespace API.Builders
{
    public class ExamBuilder
    {
        private readonly Exam _exam = new();

        public ExamBuilder WithBasicInfo(string examId, AddExamRequest request, string userId)
        {
            _exam.ExamId = examId;
            _exam.RoomId = request.RoomId;
            _exam.Title = request.Title.Trim();
            _exam.Description = request.Description;
            _exam.Duration = request.Duration;
            _exam.TotalPoints = 10;
            _exam.StartTime = request.StartTime;
            _exam.EndTime = request.EndTime;
            _exam.TotalQuestions = request.QuestionIds.Count;
            _exam.IsShowResult = request.IsShowResult;
            _exam.IsShowCorrectAnswer = request.IsShowCorrectAnswer;
            _exam.Status = request.Status;
            _exam.CreatedAt = DateTime.UtcNow;
            _exam.UpdatedAt = DateTime.UtcNow;
            _exam.CreateUser = userId;
            _exam.GuildeLines = request.GuideLines;
            _exam.ExamType = request.ExamType;
            return this;
        }

        public Exam Build() => _exam;
    }
}
