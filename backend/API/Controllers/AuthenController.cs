using API.Configurations;
using API.Helper;
using API.Models;
using API.Services.Interfaces;
using API.Utilities;
using API.ViewModels;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenController : ControllerBase
    {
        private readonly IAuthenticateService _iAuthenticate;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Sep490Context _context;
        private readonly HttpClient _httpClient;

        public AuthenController(IAuthenticateService iAuthenticate, IHttpClientFactory httpClientFactory,
            Sep490Context context, IHttpContextAccessor httpContextAccessor)
        {
            _iAuthenticate = iAuthenticate;
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
            _httpContextAccessor = httpContextAccessor;
        }


        [HttpPost("google-callback")]
        public async Task<IActionResult> GoogleCallback1([FromBody] GoogleAuthRequest request)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new[] { ConfigManager.gI().GoogleClientIp }
                };
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.Credential, settings);

                var userInfo = new GoogleUserInfo
                {
                    Email = payload.Email,
                    Name = payload.Name,
                    Picture = payload.Picture,
                    Id = payload.Subject,
                    VerifiedEmail = payload.EmailVerified,
                };

                var (msg, data) = await _iAuthenticate.LoginByGoogle(userInfo, request.CampusId);
                if (!string.IsNullOrEmpty(msg))
                    return BadRequest(new { success = false, message = msg });

                return Ok(new { success = true, message = "Login successful", data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Unable to sign in with Google. Please try again later" + ex.Message });
            }
        }
        [HttpPost("google-callback1")]
        public async Task<IActionResult> GoogleCallback([FromBody] GoogleAuthRequest request)
        {
            try
            {
                var clientId = ConfigManager.gI().GoogleClientIp;
                var clientSecret = ConfigManager.gI().GoogleClientSecert;
                var redirectUri = "https://g77-sep490-su25-ab4781.gitlab.io/login";
                //var redirectUri = "http://localhost:5173/login";

                var tokenRequestData = new Dictionary<string, string>
                {
                    { "code", request.Credential },
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "redirect_uri", redirectUri },
                    { "grant_type", "authorization_code" }
                };

                var tokenResponse = await _httpClient.PostAsync(
                    "https://oauth2.googleapis.com/token",
                    new FormUrlEncodedContent(tokenRequestData)
                );

                var tokenBody = await tokenResponse.Content.ReadAsStringAsync();
                if (!tokenResponse.IsSuccessStatusCode)
                {
                    return BadRequest(new { success = false, message = "Token exchange failed", detail = tokenBody });
                }

                var tokenData = JsonConvert.DeserializeObject<GoogleTokenResponse>(tokenBody);

                // ✅ Sử dụng hàm helper bạn viết
                var userInfo = await GoogleAuthentication.GetUserInfoAsync(tokenData.AccessToken);

                var (msg, data) = await _iAuthenticate.LoginByGoogle(userInfo, request.CampusId);
                if (!string.IsNullOrEmpty(msg))
                    return BadRequest(new { success = false, message = msg });

                return Ok(new { success = true, message = "Login successful", data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Unable to sign in with Google. Please try again later. Error: {ex.Message}" });
            }
        }

        //[HttpPost("google-callback")]
        //public async Task<IActionResult> GoogleCallback([FromBody] GoogleAuthRequest request)
        //{
        //    try
        //    {
        //        var tokenResponse = await GoogleAuthentication.GetAuthAccessTokenAsync(request.Credential, _httpContextAccessor.HttpContext);
        //        var userInfo = await GoogleAuthentication.GetUserInfoAsync(tokenResponse.AccessToken);
        //        var (msg, data) = await _iAuthenticate.LoginByGoogle(userInfo, request.CampusId);
        //        if (msg.Length > 0) { return BadRequest(msg); }

        //        return Ok(new
        //        {
        //            Message = msg,
        //            Data = data,
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { ex.Message, ex.StackTrace });
        //    }
        //}

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] UserLogin input)
        {
            var (msg, data) = await _iAuthenticate.DoLogin(input);
            if (msg.Length > 0)
            {
                if (msg.Equals(ConstMessage.ACCOUNT_UNVERIFIED))
                {
                    _httpContextAccessor.HttpContext.Session.SetString("email_verify", input.Email); // Lưu email to verify
                    msg = await EmailHandler.SendOtpAndSaveSession(input.Email, _httpContextAccessor.HttpContext);
                    if (msg.Length > 0) return BadRequest(new { success = false, message = msg, });

                    return Ok(new { success = true, message = "Mã OTP đã được gửi vào email của bạn!", errorCode = "ACCOUNT_UNVERIFIED" });
                }
                return BadRequest(new { success = false, message = msg, errorCode = "LOGIN_FAILED" });
            }
            return Ok(new { success = true, message = "Đăng nhập thành công!", data });
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegister input)
        {
            var msg = await _iAuthenticate.DoRegister(input);
            if (msg.Length > 0) return BadRequest(new { success = false, message = msg, });

            msg = await EmailHandler.SendOtpAndSaveSession(input.Email, _httpContextAccessor.HttpContext);
            if (msg.Length > 0) return BadRequest(new { success = false, message = msg, });

            return Ok(new { success = true, message = "Mã OTP đã được gửi vào email của bạn!", });
        }

        //[HttpGet("validate-token")]
        //public IActionResult ValidateToken([FromQuery] string refreshToken)
        //{
        //    var token = Request.Cookies["JwtToken"];
        //    if (string.IsNullOrEmpty(token))
        //        return Unauthorized(new { message = "No token found." });

        //    var data = _jwtAuthen.ParseJwtToken(token);
        //    if (data == null) // Token đã hết hạn
        //    {
        //        if (!string.IsNullOrEmpty(refreshToken))
        //        {
        //            var userRefresh = _context.Users.FirstOrDefault(x => x.RefreshToken == refreshToken);
        //            if (userRefresh == null || userRefresh.ExpiryDateToken < DateTime.UtcNow)
        //            {
        //                return Unauthorized(new { message = "Refresh token has expired" });
        //            }
        //            _jwtAuthen.GenerateJwtToken(userRefresh, HttpContext);
        //            var newRefreshToken = _jwtAuthen.GenerateRefreshToken(userRefresh, _context, HttpContext);

        //            return Ok(new
        //            {
        //                refreshToken = newRefreshToken,
        //                isAuthenticated = true,
        //            });
        //        }
        //        return Unauthorized(new { message = "Token không hợp lệ hoặc đã hết hạn." });
        //    }

        //    return Ok(new
        //    {
        //        isAuthenticated = true,
        //        data
        //    });
        //}

        //[HttpPost("refresh-token")]
        //public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest refreshToken)
        //{
        //    if (string.IsNullOrEmpty(refreshToken.RefreshToken))
        //        return Unauthorized(new { message = "Refresh token is missing" });
        //    try
        //    {
        //        var userRefresh = await _context.Users.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken.RefreshToken);
        //        if (userRefresh == null || userRefresh.ExpiryDateToken < DateTime.UtcNow)
        //        {
        //            return Unauthorized(new { message = "Refresh token has expried" });
        //        }

        //        _jwtAuthen.GenerateJwtToken(userRefresh, _httpContextAccessor.HttpContext);  // lưu token vào cookie
        //        var newRefreshToken = _jwtAuthen.GenerateRefreshToken(userRefresh, _context, _httpContextAccessor.HttpContext);

        //        return Ok(new
        //        {
        //            refreshToken = newRefreshToken.Result
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Unauthorized(new { message = "Error exception: " + ex.Message });
        //    }
        //}

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassword input)
        {
            var msg = await _iAuthenticate.DoChangePassword(input);
            if (msg.Length > 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = msg,
                });
            }

            return Ok(new
            {
                success = true,
                message = "Thay đổi mật khẩu thành công!"
            });
        }

        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPassword input)
        {
            var msg = await _iAuthenticate.DoForgetPassword(input);
            if (msg.Length > 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = msg,
                    errorCode = "Reset_Failed"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Mật khẩu mới đã được gửi qua email của bạn!"
            });
        }
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPassword data)
        {
            var msg = await _iAuthenticate.DoResetPassword(data);
            if (msg.Length > 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = msg,
                    errorCode = "Reset_Failed"
                });
            }
            return Ok(new
            {
                success = true,
                message = "Mật khẩu mới đã được thiết lập!"
            });
        }

        [HttpGet("logout")]
        public async Task<IActionResult> DoLogout()
        {
            var msg = _iAuthenticate.DoLogout();
            if (msg.Length > 0) return BadRequest(new { success = false, message = msg });
            return Ok(new { success = true, message = "Đăng xuất thành công!" });
        }
        [HttpGet("get-link-google")]
        public IActionResult GetGoogleLoginLink()
        {
            var clientId = ConfigManager.gI().GoogleClientIp; // Google OAuth ClientId
            var redirectUri = "https://g77-sep490-su25-ab4781.gitlab.io/login"; // URL callback thực tế
            //var redirectUri = "http://localhost:5173/login";
            var scope = "openid profile email";
            var responseType = "code";
            var accessType = "offline"; // Để lấy refresh_token
            var prompt = "consent";     // Luôn yêu cầu xác nhận quyền

            var loginUrl = $"https://accounts.google.com/o/oauth2/v2/auth" +
                           $"?client_id={clientId}" +
                           $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                           $"&response_type={responseType}" +
                           $"&scope={Uri.EscapeDataString(scope)}" +
                           $"&access_type={accessType}" +
                           $"&prompt={prompt}";

            return Ok(new
            {
                success = true,
                url = loginUrl
            });
        }
        [HttpGet("google")]
        public async Task<IActionResult> GoogleLoginGet([FromQuery] string code)
        {
            Console.WriteLine($"Received code: {code}");
            if (string.IsNullOrEmpty(code))
                return BadRequest(new { success = false, message = "Missing code parameter" });

            var clientId = ConfigManager.gI().GoogleClientIp;
            var clientSecret = ConfigManager.gI().GoogleClientSecert;
            var redirectUri = "https://2handshop.id.vn/api/auth/google";

            var tokenRequestData = new Dictionary<string, string>
    {
        { "code", code },
        { "client_id", clientId },
        { "client_secret", clientSecret },
        { "redirect_uri", redirectUri },
        { "grant_type", "authorization_code" }
    };
            Console.WriteLine($"Token Request: {JsonConvert.SerializeObject(tokenRequestData)}");

            try
            {
                var response = await _httpClient.PostAsync(
                    "https://oauth2.googleapis.com/token",
                    new FormUrlEncodedContent(tokenRequestData)
                );
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Google Response: {responseBody}");

                if (!response.IsSuccessStatusCode)
                    return BadRequest(new { success = false, message = "Token exchange failed", detail = responseBody });

                var tokenResponse = JsonConvert.DeserializeObject<GoogleTokenResponse>(responseBody);
                return Ok(new { success = true, token = tokenResponse });
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"JSON Error: {jsonEx.Message}");
                return BadRequest(new { success = false, message = $"Failed to parse token response: {jsonEx.Message}" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(500, new { success = false, message = $"Server error: {ex.Message}" });
            }
        }
        public class GoogleTokenResponse
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonProperty("refresh_token")]
            public string? RefreshToken { get; set; } // Nullable, as it’s not always returned

            [JsonProperty("scope")]
            public string Scope { get; set; }

            [JsonProperty("token_type")]
            public string TokenType { get; set; }

            [JsonProperty("id_token")]
            public string? IdToken { get; set; } // Nullable, as it’s not always returned
        }

        [HttpGet("resend-otp")]
        public async Task<IActionResult> ResendOtp()
        {
            var emailVerify = HttpContext.Session.GetString("email_verify");
            if (string.IsNullOrEmpty(emailVerify))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Vui lòng đăng nhập lại để được verify tài khoản.",
                    email = "email_empty",
                });
            }
            var msg = await EmailHandler.SendOtpAndSaveSession(emailVerify, _httpContextAccessor.HttpContext);
            if (msg.Length > 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = msg,
                });
            }
            return Ok(new
            {
                success = true,
                message = "Mã OTP đã được gửi vào email của bạn!",
            });
        }
        public class RefreshTokenRequest
        {
            public string? RefreshToken { get; set; }
        }
    }
}
