using API.ViewModels;

namespace API.Services.Interfaces
{
    public interface IFeedbackService
    {
        public Task<(string, SearchResult?)> GetList(FeedbackSearchVM search);
        public Task<(string, object?)> GetOne(string id);
        public Task<string> CreateUpdate(CreateUpdateFeedbackVM model, string usertoken);
        //Task<string> Resolve(string id, string responseContent, string usertoken);
        public Task<string> Delete(string id);
    }
}
