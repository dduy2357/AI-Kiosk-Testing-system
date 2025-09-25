//using API.Commons;
//using API.Helper;
//using API.Models;
//using API.Services;
//using API.ViewModels;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Xunit;

//namespace API.Tests
//{
//    public class ConfigDesktopServiceTests : IDisposable
//    {
//        private readonly Sep490Context _context;
//        private readonly ConfigDesktopService _service;

//        public ConfigDesktopServiceTests()
//        {
//            var options = new DbContextOptionsBuilder<Sep490Context>()
//                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
//                .Options;
//            _context = new Sep490Context(options);
//            _service = new ConfigDesktopService(_context);
//        }

//        [Fact]
//        public async Task GetConfigurations_NoProhibitedApps_ReturnsErrorMessage()
//        {
//            var (message, config) = await _service.GetConfigurations();
//            Assert.Equal("No prohibited applications found.", message);
//            Assert.Null(config);
//        }

//        [Fact]
//        public async Task GetConfigurations_NoShortcutKeys_ReturnsErrorMessage()
//        {
//            _context.ProhibitedApps.Add(new ProhibitedApp { AppId = "1", ProcessName = "proc1", AppName="a", IsActive = true, TypeApp = (int)TypeApp.Prohibited });
//            await _context.SaveChangesAsync();
//            var (message, config) = await _service.GetConfigurations();
//            Assert.Equal("No shortcut keys found.", message);
//            Assert.Null(config);
//        }

//        [Fact]
//        public async Task GetConfigurations_NoActiveUrl_ReturnsErrorMessage()
//        {
//            _context.ProhibitedApps.Add(new ProhibitedApp { AppId = "1", ProcessName = "proc1", AppName="x", IsActive = true, TypeApp = (int)TypeApp.Prohibited });
//            _context.DisabledKeys.Add(new DisabledKey { KeyId = "1", KeyCode = "Ctrl+Alt+Del", KeyCombination="s", IsActive = true });
//            await _context.SaveChangesAsync();
//            var (message, config) = await _service.GetConfigurations();
//            Assert.Equal("No active URL configuration found.", message);
//            Assert.Null(config);
//        }

//        [Fact]
//        public async Task GetConfigurations_AllValid_ReturnsConfig()
//        {
//            _context.ProhibitedApps.Add(new ProhibitedApp { AppId = "1", ProcessName = "proc1", AppName = "x", IsActive = true, TypeApp = (int)TypeApp.Prohibited });
//            _context.DisabledKeys.Add(new DisabledKey { KeyId = "1", KeyCode = "Ctrl+Alt+Del", KeyCombination = "s", IsActive = true });
//            _context.ConfigUrls.Add(new ConfigUrl { Id = "1", Name = "Test URL", Url = "http://test.com", IsActive = true });
//            await _context.SaveChangesAsync();
//            var (message, config) = await _service.GetConfigurations();
//            Assert.Equal("", message);
//            Assert.NotNull(config);
//            Assert.Equal("http://test.com", config.ProtectedUrl);
//            Assert.Contains("proc1", config.BlockedApps);
//            Assert.Contains("Ctrl+Alt+Del", config.ShortcutKeys);
//        }

//        [Fact]
//        public async Task GetConfigurations_MultipleProhibitedApps_ReturnsConfig()
//        {
//            _context.ProhibitedApps.Add(new ProhibitedApp { AppId = "1", ProcessName = "proc1", AppName = "a", IsActive = true, TypeApp = (int)TypeApp.Prohibited });
//            _context.ProhibitedApps.Add(new ProhibitedApp { AppId = "2", ProcessName = "proc2", AppName = "b", IsActive = true, TypeApp = (int)TypeApp.Prohibited });
//            _context.DisabledKeys.Add(new DisabledKey { KeyId = "1", KeyCode = "Ctrl+Alt+Del", KeyCombination = "s", IsActive = true });
//            _context.ConfigUrls.Add(new ConfigUrl { Id = "1", Name = "Test URL", Url = "http://test.com", IsActive = true });
//            await _context.SaveChangesAsync();
//            var (message, config) = await _service.GetConfigurations();
//            Assert.Equal("", message);
//            Assert.NotNull(config);
//            Assert.Contains("proc1", config.BlockedApps);
//            Assert.Contains("proc2", config.BlockedApps);
//        }

//        [Fact]
//        public async Task GetConfigurations_MultipleShortcutKeys_ReturnsConfig()
//        {
//            _context.ProhibitedApps.Add(new ProhibitedApp { AppId = "1", ProcessName = "proc1", AppName = "a", IsActive = true, TypeApp = (int)TypeApp.Prohibited });
//            _context.DisabledKeys.Add(new DisabledKey { KeyId = "1", KeyCode = "Ctrl+Alt+Del", KeyCombination = "s", IsActive = true });
//            _context.DisabledKeys.Add(new DisabledKey { KeyId = "2", KeyCode = "Alt+Tab", KeyCombination = "a", IsActive = true });
//            _context.ConfigUrls.Add(new ConfigUrl { Id = "1", Name = "Test URL", Url = "http://test.com", IsActive = true });
//            await _context.SaveChangesAsync();
//            var (message, config) = await _service.GetConfigurations();
//            Assert.Equal("", message);
//            Assert.NotNull(config);
//            Assert.Contains("Ctrl+Alt+Del", config.ShortcutKeys);
//            Assert.Contains("Alt+Tab", config.ShortcutKeys);
//        }

//        [Fact]
//        public async Task GetConfigurations_MultipleUrls_ReturnsFirstActiveUrl()
//        {
//            _context.ProhibitedApps.Add(new ProhibitedApp { AppId = "1", ProcessName = "proc1", AppName = "a", IsActive = true, TypeApp = (int)TypeApp.Prohibited });
//            _context.DisabledKeys.Add(new DisabledKey { KeyId = "1", KeyCode = "Ctrl+Alt+Del", KeyCombination = "s", IsActive = true });
//            _context.ConfigUrls.Add(new ConfigUrl { Id = "1", Name = "Test URL", Url = "http://test.com", IsActive = false });
//            _context.ConfigUrls.Add(new ConfigUrl { Id = "2", Name = "Test URL 2", Url = "http://test2.com", IsActive = true });
//            await _context.SaveChangesAsync();
//            var (message, config) = await _service.GetConfigurations();
//            Assert.Equal("", message);
//            Assert.NotNull(config);
//            Assert.Equal("http://test2.com", config.ProtectedUrl);
//        }

//        [Fact]
//        public async Task GetConfigurations_SpecialCharacters_ReturnsConfig()
//        {
//            _context.ProhibitedApps.Add(new ProhibitedApp { AppId = "1", ProcessName = "!@#$%^&*()_+", AppName = "a", IsActive = true, TypeApp = (int)TypeApp.Prohibited });
//            _context.DisabledKeys.Add(new DisabledKey { KeyId = "1", KeyCode = "!@#$%^&*()_+", KeyCombination = "s", IsActive = true });
//            _context.ConfigUrls.Add(new ConfigUrl { Id = "1", Name = "Test URL", Url = "http://special.com", IsActive = true });
//            await _context.SaveChangesAsync();
//            var (message, config) = await _service.GetConfigurations();
//            Assert.Equal("", message);
//            Assert.NotNull(config);
//            Assert.Contains("!@#$%^&*()_+", config.BlockedApps);
//            Assert.Contains("!@#$%^&*()_+", config.ShortcutKeys);
//        }

//        [Fact]
//        public async Task GetConfigurations_LongProcessName_ReturnsConfig()
//        {
//            var longName = new string('a', 1000);
//            _context.ProhibitedApps.Add(new ProhibitedApp { AppId = "1", ProcessName = longName, AppName = "a", IsActive = true, TypeApp = (int)TypeApp.Prohibited });
//            _context.DisabledKeys.Add(new DisabledKey { KeyId = "1", KeyCode = "Ctrl+Alt+Del", KeyCombination = "s", IsActive = true });
//            _context.ConfigUrls.Add(new ConfigUrl { Id = "1", Name = "Test URL", Url = "http://test.com", IsActive = true });
//            await _context.SaveChangesAsync();
//            var (message, config) = await _service.GetConfigurations();
//            Assert.Equal("", message);
//            Assert.NotNull(config);
//            Assert.Contains(longName, config.BlockedApps);
//        }
//        [Fact]
//        public async Task GetConfigurations_OnlyInactiveApps_ReturnsError()
//        {
//            _context.ProhibitedApps.Add(new ProhibitedApp { AppId = "1", ProcessName = "proc1", AppName = "a", IsActive = false, TypeApp = (int)TypeApp.Prohibited });
//            _context.DisabledKeys.Add(new DisabledKey { KeyId = "1", KeyCode = "Ctrl+Alt+Del", KeyCombination = "s", IsActive = true });
//            _context.ConfigUrls.Add(new ConfigUrl { Id = "1", Name = "Test URL", Url = "http://test.com", IsActive = true });
//            await _context.SaveChangesAsync();

//            var (message, config) = await _service.GetConfigurations();

//            Assert.Equal("No prohibited applications found.", message);
//            Assert.Null(config);
//        }

//        [Fact]
//        public async Task GetConfigurations_OnlyInactiveKeys_ReturnsError()
//        {
//            _context.ProhibitedApps.Add(new ProhibitedApp { AppId = "1", ProcessName = "proc1", AppName = "a", IsActive = true, TypeApp = (int)TypeApp.Prohibited });
//            _context.DisabledKeys.Add(new DisabledKey { KeyId = "1", KeyCode = "Ctrl+Alt+Del", KeyCombination = "s", IsActive = false });
//            _context.ConfigUrls.Add(new ConfigUrl { Id = "1", Name = "Test URL", Url = "http://test.com", IsActive = true });
//            await _context.SaveChangesAsync();

//            var (message, config) = await _service.GetConfigurations();

//            Assert.Equal("No shortcut keys found.", message);
//            Assert.Null(config);
//        }

//        [Fact]
//        public async Task GetConfigurations_OnlyInactiveUrls_ReturnsError()
//        {
//            _context.ProhibitedApps.Add(new ProhibitedApp { AppId = "1", ProcessName = "proc1", AppName = "a", IsActive = true, TypeApp = (int)TypeApp.Prohibited });
//            _context.DisabledKeys.Add(new DisabledKey { KeyId = "1", KeyCode = "Ctrl+Alt+Del", KeyCombination = "s", IsActive = true });
//            _context.ConfigUrls.Add(new ConfigUrl { Id = "1", Name = "Test URL", Url = "http://test.com", IsActive = false });
//            await _context.SaveChangesAsync();

//            var (message, config) = await _service.GetConfigurations();

//            Assert.Equal("No active URL configuration found.", message);
//            Assert.Null(config);
//        }

//        [Fact]
//        public async Task GetConfigurations_MixedActiveInactive_OnlyActiveIncluded()
//        {
//            _context.ProhibitedApps.Add(new ProhibitedApp { AppId = "1", ProcessName = "proc1", AppName = "a", IsActive = true, TypeApp = (int)TypeApp.Prohibited });
//            _context.ProhibitedApps.Add(new ProhibitedApp { AppId = "2", ProcessName = "proc2", AppName = "b", IsActive = false, TypeApp = (int)TypeApp.Prohibited });
//            _context.DisabledKeys.Add(new DisabledKey { KeyId = "1", KeyCode = "Ctrl+Alt+Del", KeyCombination = "s", IsActive = true });
//            _context.DisabledKeys.Add(new DisabledKey { KeyId = "2", KeyCode = "Alt+F4", KeyCombination = "s", IsActive = false });
//            _context.ConfigUrls.Add(new ConfigUrl { Id = "1", Name = "Test URL", Url = "http://test.com", IsActive = true });
//            _context.ConfigUrls.Add(new ConfigUrl { Id = "2", Name = "Old URL", Url = "http://old.com", IsActive = false });
//            await _context.SaveChangesAsync();

//            var (message, config) = await _service.GetConfigurations();

//            Assert.Equal("", message);
//            Assert.NotNull(config);
//            Assert.Contains("proc1", config.BlockedApps);
//            Assert.DoesNotContain("proc2", config.BlockedApps);
//            Assert.Contains("Ctrl+Alt+Del", config.ShortcutKeys);
//            Assert.DoesNotContain("Alt+F4", config.ShortcutKeys);
//            Assert.Equal("http://test.com", config.ProtectedUrl);
//        }

//        public void Dispose()
//        {
//            _context.Database.EnsureDeleted();
//            _context.Dispose();
//        }
//    }
//} 