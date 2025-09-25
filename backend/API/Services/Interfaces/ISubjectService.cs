using API.ViewModels;

namespace API.Services.Interfaces
{
    public interface ISubjectService
    {
        public Task<(string, SearchResult?)> GetAllSubjects(SearchSubjectVM search);
        public Task<(string, SubjectVM?)> GetSubjectById(string subjectId);
        public Task<string> ChangeActivateSubject(string subjectId, string usertoken);
        public Task<string> CreateUpdateSubject(CreateUpdateSubjectVM subject, string usertoken);
    }
}
