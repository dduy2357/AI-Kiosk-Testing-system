using API.ViewModels;

namespace API.Services.Interfaces
{
    public interface IPositionService
    {
        Task<(string, List<PositionVM>?)> GetPositionByDepartment(string departmentId);
    }
}
