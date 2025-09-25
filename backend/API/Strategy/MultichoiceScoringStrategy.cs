using API.Helper;
using API.Models;
using API.Repository.Interface;
using API.Strategy.Interface;

namespace API.Strategy
{
    public class MultichoiceScoringStrategy : IScoringStrategy
    {
        public async Task<(decimal TotalScore, int Status)> ScoreAnswers(
            List<StudentAnswer> answers,
            Dictionary<string, ExamQuestion> examQuestions,
            DateTime now,
            IStudentAnswerRepository context)
        {
            decimal totalScore = 0;
            foreach (var answer in answers)
            {
                if (examQuestions.TryGetValue(answer.QuestionId, out var examQuestion))
                {
                    var question = examQuestion.Question!;
                    var studentAns = answer.UserAnswer?.Trim().ToLowerInvariant();
                    var correctAns = question.CorrectAnswer?.Trim().ToLowerInvariant();

                    bool isCorrect = studentAns == correctAns;
                    answer.IsCorrect = isCorrect;
                    answer.PointsEarned = isCorrect ? examQuestion.Points : 0;
                    answer.UpdatedAt = now;

                    totalScore += answer.PointsEarned ?? 0;
                }
            }
            context.UpdateRange(answers);
            int status = totalScore > 0 ? (int)StudentExamStatus.Passed : (int)StudentExamStatus.Failed;
            return (totalScore, status);
        }
    }

}
