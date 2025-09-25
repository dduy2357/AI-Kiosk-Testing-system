using API.Models;
using API.ViewModels;

namespace API.Services.Interfaces
{
    public interface IUserService
    {
        public Task<string> DoToggleActive(string? usertoken, string? userId);
        public Task<(string, SearchResult?)> GetList(SearchUserVM search, string? usertoken);
        public Task<(string, UserListVM?)> GetById(string userID);
        public Task<string> Create(CreateUserVM input, string? usertoken);
        public Task<string> Update(UpdateUserVM input, string? userToken);
        public Task<string> CheckEmailExisted(string? email);
        public Task<string> CheckPhoneExisted(string? phone, string? userId = null);
        public Task<string> CheckUserCodeExisted(string? code, string? userId = null);
        public Task<(string, MemoryStream?)> ExportData(string? usertoken);
        public string CheckImportData(IFormFile fileData, out List<ErrorImport> result);
        public Task<(string, List<(AddListUserVM, string)>)> AddListUser(List<AddListUserVM> users, string? userToken);
        public Task<string> ChangeAvatar(ChangeAvatarVM avatarVM, string usertoken);

    }
}
