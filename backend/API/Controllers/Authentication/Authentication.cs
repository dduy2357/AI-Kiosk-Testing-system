using API.Utilities;
using API.ViewModels.Token;

namespace API.Controllers.Authentication
{
    public class Authentication : BaseController
    {
        public UserToken? UserToken { get; set; } = null;
        public ResultObject ResultCheckToken { get; set; }

        public Authentication(IHttpContextAccessor httpContextAccessor)
        {
            ResultCheckToken = new ResultObject();

            var token = GetJwtTokenFromHeaderOrCookie(httpContextAccessor.HttpContext);
            if (string.IsNullOrEmpty(token))
            {
                ResultCheckToken = new ResultObject
                {
                    isOk = false,
                    Message = "Error Authentication: Token not found!"
                };
                return;
            }
            UserToken = JwtAuthentication.ParseJwtToken(token);
            if (UserToken == null)
            {
                ResultCheckToken = new ResultObject
                {
                    isOk = false,
                    Message = "Token is invalid or expired"
                };
                return;
            }
            ResultCheckToken.isOk = true;
        }

        private string GetJwtTokenFromHeaderOrCookie(HttpContext? context)
        {
            if (context == null) return string.Empty;
            if (context.Request.Headers.TryGetValue("Authorization", out var bearer))
                return bearer.ToString().Replace("Bearer ", "");

            if (context.Request.Cookies.TryGetValue("JwtToken", out var cookieToken))
                return cookieToken;

            return string.Empty;
        }
    }
}
