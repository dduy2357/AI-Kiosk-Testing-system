using API.Commons;
using API.Helper;
using API.Models;
using API.Observers.Interface;
using API.ViewModels;

namespace API.Observers
{
    public class ExamLogObserver : IExamObserver
    {
        private readonly ILog _log;

        public ExamLogObserver(ILog log)
        {
            _log = log;
        }

        public async Task OnExamAdded(Exam exam, string userId)
        {
            var log = new AddUserLogVM
            {
                UserId = userId,
                ActionType = "Create Exam",
                ObjectId = exam.ExamId,
                Description = $"Created exam '{exam.Title}' in room '{exam.RoomId}'",
                Status = (int)LogStatus.Success,
                Metadata = $"ExamType={exam.ExamType}, Duration={exam.Duration}, Questions={exam.TotalQuestions}"
            };

            await _log.WriteActivity(log);
        }
    }
}
