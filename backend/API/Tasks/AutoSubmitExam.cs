using API.Helper;
using API.Models;
using Microsoft.EntityFrameworkCore;


namespace API.Tasks
{
    public class AutoSubmitExam : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public AutoSubmitExam(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<Sep490Context>();

                using var transaction = await context.Database.BeginTransactionAsync(stoppingToken);
                try
                {
                    var now = DateTime.UtcNow;

                    var expiredExams = await context.StudentExams
                        .Include(se => se.Exam)
                        .Where(se => se.Status == (int)StudentExamStatus.InProgress &&
                                     se.SubmitTime < now)
                        .ToListAsync(stoppingToken);

                    foreach (var studentExam in expiredExams)
                    {
                        var allAnswers = await context.StudentAnswers
                            .Where(sa => sa.StudentExamId == studentExam.StudentExamId)
                            .ToListAsync(stoppingToken);

                        decimal totalScore = 0;

                        if (studentExam.Exam?.ExamType != null && studentExam.Exam.ExamType != (int)QuestionTypeChoose.Essay)
                        {
                            var examQuestions = await context.ExamQuestions
                                .Include(eq => eq.Question)
                                .Where(eq => eq.ExamId == studentExam.ExamId &&
                                             allAnswers.Select(a => a.QuestionId).Contains(eq.QuestionId))
                                .ToDictionaryAsync(eq => eq.QuestionId, eq => eq, cancellationToken: stoppingToken);

                            foreach (var answer in allAnswers)
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

                            context.StudentAnswers.UpdateRange(allAnswers);
                        }

                        // update bài thi
                        studentExam.Status = (int)StudentExamStatus.Submitted;
                        studentExam.Score = totalScore;
                        studentExam.SubmitTime = now;
                        studentExam.UpdatedAt = now;
                        studentExam.TotalQuestions = allAnswers.Count;

                        context.StudentExams.Update(studentExam);
                    }

                    if (expiredExams.Any())
                    {
                        await context.SaveChangesAsync(stoppingToken);
                        await transaction.CommitAsync(stoppingToken);
                    }
                    else
                    {
                        await transaction.RollbackAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(stoppingToken);
                    Console.WriteLine("AutoSubmitExpiredExams error: " + ex.Message);
                }

                // lặp lại sau 1 giây
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
    }
}
