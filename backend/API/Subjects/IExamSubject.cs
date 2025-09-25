using API.Models;
using API.Observers.Interface;

namespace API.Subjects
{
    public interface IExamSubject
    {
        void Attach(IExamObserver observer);
        void Detach(IExamObserver observer);
        Task Notify(Exam exam, string userId);
    }
}
