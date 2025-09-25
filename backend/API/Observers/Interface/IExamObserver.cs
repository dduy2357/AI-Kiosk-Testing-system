using API.Models;

namespace API.Observers.Interface
{
    public interface IExamObserver
    {
        Task OnExamAdded(Exam exam, string userId);
    }
}
