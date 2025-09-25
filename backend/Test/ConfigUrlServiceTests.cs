using API.Commons;
using API.Models;
using API.Services;
using API.ViewModels;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace API.Tests
{
    public class ConfigUrlServiceTests : IDisposable
    {
        private readonly Sep490Context _context;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILog> _mockLogger;
        private readonly ConfigUrlService _service;

        public ConfigUrlServiceTests()
        {
            var options = new DbContextOptionsBuilder<Sep490Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new Sep490Context(options);
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILog>();
            _mockMapper
                .Setup(x => x.Map<ConfigUrlVM>(It.IsAny<ConfigUrl>()))
                .Returns<ConfigUrl>(c => new ConfigUrlVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    Url = c.Url
                });
            _mockMapper
                .Setup(x => x.Map<List<ConfigUrlVM>>(It.IsAny<List<ConfigUrl>>()))
                .Returns<List<ConfigUrl>>(list => list.Select(c => new ConfigUrlVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    Url = c.Url
                }).ToList());

            _mockLogger.Setup(x => x.WriteActivity(It.IsAny<AddUserLogVM>())).ReturnsAsync("");
            _service = new ConfigUrlService(_context, _mockMapper.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetOne_NullId_ReturnsErrorMessage()
        {
            var (message, result) = await _service.GetOne(null);
            Assert.Equal("Id cannot be null or empty", message);
            Assert.Null(result);
        }


        [Fact]
        public async Task GetOne_ValidId_ReturnsNoMessage()
        {
            _context.ConfigUrls.Add(new ConfigUrl { Id = "1", Name = "a", Url = "http://test.com" });
            await _context.SaveChangesAsync();
            var (message, result) = await _service.GetOne("1");
            Assert.Equal("", message);
            Assert.NotNull(result);
        }


        [Fact]
        public async Task GetOne_InValidId_ReturnsErrorMessage()
        {
            _context.ConfigUrls.Add(new ConfigUrl { Id = "1", Name = "a", Url = "http://test.com" });
            await _context.SaveChangesAsync();
            var (message, result) = await _service.GetOne("2");
            Assert.Equal("ConfigUrl not found", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAll_NoConfigUrlsFound_ReturnsErrorMessage()
        {
            var (message, result) = await _service.GetAll();
            Assert.Equal("No ConfigUrls found", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task ToggleConfigUrl_NullId_ReturnsErrorMessage()
        {
            var message = await _service.ToggleConfigUrl(null, "token");
            Assert.Equal("Id cannot be null or empty", message);
        }

        [Fact]
        public async Task CreateUpdate_UrlDuplicate_ReturnsErrorMessage()
        {
            _context.ConfigUrls.Add(new ConfigUrl { Id = "1", Name="a", Url = "http://test.com" });
            await _context.SaveChangesAsync();
            var input = new CreateUpdateConfigUrlVM { Url = "http://test.com" };
            var (message, result) = await _service.CreateUpdate(input, "token");
            Assert.Contains("already exists", message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task DoRemove_NullUrls_ReturnsErrorMessage()
        {
            var (message, result) = await _service.DoRemove(null, "token");
            Assert.Equal("App Id cannot be null or empty.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetOne_ValidId_ReturnsConfigUrl()
        {
            _context.ConfigUrls.Add(new ConfigUrl { Id = "1", Name = "Test", Url = "http://test.com" });
            await _context.SaveChangesAsync();
            var (message, result) = await _service.GetOne("1");
            Assert.Equal("", message);
            Assert.NotNull(result);
            Assert.Equal("http://test.com", result.Url);
        }

        [Fact]
        public async Task GetAll_WithConfigUrls_ReturnsList()
        {
            _context.ConfigUrls.Add(new ConfigUrl { Id = "1", Name = "Test", Url = "http://test.com" });
            await _context.SaveChangesAsync();
            var (message, result) = await _service.GetAll();
            Assert.Equal("", message);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task ToggleConfigUrl_ValidId_ReturnsSuccess()
        {
            _context.ConfigUrls.Add(new ConfigUrl { Id = "1", Name = "Test", Url = "http://test.com", IsActive = false });
            await _context.SaveChangesAsync();
            var message = await _service.ToggleConfigUrl("1", "token");
            Assert.Equal("", message);
        }

        [Fact]
        public async Task CreateUpdate_ValidInput_ReturnsSuccess()
        {
            var input = new CreateUpdateConfigUrlVM { Name = "Test", Url = "http://test2.com" };
            var (message, result) = await _service.CreateUpdate(input, "token");
            Assert.Equal("", message);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task DoRemove_ValidIds_ReturnsSuccess()
        {
            _context.ConfigUrls.Add(new ConfigUrl { Id = "1", Name = "Test", Url = "http://test.com" });
            await _context.SaveChangesAsync();
            var (message, result) = await _service.DoRemove(new List<string> { "http://test.com" }, "token");
            Assert.Equal("", message);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task DoRemove_NotFoundIds_ReturnsError()
        {
            var (message, result) = await _service.DoRemove(new List<string> { "notfound" }, "token");
            Assert.Contains("No prohibited apps found for the provided", message, StringComparison.OrdinalIgnoreCase);
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateUpdate_SpecialCharacters_ReturnsSuccess()
        {
            var input = new CreateUpdateConfigUrlVM { Name = "!@#$%^&*()_+", Url = "http://special.com" };
            var (message, result) = await _service.CreateUpdate(input, "token");
            Assert.Equal("", message);
            Assert.NotNull(result);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
} 