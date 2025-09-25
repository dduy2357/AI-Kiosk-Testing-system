using API.Models;
using API.Repository.Interface;

namespace API.Strategy.Interface
{
    public interface IScoringStrategy
    {
        Task<(decimal TotalScore, int Status)> ScoreAnswers(
            List<StudentAnswer> answers,
            Dictionary<string, ExamQuestion> examQuestions,
            DateTime now,
            IStudentAnswerRepository context);
    }
}
