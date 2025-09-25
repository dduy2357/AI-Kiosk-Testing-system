using API.Models;
using API.Observers.Interface;
using API.ViewModels;

namespace API.Subjects
{
    public interface IMonitoringSubject
    {
        void Attach(IMonitoringObserver observer);
        void Detach(IMonitoringObserver observer);
        Task Notify(StudentExamExtraTime time, StudentExam studentExam, string userId);
        Task Notify(FinishStudentExam finish, StudentExam studentExam, string userId);
        Task Notify(ReAssignStudent reAssign, Exam exam, string userId, string oldStudentExamId);
    }
}
