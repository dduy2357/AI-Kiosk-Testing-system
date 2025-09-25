using API.Commons;
using API.Models;
using API.Services;
using API.ViewModels;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace API.Tests
{
    public class ProhibitedAppServiceTests : IDisposable
    {
        private readonly Sep490Context _context;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILog> _mockLogger;
        private readonly ProhibitedAppService _service;

        public ProhibitedAppServiceTests()
        {
            var options = new DbContextOptionsBuilder<Sep490Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new Sep490Context(options);
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILog>();
            _mockMapper
           .Setup(x => x.Map<ProhibitedAppVM>(It.IsAny<ProhibitedApp>()))
           .Returns<ProhibitedApp>(c => new ProhibitedAppVM
           {
               AppId = c.AppId,
               AppName = c.AppName,
               ProcessName = c.ProcessName,
               IsActive = c.IsActive,
               CreatedAt = c.CreatedAt,
               UpdatedAt = c.UpdatedAt
           });
            _mockMapper
                .Setup(x => x.Map<List<ProhibitedAppVM>>(It.IsAny<List<ProhibitedApp>>()))
                .Returns<List<ProhibitedApp>>(list => list.Select(c => new ProhibitedAppVM
                {
                    AppId = c.AppId,
                    AppName = c.AppName,
                    ProcessName = c.ProcessName,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                }).ToList());
            _mockLogger.Setup(x => x.WriteActivity(It.IsAny<AddUserLogVM>())).ReturnsAsync("");
            _service = new ProhibitedAppService(_context, _mockMapper.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAll_NoAppsFound_ReturnsErrorMessage()
        {
            var search = new ProhibitedAppSearchVM { CurrentPage = 1, PageSize = 10 };
            var (message, result) = await _service.GetAll(search);
            Assert.Equal("No prohibited apps found.", message);
            Assert.Null(result.Result);
        }

        [Fact]
        public async Task GetOne_NullId_ReturnsErrorMessage()
        {
            var (message, result) = await _service.GetOne(null);
            Assert.Equal("App ID cannot be null or empty.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task ChangeActivate_NullAppIds_ReturnsErrorMessage()
        {
            var (message, result) = await _service.ChangeActivate(null, "token");
            Assert.Equal("App Id cannot be null or empty.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateUpdate_DuplicateAppName_ReturnsErrorMessage()
        {
            _context.ProhibitedApps.Add(new ProhibitedApp { AppId = "1", AppName = "App1", ProcessName = "proc1" });
            await _context.SaveChangesAsync();
            var input = new CreateUpdateProhibitedAppVM { AppName = "App1", ProcessName = "proc2" };
            var message = await _service.CreateUpdate(input, "token");
            Assert.Contains("already in use", message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task DoRemove_NullAppIds_ReturnsErrorMessage()
        {
            var (message, result) = await _service.DoRemove(null, "token");
            Assert.Equal("App Id cannot be null or empty.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAll_WithApps_ReturnsList()
        {
            _context.ProhibitedApps.Add(new ProhibitedApp { AppId = "1", AppName = "App1", ProcessName = "proc1", IsActive = true });
            await _context.SaveChangesAsync();
            var search = new ProhibitedAppSearchVM { CurrentPage = 1, PageSize = 10 };
            var (message, result) = await _service.GetAll(search);
            Assert.Equal("", message);
            Assert.NotNull(result.Result);
        }

        [Fact]
        public async Task GetOne_ValidId_ReturnsApp()
        {
            _context.ProhibitedApps.Add(new ProhibitedApp { AppId = "1", AppName = "App1", ProcessName = "proc1", IsActive = true });
            await _context.SaveChangesAsync();
            var (message, result) = await _service.GetOne("1");
            Assert.Equal("", message);
            Assert.NotNull(result);
            Assert.Equal("App1", result.AppName);
        }

        [Fact]
        public async Task ChangeActivate_Valid_ReturnsResult()
        {
            _context.ProhibitedApps.Add(new ProhibitedApp { AppId = "1", AppName = "App1", ProcessName = "proc1", IsActive = false });
            await _context.SaveChangesAsync();
            var (message, result) = await _service.ChangeActivate(new List<string> { "1" }, "token");
            Assert.Equal("", message);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task DoRemove_ValidAppIds_ReturnsResult()
        {
            _context.ProhibitedApps.Add(new ProhibitedApp { AppId = "1", AppName = "App1", ProcessName = "proc1", IsActive = true });
            await _context.SaveChangesAsync();
            var (message, result) = await _service.DoRemove(new List<string> { "1" }, "token");
            Assert.Equal("", message);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task DoRemove_NotFoundAppIds_ReturnsError()
        {
            var (message, result) = await _service.DoRemove(new List<string> { "notfound" }, "token");
            Assert.Contains("No prohibited apps found for the provided", message, StringComparison.OrdinalIgnoreCase);
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateUpdate_ValidInput_ReturnsSuccess()
        {
            var input = new CreateUpdateProhibitedAppVM { AppName = "App2", ProcessName = "proc2", IsActive = true };
            var message = await _service.CreateUpdate(input, "token");
            Assert.Equal("", message);
        }

        [Fact]
        public async Task CreateUpdate_DuplicateProcessName_ReturnsError()
        {
            _context.ProhibitedApps.Add(new ProhibitedApp { AppId = "1", AppName = "App1", ProcessName = "proc1" });
            await _context.SaveChangesAsync();
            var input = new CreateUpdateProhibitedAppVM { AppName = "App2", ProcessName = "proc1" };
            var message = await _service.CreateUpdate(input, "token");
            Assert.Contains("already in use", message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ChangeActivate_InvalidAppIds_ReturnsError()
        {
            var (message, result) = await _service.ChangeActivate(new List<string> { "notfound" }, "token");
            Assert.Contains("No prohibited apps found for the provided", message, StringComparison.OrdinalIgnoreCase);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAll_Pagination_WorksCorrectly()
        {
            for (int i = 0; i < 12; i++)
                _context.ProhibitedApps.Add(new ProhibitedApp { AppId = $"a{i}", AppName = $"App{i}", ProcessName = $"proc{i}", IsActive = true });
            await _context.SaveChangesAsync();
            var search = new ProhibitedAppSearchVM { CurrentPage = 2, PageSize = 10 };
            var (message, result) = await _service.GetAll(search);
            Assert.Equal("", message);
            Assert.NotNull(result.Result);
        }

        [Fact]
        public async Task CreateUpdate_SpecialCharacters_ReturnsSuccess()
        {
            var input = new CreateUpdateProhibitedAppVM { AppName = "!@#$%^&*()_+", ProcessName = "!@#$%^&*()_+", IsActive = true };
            var message = await _service.CreateUpdate(input, "token");
            Assert.Equal("", message);
        }

        [Fact]
        public async Task CreateUpdate_LongAppName_ReturnsSuccess()
        {
            var longName = new string('a', 1000);
            var input = new CreateUpdateProhibitedAppVM { AppName = longName, ProcessName = "proc3", IsActive = true };
            var message = await _service.CreateUpdate(input, "token");
            Assert.Equal("", message);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}