using API.Models;
using API.ViewModels;

namespace API.Services.Interfaces
{
    public interface ICampusService
    {
        public Task<(string, List<CampusVM>?)> GetAllCampusesAsync();
    }
}
