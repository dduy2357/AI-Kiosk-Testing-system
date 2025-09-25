using API.ViewModels;

namespace API.Services.Interfaces
{
    public interface IProhibitedAppService
    {
        public Task<(string, SearchResult?)> GetAll(ProhibitedAppSearchVM search);
        public Task<(string, ProhibitedAppVM?)> GetOne(string appId);
        public Task<string> CreateUpdate(CreateUpdateProhibitedAppVM input, string usertoken);
        public Task<(string, List<string>?)> ChangeActivate(List<string> appIds, string usertoken);
        public Task<(string, List<string>?)> DoRemove(List<string> appIds, string usertoken);
    }
}
