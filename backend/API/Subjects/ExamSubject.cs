using API.Models;
using API.Observers.Interface;

namespace API.Subjects
{
    public class ExamSubject : IExamSubject
    {
        private readonly List<IExamObserver> _observers = new();

        public void Attach(IExamObserver observer)
        {
            _observers.Add(observer);
        }

        public void Detach(IExamObserver observer)
        {
            _observers.Remove(observer);
        }

        public async Task Notify(Exam exam, string userId)
        {
            foreach (var observer in _observers)
            {
                await observer.OnExamAdded(exam, userId);
            }
        
        }
    }
}
