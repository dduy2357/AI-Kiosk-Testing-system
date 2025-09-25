using API.Models;
using API.ViewModels;

namespace API.Observers.Interface
{
    public interface IMonitoringObserver
    {
        Task OnExtraTimeAdded(StudentExamExtraTime time, StudentExam studentExam, string userId); 
        Task OnExamFinished(FinishStudentExam finish, StudentExam studentExam, string userId);
        Task OnStudentReAssigned(ReAssignStudent reAssign, Exam exam, string userId, string oldStudentExamId);

    }
}
