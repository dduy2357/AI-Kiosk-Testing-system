using API.ViewModels;

namespace API.Services.Interfaces
{
    public interface IStudentViolationService
    {
        public Task<(string, SearchResult?)> GetAll(SearchStudentViolation search);
        public Task<(string, StudentViolationVM?)> GetById(string id);
        public Task<string> Create(SendStudentViolationVM send, string usertoken);
        public Task<string> Delete(string id, string usertoken);
    }
}
