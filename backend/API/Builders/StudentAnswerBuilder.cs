using API.Models;

namespace API.Builders
{
    public class StudentAnswerBuilder
    {
        private readonly StudentAnswer _studentAnswer = new();

        public StudentAnswerBuilder WithStudentExamId(string studentExamId)
        {
            _studentAnswer.StudentExamId = studentExamId;
            return this;
        }

        public StudentAnswerBuilder WithQuestionId(string questionId)
        {
            _studentAnswer.QuestionId = questionId;
            return this;
        }

        public StudentAnswerBuilder WithUserAnswer(string? userAnswer)
        {
            _studentAnswer.UserAnswer = userAnswer;
            return this;
        }

        public StudentAnswerBuilder AsNew()
        {
            var now = DateTime.UtcNow;
            _studentAnswer.StudentAnswerId = Guid.NewGuid().ToString();
            _studentAnswer.IsCorrect = null;
            _studentAnswer.PointsEarned = 0;
            _studentAnswer.TimeSpent = 0;
            _studentAnswer.CreatedAt = now;
            _studentAnswer.UpdatedAt = now;
            return this;
        }

        public StudentAnswerBuilder AsUpdate(StudentAnswer existing)
        {
            _studentAnswer.StudentAnswerId = existing.StudentAnswerId;
            _studentAnswer.StudentExamId = existing.StudentExamId;
            _studentAnswer.QuestionId = existing.QuestionId;
            _studentAnswer.IsCorrect = existing.IsCorrect;
            _studentAnswer.PointsEarned = existing.PointsEarned;
            _studentAnswer.TimeSpent = existing.TimeSpent;
            _studentAnswer.CreatedAt = existing.CreatedAt;
            _studentAnswer.UpdatedAt = DateTime.UtcNow;
            return this;
        }

        public StudentAnswer Build() => _studentAnswer;
    }
}
