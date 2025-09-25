using API.ViewModels;

namespace API.Services.Interfaces
{
    public interface ISpecializationService
    {
        Task<(string, List<SpecializationVM>?)> GetAllSpecializationsAsync();                           
    }
}
