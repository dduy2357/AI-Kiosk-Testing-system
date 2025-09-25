using API.ViewModels;

namespace API.Services.Interfaces
{
    public interface IDisabledKeyService
    {
        public Task<(string, SearchResult)> GetAll(DisabledKeySearchVM search);
        public Task<(string, DisabledKeyVM?)> GetOne(string keyId);
        public Task<(string, List<string>?)> ChangeActivate(List<string> keyIds, string usertoken);
        public Task<(string, List<string>?)> DoDelete(List<string> keyIds, string usertoken);
        public Task<string> CreateUpdate(CreateUpdateDisabledKeyVM input, string usertoken);
    }
}
