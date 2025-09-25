using API.Models;

namespace API.Repository.Interface
{
    public interface IExamSupervisorRepository
    {
        Task AddAsync(ExamSupervisor examSupervisor);
    }
}
