using System.Runtime.CompilerServices;
using API.Models;

namespace API.Builders
{
    public class ExamQuestionBuilder
    {
        private readonly ExamQuestion _examQuestion;

        public ExamQuestionBuilder()
        {
            _examQuestion = new ExamQuestion
            {
                ExamQuestionId = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow
            };
        }

        public ExamQuestionBuilder SetExamId(string examId)
        {
            _examQuestion.ExamId = examId;
            return this;
        }

        public ExamQuestionBuilder SetQuestion(Question question, decimal scale)
        {
            _examQuestion.QuestionId = question.QuestionId;
            _examQuestion.Points = Math.Round(question.Point * scale, 2);
            return this;
        }

        public ExamQuestion Build() => _examQuestion;
    }
}
