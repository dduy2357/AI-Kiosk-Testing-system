using API.Commons;
using API.Helper;
using API.Models;
using API.Observers.Interface;
using API.ViewModels;

namespace API.Observers
{
    public class LoggerObserver : IMonitoringObserver
    {
        private readonly ILog _log;

        public LoggerObserver(ILog log)
        {
            _log = log;
        }

        public async Task OnExtraTimeAdded(StudentExamExtraTime time, StudentExam studentExam, string userId)
        {
            var log = new AddUserLogVM
            {
                ActionType = "AddExtraTime",
                UserId = userId,
                Description = $"Added {time.ExtraMinutes} minutes of extra time to student exam {studentExam.User?.UserCode} for exam {studentExam.Exam!.Title}.",
                Metadata = studentExam.StudentId.ToString() + " \n " + time.ExtraMinutes + "minutes",
                ObjectId = studentExam.StudentExamId,
                Status = (int)LogStatus.Success
            };
            await _log.WriteActivity(log);
        }
        public async Task OnExamFinished(FinishStudentExam finish, StudentExam studentExam, string userId)
        {
            var log = new AddUserLogVM
            {
                ActionType = "FinishStudentExam",
                UserId = userId,
                Description = $"Student {(studentExam.User?.UserCode ?? "Unknown")} finished Exam: {(studentExam.Exam?.Title ?? "Unknown")}.",
                Metadata = studentExam.StudentId.ToString(),
                ObjectId = studentExam.StudentExamId,
                Status = (int)LogStatus.Success
            };
            await _log.WriteActivity(log);
        }
        public async Task OnStudentReAssigned(ReAssignStudent reAssign, Exam exam, string userId, string oldStudentExamId)
        {
            var log = new AddUserLogVM
            {
                ActionType = "ReAssignStudent",
                UserId = userId,
                Description = $"Reassigned exam {(exam.Title ?? "Unknown")} to student {reAssign.StudentId}.",
                Metadata = reAssign.StudentId,
                ObjectId = exam.ExamId,
                Status = (int)LogStatus.Success
            };
            await _log.WriteActivity(log);
        }
    }
}
