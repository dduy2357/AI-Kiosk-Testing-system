using API.Models;
using API.ViewModels;

namespace API.Services.Interfaces
{
    public interface IConfigUrlService
    {
        public Task<(string, ConfigUrlVM?)> GetOne(string id);
        public Task<(string, List<ConfigUrlVM>?)> GetAll();
        public Task<string> ToggleConfigUrl(string id, string? usertoken);
        public Task<(string, object?)> CreateUpdate(CreateUpdateConfigUrlVM model, string? usertoken);
        public Task<(string, List<string>?)> DoRemove(List<string> urls, string? usertoken);
    }
}
