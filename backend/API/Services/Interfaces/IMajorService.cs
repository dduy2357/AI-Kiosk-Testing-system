using API.ViewModels;

namespace API.Services.Interfaces
{
    public interface IMajorService
    {
        Task<(string, List<MajorVM>?)> GetAllMajorsAsync();
         
    }
}
