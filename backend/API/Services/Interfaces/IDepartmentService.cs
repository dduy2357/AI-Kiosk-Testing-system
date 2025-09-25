using API.ViewModels;

namespace API.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<(string, List<DepartmentVM>?)> GetAllDepartments();
        ///                                    
        /// <summary>
        ///        /// Retrieves a specific department by its ID.
        ///               /// </summary>
        ///                      /// <param name="departmentId">The ID of the department to retrieve.</param>
        ///                             /// <returns>A tuple containing a message and the department view model.</returns>
        ///                                    Task<(string, DepartmentVM?)> GetDepartmentByIdAsync(string departmentId);
    }
}
