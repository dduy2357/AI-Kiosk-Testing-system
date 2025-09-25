using API.Models;

namespace API.Repository.Interface
{
    public interface ISubjectRepository
    {
        Task<bool> GetSubjectByIdAsync(string subjectId);
    }
}
