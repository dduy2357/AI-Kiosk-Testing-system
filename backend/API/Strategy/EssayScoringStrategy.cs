using API.Helper;
using API.Models;
using API.Repository.Interface;
using API.Strategy.Interface;

namespace API.Strategy
{
    public class EssayScoringStrategy : IScoringStrategy
    {
        public async Task<(decimal TotalScore, int Status)> ScoreAnswers(
            List<StudentAnswer> answers,
            Dictionary<string, ExamQuestion> examQuestions,
            DateTime now,
            IStudentAnswerRepository context)
        {
            // Bài thi dạng essay không chấm điểm tự động
            return (0, (int)StudentExamStatus.Submitted);
        }
    }
}
