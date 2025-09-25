using API.Models;
using API.Observers.Interface;
using API.ViewModels;

namespace API.Subjects
{
    public class MonitoringSubject : IMonitoringSubject
    {
        private readonly List<IMonitoringObserver> _observers = new();

        public void Attach(IMonitoringObserver observer)
        {
            _observers.Add(observer);
        }

        public void Detach(IMonitoringObserver observer)
        {
            _observers.Remove(observer);
        }

        public async Task Notify(StudentExamExtraTime time, StudentExam studentExam, string userId)
        {
            foreach (var observer in _observers)
            {
                await observer.OnExtraTimeAdded(time, studentExam, userId);
            }
        }
        public async Task Notify(FinishStudentExam finish, StudentExam studentExam, string userId)
        {
            foreach (var observer in _observers)
            {
                await observer.OnExamFinished(finish, studentExam, userId);
            }
        }
        public async Task Notify(ReAssignStudent reAssign, Exam exam, string userId, string oldStudentExamId)
        {
            foreach (var observer in _observers)
            {
                await observer.OnStudentReAssigned(reAssign, exam, userId, oldStudentExamId);
            }
        }
    }
}
