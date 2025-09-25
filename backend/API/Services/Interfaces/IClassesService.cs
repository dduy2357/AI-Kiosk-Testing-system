using API.ViewModels;

namespace API.Services.Interfaces
{
    public interface IClassesService
    {
        public Task<(string, SearchResult?)> GetAllClasses(SearchClassVM search);
        public Task<(string, ClassVM?)> GetClassById(string classId);
        public Task<string> DoCreateUpdateClass(CreateUpdateClassVM input, string usertoken);
        public Task<string> DoDeactivateClass(string classId, string usertoken);
    }
}
