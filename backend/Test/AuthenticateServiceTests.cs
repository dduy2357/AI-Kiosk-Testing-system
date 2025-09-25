using Xunit;
using Moq;
using API.Services;
using API.Models;
using API.ViewModels;
using API.ViewModels.Token;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using AutoMapper;
using API.Commons;
using API.Configurations;
using Microsoft.Extensions.Configuration;

namespace API.Tests;

public class AuthenticateServiceTests
{
    private readonly Sep490Context _context;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly AuthenticateService _service;

    public AuthenticateServiceTests()
    {
        #region Config
        var inMemorySettings = new Dictionary<string, string>
        {
            {"EmailSettings:EmailDisplayName", "test"},
            {"EmailSettings:EmailHost", "smtp.test.com"},
            {"EmailSettings:EmailUsername", "test@test.com"},
            {"EmailSettings:EmailPassword", "test"},
            {"JwtSettings:SecretKey", "this_is_a_test_secret_key_123456789"}, // ít nhất 16 bytes
            {"JwtSettings:Issuer", "testIssuer"},
            {"JwtSettings:Audience", "testAudience"},
            {"JwtSettings:ExpiresInMinutes", "30"}
        };


        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        ConfigManager.CreateManager(configuration);
        #endregion
        // Fake ConfigManager để không bị null
        ConfigManager.gI().EmailDisplayName = "test";
        ConfigManager.gI().EmailHost = "smtp.test.com";
        ConfigManager.gI().EmailUsername = "test@test.com";
        ConfigManager.gI().EmailPassword = "test";
        ConfigManager.gI().SecretKey = "this_is_a_test_secret_key_123456789";
        ConfigManager.gI().Issuer = "testIssuer";
        ConfigManager.gI().Audience = "testAudience";
        ConfigManager.gI().ExpiresInMinutes = 30;

        var options = new DbContextOptionsBuilder<Sep490Context>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new Sep490Context(options);
        _mockMapper = new Mock<IMapper>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        _service = new AuthenticateService(_mockMapper.Object, _context, _mockHttpContextAccessor.Object);
    }

    [Fact]
    public async Task DoLogin_EmailNotFound_ReturnsError()
    {
        var input = new UserLogin { Email = "notfound@email.com", Password = "123", CampusId = "1" };
        var (msg, loginResult) = await _service.DoLogin(input);
        Assert.Contains("Incorrect email or password", msg);
        Assert.Null(loginResult);
    }

    [Fact]
    public async Task DoLogin_WrongPassword_ReturnsError()
    {
        var user = new User { UserId = "u1", Email = "test@email.com", Password = "md5", CampusId = "1", Status = 1 };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        var input = new UserLogin { Email = "test@email.com", Password = "wrong", CampusId = "1" };
        var (msg, loginResult) = await _service.DoLogin(input);
        Assert.Contains("Incorrect email or password", msg);
        Assert.Null(loginResult);
    }

    [Fact]
    public async Task DoLogin_UserInactive_ReturnsError()
    {
        Converter.StringToMD5("md5", out string mkMd5);
        var user = new User { UserId = "u1", Email = "inactive@email.com", Password = mkMd5, CampusId = "1", Status = 0 };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        var input = new UserLogin { Email = "inactive@email.com", Password = "md5", CampusId = "1" };
        var (msg, loginResult) = await _service.DoLogin(input);
        Assert.Contains("deactivated", msg);
        Assert.Null(loginResult);
    }

    [Fact]
    public async Task LoginByGoogle_EmailNotFpt_ReturnsError()
    {
        var input = new GoogleUserInfo { Email = "user@gmail.com", Id = "gid", Name = "Test", Picture = "pic" };
        var (msg, loginResult) = await _service.LoginByGoogle(input, "1");
        Assert.Contains("@fpt.edu.vn", msg);
        Assert.Null(loginResult);
    }

    [Fact]
    public async Task LoginByGoogle_UserNotFound_ReturnsError()
    {
        var input = new GoogleUserInfo { Email = "user@fpt.edu.vn", Id = "gid", Name = "Test", Picture = "pic" };
        var (msg, loginResult) = await _service.LoginByGoogle(input, "1");
        Assert.Contains("not authorized", msg);
        Assert.Null(loginResult);
    }

    [Fact]
    public async Task DoLogin_CampusIdMismatch_ReturnsError()
    {
        Converter.StringToMD5("md5", out string mkMd5);
        var user = new User { UserId = "u1", Email = "test2@email.com", Password = mkMd5, CampusId = "2", Status = 1 };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        var input = new UserLogin { Email = "test2@email.com", Password = "md5", CampusId = "1" };
        var (msg, loginResult) = await _service.DoLogin(input);
        Assert.Contains("does not match the selected campus", msg);
        Assert.Null(loginResult);
    }

    [Fact]
    public async Task DoLogin_PasswordEmpty_ReturnsError()
    {
        var user = new User { UserId = "u1", Email = "test3@email.com", Password = "", CampusId = "1", Status = 1 };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        var input = new UserLogin { Email = "test3@email.com", Password = "123", CampusId = "1" };
        var (msg, loginResult) = await _service.DoLogin(input);
        Assert.Contains("Password is empty", msg);
        Assert.Null(loginResult);
    }

    [Fact]
    public async Task DoLogin_Success_ReturnsToken()
    {
        Converter.StringToMD5("md5", out string mkMd5);
        var user = new User { UserId = "u1", Email = "success@email.com", Password = mkMd5, CampusId = "1", Status = 1 };
        _context.Users.Add(user);
        _context.UserRoles.Add(new UserRole { UserId = "u1", RoleId = 1 });
        await _context.SaveChangesAsync();
        // Giả lập IMapper.Map trả về UserToken
        _mockMapper.Setup(m => m.Map<UserToken>(It.IsAny<User>())).Returns(new UserToken { UserID = "u1" });
        var input = new UserLogin { Email = "success@email.com", Password = "md5", CampusId = "1" };
        var (msg, loginResult) = await _service.DoLogin(input);
        Assert.True(string.IsNullOrEmpty(msg));
        Assert.NotNull(loginResult);
        Assert.NotNull(loginResult.AccessToken);
        Assert.NotNull(loginResult.Data);
    }

    [Fact]
    public async Task LoginByGoogle_CampusIdMismatch_ReturnsError()
    {
        var user = new User { UserId = "g1", Email = "g1@fpt.edu.vn", GoogleId = "gid1", CampusId = "2", Status = 1 };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        var input = new GoogleUserInfo { Email = "g1@fpt.edu.vn", Id = "gid1", Name = "Test", Picture = "pic" };
        var (msg, loginResult) = await _service.LoginByGoogle(input, "1");
        Assert.Contains("does not match the selected campus", msg);
        Assert.Null(loginResult);
    }

    [Fact]
    public async Task LoginByGoogle_UserInactive_ReturnsError()
    {
        var user = new User { UserId = "g2", Email = "g2@fpt.edu.vn", GoogleId = "gid2", CampusId = "1", Status = 0 };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        var input = new GoogleUserInfo { Email = "g2@fpt.edu.vn", Id = "gid2", Name = "Test", Picture = "pic" };
        var (msg, loginResult) = await _service.LoginByGoogle(input, "1");
        Assert.Contains("deactivated", msg);
        Assert.Null(loginResult);
    }

    //[Fact]
    //public async Task LoginByGoogle_Success_ReturnsToken()
    //{
    //    // Seed Role
    //    _context.Roles.Add(new Role { Id = 1, Name = "Admin", IsActive = true });
    //    // Seed User + UserRole
    //    var user = new User { UserId = "g3", Email = "g3@fpt.edu.vn", GoogleId = "gid3", CampusId = "1", Status = 1 };
    //    _context.Users.Add(user);
    //    _context.UserRoles.Add(new UserRole { UserId = "g3", RoleId = 1 });
    //    await _context.SaveChangesAsync();

    //    // Fake mapper
    //    _mockMapper.Setup(m => m.Map<UserToken>(It.IsAny<User>()))
    //               .Returns(new UserToken { UserID = "g3" });

    //    // Input google user
    //    var input = new GoogleUserInfo { Email = "g3@fpt.edu.vn", Id = "gid3", Name = "Test", Picture = "pic" };

    //    // Act
    //    var (msg, loginResult) = await _service.LoginByGoogle(input, "1");

    //    // Assert
    //    Assert.True(string.IsNullOrEmpty(msg));
    //    Assert.NotNull(loginResult);
    //    Assert.NotNull(loginResult.AccessToken);
    //    Assert.NotNull(loginResult.Data);
    //}


    //[Fact]
    //public async Task DoChangePassword_UserNotFound_ReturnsError()
    //{
    //    var input = new ChangePassword { UserId = "notfound", ExPassword = "old", Password = "new" };
    //    var result = await _service.DoChangePassword(input);
    //    Assert.Equal("User not found.", result);
    //}

    //[Fact]
    //public async Task DoChangePassword_SamePassword_ReturnsError()
    //{
    //    Converter.StringToMD5("same", out string oldPasswordMd5);
    //    var user = new User { UserId = "u1", Password = oldPasswordMd5 };
    //    _context.Users.Add(user);
    //    await _context.SaveChangesAsync();
    //    var input = new ChangePassword { UserId = "u1", ExPassword = "same", Password = "same" };
    //    var result = await _service.DoChangePassword(input);
    //    Assert.Contains("New password must be different", result);
    //}

    //[Fact]
    //public async Task DoChangePassword_Success_ReturnsEmpty()
    //{
    //    Converter.StringToMD5("oldmd5", out string oldPasswordMd5);
    //    var user = new User { UserId = "u2", Password = oldPasswordMd5 };
    //    _context.Users.Add(user);
    //    await _context.SaveChangesAsync();

    //    var input = new ChangePassword
    //    {
    //        UserId = "u2",
    //        ExPassword = "oldmd5",
    //        Password = "newmd5"
    //    };

    //    var result = await _service.DoChangePassword(input);
    //    Assert.True(string.IsNullOrEmpty(result));
    //}

    //[Fact]
    //public async Task DoForgetPassword_EmailNotExist_ReturnsError()
    //{
    //    var input = new ForgetPassword { Email = "notfound@email.com" };
    //    var result = await _service.DoForgetPassword(input);
    //    Assert.Contains("not exist", result);
    //}

    //[Fact]
    //public async Task DoForgetPassword_Success_ReturnsEmpty()
    //{
    //    var user = new User { UserId = "f1", Email = "f1@email.com", Password = "md5" };
    //    _context.Users.Add(user);
    //    await _context.SaveChangesAsync();
    //    var input = new ForgetPassword { Email = "f1@email.com" };
    //    var result = await _service.DoForgetPassword(input);
    //    Assert.True(string.IsNullOrEmpty(result));
    //}

    //[Fact]
    //public async Task DoResetPassword_UserIdNull_ReturnsError()
    //{
    //    var input = new ResetPassword { UserId = null, Password = "12345678", RePassword = "12345678" };
    //    var result = await _service.DoResetPassword(input);
    //    Assert.Equal("UserId is null", result);
    //}

    //[Fact]
    //public async Task DoResetPassword_UserNotFound_ReturnsError()
    //{
    //    var input = new ResetPassword { UserId = "notfound", Password = "12345678", RePassword = "12345678" };
    //    var result = await _service.DoResetPassword(input);
    //    Assert.Equal("User not found.", result);
    //}

    //[Fact]
    //public async Task DoResetPassword_Success_ReturnsEmpty()
    //{
    //    var user = new User { UserId = "r1", Password = "oldmd5" };
    //    _context.Users.Add(user);
    //    await _context.SaveChangesAsync();
    //    var input = new ResetPassword { UserId = "r1", Password = "newmd5", RePassword = "newmd5" };
    //    var result = await _service.DoResetPassword(input);
    //    Assert.True(string.IsNullOrEmpty(result));
    //}

    //[Fact]
    //public async Task DoSearchByEmail_EmailInvalid_ReturnsError()
    //{
    //    var (msg, user) = await _service.DoSearchByEmail("");
    //    Assert.Contains("not valid", msg);
    //    Assert.Null(user);
    //}

    //[Fact]
    //public async Task DoSearchByEmail_EmailNotExist_ReturnsError()
    //{
    //    var (msg, user) = await _service.DoSearchByEmail("notfound@email.com");
    //    Assert.Contains("not exist", msg);
    //    Assert.Null(user);
    //}

    //[Fact]
    //public async Task DoSearchByEmail_Success_ReturnsUser()
    //{
    //    var user = new User { UserId = "s1", Email = "s1@email.com", Password = "md5" };
    //    _context.Users.Add(user);
    //    await _context.SaveChangesAsync();
    //    var (msg, found) = await _service.DoSearchByEmail("s1@email.com");
    //    Assert.True(string.IsNullOrEmpty(msg));
    //    Assert.NotNull(found);
    //    Assert.Equal("s1", found.UserId);
    //}
}
