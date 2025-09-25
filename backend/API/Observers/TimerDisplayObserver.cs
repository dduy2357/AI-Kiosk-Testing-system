using API.Hubs;
using API.Models;
using API.Observers.Interface;
using API.ViewModels;
using Microsoft.AspNetCore.SignalR;

namespace API.Observers
{
    public class TimerDisplayObserver : IMonitoringObserver
    {
        private readonly IHubContext<ExamHub> _examHub;
        public TimerDisplayObserver(IHubContext<ExamHub> examHub) => _examHub = examHub;

        public async Task OnExtraTimeAdded(StudentExamExtraTime time, StudentExam studentExam, string userId)
        {
            var newSubmitTime = studentExam.StartTime?.AddMinutes(studentExam.Exam!.Duration + studentExam.ExtraTimeMinutes!.Value);
            if (newSubmitTime.HasValue)
            {
                await _examHub.Clients.Group(studentExam.StudentExamId)
                    .SendAsync(ExamHub.RECEIVE_EXTRA_TIME, new
                    {
                        StudentExamId = studentExam.StudentExamId,
                        NewSubmitTime = newSubmitTime,
                        ExtraMinutes = time.ExtraMinutes
                    });
            }
        }
        public async Task OnExamFinished(FinishStudentExam finish, StudentExam studentExam, string userId)
        {
            await _examHub.Clients.Group(studentExam.StudentExamId)
                .SendAsync(ExamHub.FINISH_STUDENT_EXAM, new
                {
                    ExamId = studentExam.ExamId,
                    StudentExamId = studentExam.StudentExamId,
                    Success = true
                });
        }
        public async Task OnStudentReAssigned(ReAssignStudent reAssign, Exam exam, string userId, string oldStudentExamId)
        {
            await _examHub.Clients.Group(oldStudentExamId)
                .SendAsync(ExamHub.RE_ASSIGN_EXAM, new
                {
                    StudentId = reAssign.StudentId,
                    OldStudentExamId = oldStudentExamId
                });
        }
    }
}
