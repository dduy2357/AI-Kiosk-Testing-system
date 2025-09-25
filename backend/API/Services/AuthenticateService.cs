using API.Commons;
using API.Helper;
using API.Models;
using API.Services.Interfaces;
using API.Utilities;
using API.ViewModels;
using API.ViewModels.Token;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class AuthenticateService : IAuthenticateService
    {
        private readonly IMapper _mapper;
        private readonly Sep490Context _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthenticateService(IMapper mapper, Sep490Context context, IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> DoChangePassword(ChangePassword input)
        {
            var user = await _context.Users.FindAsync(input.UserId);
            if (user == null) return "User not found.";

            string msg = Converter.StringToMD5(input.ExPassword, out string? exPassMd5);
            if (msg.Length > 0) return $"Error encrypting: {msg}";

            if (!user.Password.Equals(exPassMd5)) return "Current password is not correct.";
            if (input.Password.Equals(input.ExPassword)) return "New password must be different from old password.";

            msg = Converter.StringToMD5(input.Password, out string newPasswordMd5);
            if (msg.Length > 0) return $"Error encrypting new password: {msg}";

            user.Password = newPasswordMd5;
            user.UpdateUser = user.UserId;
            user.UpdateAt = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return "";
        }

        public async Task<string> DoForgetPassword(ForgetPassword input)
        {
            var (msg, user) = await DoSearchByEmail(input.Email);
            if (msg.Length > 0) return msg;
            else if (user != null)
            {
                string newpass = "";
                (msg, newpass) = await EmailHandler.SendEmailAndPassword(input.Email, _httpContextAccessor.HttpContext);
                if (msg.Length > 0) return msg;

                msg = Converter.StringToMD5(newpass, out string mkMd5);
                if (msg.Length > 0) return $"Error convert to MD5: {msg}";

                user.Password = mkMd5;
                user.UpdateUser = user.UserId;
                user.UpdateAt = DateTime.UtcNow;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
            return "";
        }

        public async Task<(string, LoginResult?)> DoLogin(UserLogin userLogin)
        {
            string msg = "";
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == userLogin.Email);
            if (user is null) return ("Incorrect email or password. Please try again.", null);

            msg = Converter.StringToMD5(userLogin.Password, out string mkMd5);
            if (msg.Length > 0) return ($"Error convert to MD5: {msg}", null);
            if (user.Password.IsEmpty()) return ("Password is empty. Please contact support.", null);
            if (!user.Password.ToUpper().Equals(mkMd5.ToUpper())) return ("Incorrect email or password. Please try again", null);
            if (user.CampusId != userLogin.CampusId) return ("This account does not match the selected campus. Please verify and try again.", null);
            if (user.Status == (int)UserStatus.Inactive) return ($"User is deactivated. Please contact us to support.", null);

            user.Status = (int)UserStatus.Active;
            user.LastLogin = DateTime.UtcNow;
            user.LastLoginIp = Utils.GetClientIpAddress(_httpContextAccessor.HttpContext);
            await _context.SaveChangesAsync();

            var roleIds = await _context.UserRoles.Where(ur => ur.UserId == user.UserId).Select(ur => ur.RoleId).ToListAsync();
            var usertoken = _mapper.Map<UserToken>(user);
            usertoken.RoleId = roleIds;
            var accessToken = JwtAuthentication.GenerateJwtToken(usertoken, _httpContextAccessor.HttpContext);
            //var refreshToken = await _jwtAuthen.GenerateRefreshToken(user, _context, _httpContextAccessor.HttpContext);

            return ("", new LoginResult
            {
                AccessToken = accessToken,
                Data = usertoken
            });
        }

        public async Task<(string, LoginResult?)> LoginByGoogle(GoogleUserInfo input, string campusId)
        {
            if (string.IsNullOrEmpty(input.Email) || !input.Email.EndsWith("@fpt.edu.vn", StringComparison.OrdinalIgnoreCase))
                return ("Please use your @fpt.edu.vn account to log in.", null);

            var user = await _context.Users.FirstOrDefaultAsync(x => x.GoogleId == input.Id || x.Email == input.Email);
            if (user == null) return ("Your account is not authorized to access the system.", null);
            if (user.CampusId != campusId) return ("This account does not match the selected campus. Please verify and try again.", null);

            if (user.AvatarUrl.IsEmpty() && !input.Picture.IsEmpty())
                user.AvatarUrl = input.Picture;

            user.FullName = input.Name ?? user.FullName;
            user.LastLogin = DateTime.UtcNow;
            user.LastLoginIp = Utils.GetClientIpAddress(_httpContextAccessor.HttpContext);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            if (user.Status == (int)UserStatus.Inactive) return ("User is deactivated. Please contact us to support.", null);

            // Lấy toàn bộ Role đang gán cho user và kiểm tra role nào còn hoạt động
            var roles = await _context.UserRoles.Where(ur => ur.UserId == user.UserId).Include(ur => ur.Role).ToListAsync();
            var activeRoleIds = roles.Where(ur => ur.Role != null && ur.Role.IsActive).Select(ur => ur.RoleId).ToList();
            if (!activeRoleIds.Any()) return ("Your account does not have any active roles. Please contact admin.", null);

            var usertoken = _mapper.Map<UserToken>(user);
            usertoken.RoleId = activeRoleIds;

            var accessToken = JwtAuthentication.GenerateJwtToken(usertoken, _httpContextAccessor.HttpContext);
            return ("", new LoginResult
            {
                AccessToken = accessToken,
                Data = usertoken
            });
        }
        public string DoLogout()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            httpContext.Response.Cookies.Delete("JwtToken");
            httpContext.Session.Clear();
            return "";
        }

        public async Task<string> DoRegister(UserRegister input)
        {
            string msg = "";

            msg = _context.Users.CheckEmail(input.Email);
            if (msg.Length > 0) return msg;

            msg = Converter.StringToMD5(input.Password, out string mkMd5);
            if (msg.Length > 0) return $"Error convert to MD5: {msg}";

            var userId = Guid.NewGuid().ToString();
            var user = new User
            {
                UserId = userId,
                FullName = input.Fullname,
                Email = input.Email,
                Password = mkMd5,
                //RoleId = (int)Role.Student,
                Status = (int)UserStatus.Active,
                CreateAt = DateTime.UtcNow,
                UpdateAt = null,
                CreateUser = userId,
                AvatarUrl = "default-avatar.png",
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return "";
        }

        public async Task<string> DoResetPassword(ResetPassword input)
        {
            if (input.UserId == null) return "UserId is null";

            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == input.UserId);
            if (user == null) return "User not found.";

            var msg = Converter.StringToMD5(input.RePassword, out string mkMd5);
            if (msg.Length > 0) return $"Error convert to MD5: {msg}";

            user.Password = mkMd5;
            user.UpdateUser = user.UserId;
            user.UpdateAt = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return "";
        }
        public async Task<(string message, User? user)> DoSearchByEmail(string? email)
        {
            if (string.IsNullOrEmpty(email) || !email.IsValidEmailFormat())
                return ("Email is not valid", null);

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null) return ("Email is not exist.", null);

            return (string.Empty, user);
        }
    }
}
