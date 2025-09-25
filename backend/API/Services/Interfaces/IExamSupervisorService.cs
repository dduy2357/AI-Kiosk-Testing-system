using API.ViewModels;

namespace API.Services.Interfaces
{
    public interface IExamSupervisorService
    {
        public Task<(string, object?)> GetAll(string examId, string usertoken);
        public Task<(string, SearchResult?)> GetExams(SearchRequestVM search, string usertoken);
        public Task<(string, object?)> AssignSupervisor(EditExamSupervisorVM input, string usertoken);
        public Task<(string, object?)> Remove(EditExamSupervisorVM edit, string usertoken);
        public Task<(string, SearchResult?)> GetSupervisors(SearchRequestVM search);

    }
}
