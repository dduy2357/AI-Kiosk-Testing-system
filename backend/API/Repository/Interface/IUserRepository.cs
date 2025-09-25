using API.Models;

namespace API.Repository.Interface
{
    public interface IUserRepository
    {
        Task<User?> GetUserByIdAsync(string userId);
    }
}
