using API.Models;
using API.ViewModels;

namespace API.Services.Interfaces
{
    public interface IAuthenticateService
    {
        public Task<(string, LoginResult?)> LoginByGoogle(GoogleUserInfo input, string campusId);
        public Task<(string, LoginResult?)> DoLogin(UserLogin userLogin);
        public Task<string> DoRegister(UserRegister userRegister);
        public string DoLogout();
        public Task<string> DoForgetPassword(ForgetPassword input);
        public Task<string> DoResetPassword(ResetPassword input);
        public Task<string> DoChangePassword(ChangePassword input);
        public Task<(string message, User? user)> DoSearchByEmail(string? email);
    }
}
