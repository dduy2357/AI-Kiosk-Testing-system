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
    public class DisabledKeyServiceTests : IDisposable
    {
        private readonly Sep490Context _context;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILog> _mockLogger;
        private readonly DisabledKeyService _service;

        public DisabledKeyServiceTests()
        {
            var options = new DbContextOptionsBuilder<Sep490Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new Sep490Context(options);
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILog>();
            _mockLogger.Setup(x => x.WriteActivity(It.IsAny<AddUserLogVM>())).ReturnsAsync("");
            _mockMapper
              .Setup(x => x.Map<DisabledKeyVM>(It.IsAny<DisabledKey>()))
              .Returns<DisabledKey>(c => new DisabledKeyVM
              {
                  KeyId = c.KeyId,
                  KeyCode = c.KeyCode,
                  KeyCombination = c.KeyCombination,
                  IsActive = c.IsActive,
                  CreatedAt = c.CreatedAt,
              });
            _mockMapper
                .Setup(x => x.Map<List<DisabledKeyVM>>(It.IsAny<List<DisabledKeyVM>>()))
                .Returns<List<DisabledKey>>(list => list.Select(c => new DisabledKeyVM
                {
                    KeyId = c.KeyId,
                    KeyCode = c.KeyCode,
                    KeyCombination = c.KeyCombination,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                }).ToList());

            _mockMapper
                .Setup(x => x.Map<List<DisabledKeyVM>>(It.IsAny<List<DisabledKey>>()))
                .Returns<List<DisabledKey>>(list => list.Select(x => new DisabledKeyVM
                {
                    KeyId = x.KeyId,
                    KeyCode = x.KeyCode,
                    KeyCombination = x.KeyCombination,
                    IsActive = x.IsActive,
                    RiskLevel = x.RiskLevel
                }).ToList());

            _service = new DisabledKeyService(_context, _mockMapper.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAll_NoKeysFound_ReturnsErrorMessage()
        {
            var search = new DisabledKeySearchVM { CurrentPage = 1, PageSize = 10 };
            var (message, result) = await _service.GetAll(search);
            Assert.Equal("No disabled keys found.", message);
            Assert.Null(result.Result);
        }

        [Fact]
        public async Task GetOne_NullId_ReturnsErrorMessage()
        {
            var (message, result) = await _service.GetOne(null);
            Assert.Equal("Key Id cannot null or empty.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task ChangeActivate_NullUserToken_ReturnsErrorMessage()
        {
            var (message, result) = await _service.ChangeActivate(new List<string> { "1" }, null);
            Assert.Equal("Current user ID is required.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task DoDelete_NullKeyIds_ReturnsErrorMessage()
        {
            var (message, result) = await _service.DoDelete(null, "token");
            Assert.Equal("Key Id cannot be null or empty.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateUpdate_NullInput_ReturnsErrorMessage()
        {
            var message = await _service.CreateUpdate(null, "token");
            Assert.Equal("Input cannot be null.", message);
        }

        [Fact]
        public async Task GetAll_WithKeys_ReturnsList()
        {
            _context.DisabledKeys.Add(new DisabledKey { KeyId = "1", KeyCode = "Ctrl+Alt+Del", KeyCombination = "s", IsActive = true });
            await _context.SaveChangesAsync();
            var search = new DisabledKeySearchVM { CurrentPage = 1, PageSize = 5 };
            var (message, result) = await _service.GetAll(search);
            Assert.Equal("", message);
            Assert.NotNull(result.Result);
        }

        [Fact]
        public async Task GetOne_ValidId_ReturnsKey()
        {
            _context.DisabledKeys.Add(new DisabledKey { KeyId = "1", KeyCode = "Ctrl+Alt+Del", KeyCombination = "s", IsActive = true });
            await _context.SaveChangesAsync();
            var (message, result) = await _service.GetOne("1");
            Assert.Equal("", message);
            Assert.NotNull(result);
            Assert.Equal("Ctrl+Alt+Del", result.KeyCode);
        }

        [Fact]
        public async Task ChangeActivate_Valid_ReturnsResult()
        {
            _context.DisabledKeys.Add(new DisabledKey { KeyId = "1", KeyCode = "Ctrl+Alt+Del", KeyCombination = "s", IsActive = false });
            await _context.SaveChangesAsync();
            var (message, result) = await _service.ChangeActivate(new List<string> { "1" }, "user1");
            Assert.Equal("", message);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task DoDelete_ValidKeyIds_ReturnsResult()
        {
            _context.DisabledKeys.Add(new DisabledKey { KeyId = "1", KeyCode = "Ctrl+Alt+Del", KeyCombination = "s", IsActive = true });
            await _context.SaveChangesAsync();
            var (message, result) = await _service.DoDelete(new List<string> { "1" }, "token");
            Assert.Equal("", message);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task DoDelete_NotFoundKeyIds_ReturnsError()
        {
            var (message, result) = await _service.DoDelete(new List<string> { "notfound" }, "token");
            Assert.Contains("No matching keys found.", message, StringComparison.OrdinalIgnoreCase);
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateUpdate_ValidInput_ReturnsSuccess()
        {
            var input = new CreateUpdateDisabledKeyVM { KeyCode = "Ctrl+Shift+Esc", KeyCombination = "c", IsActive = true };
            var message = await _service.CreateUpdate(input, "token");
            Assert.Equal("", message);
        }

        [Fact]
        public async Task CreateUpdate_DuplicateKey_ReturnsError()
        {
            _context.DisabledKeys.Add(new DisabledKey { KeyId = "1", KeyCode = "Ctrl+Alt+Del", KeyCombination = "s", IsActive = true });
            await _context.SaveChangesAsync();
            var input = new CreateUpdateDisabledKeyVM { KeyCode = "Ctrl+Alt+Del", KeyCombination = "s", IsActive = true };
            var message = await _service.CreateUpdate(input, "token");
            Assert.Contains("already exists", message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ChangeActivate_InvalidKeyIds_ReturnsError()
        {
            var (message, result) = await _service.ChangeActivate(new List<string> { "notfound" }, "user1");
            Assert.Contains("No matching keys found.", message, StringComparison.OrdinalIgnoreCase);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAll_Pagination_WorksCorrectly()
        {
            for (int i = 0; i < 12; i++)
                _context.DisabledKeys.Add(new DisabledKey { KeyId = $"k{i}", KeyCode = $"Key{i}", KeyCombination = "s", IsActive = true });
            await _context.SaveChangesAsync();
            var search = new DisabledKeySearchVM { CurrentPage = 2, PageSize = 5 };
            var (message, result) = await _service.GetAll(search);
            Assert.Equal("", message);
            Assert.NotNull(result.Result);
        }

        [Fact]
        public async Task CreateUpdate_SpecialCharacters_ReturnsSuccess()
        {
            var input = new CreateUpdateDisabledKeyVM { KeyCode = "!@#$%^&*()_+", KeyCombination = "c", IsActive = true };
            var message = await _service.CreateUpdate(input, "token");
            Assert.Equal("", message);
        }

        [Fact]
        public async Task CreateUpdate_LongKeyCode_ReturnsSuccess()
        {
            var longKey = new string('a', 1000);
            var input = new CreateUpdateDisabledKeyVM { KeyCode = longKey, KeyCombination = "c", IsActive = true };
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