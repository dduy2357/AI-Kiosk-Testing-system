using Xunit;
using Moq;
using API.Services;
using API.Models;
using API.ViewModels;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using API.Cached;
using API.Commons;

namespace API.Tests
{
    public class AuthorizeServiceTests
    {
        private readonly Sep490Context _context;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILog> _mockLog;
        private readonly Mock<IDataCached> _mockDataCached;
        private readonly AuthorizeService _service;

        public AuthorizeServiceTests()
        {
            var options = new DbContextOptionsBuilder<Sep490Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new Sep490Context(options);
            _mockMapper = new Mock<IMapper>();
            _mockLog = new Mock<ILog>();
            _mockDataCached = new Mock<IDataCached>();
            _service = new AuthorizeService(_context, _mockMapper.Object, _mockLog.Object, _mockDataCached.Object);
        }

        [Fact]
        public async Task GetAllPermissions_NoPermission_ReturnsNoPermission()
        {
            var search = new SearchPermissionVM { CurrentPage = 2, PageSize = 10, TextSearch = "" };
            var (msg, result) = await _service.GetAllPermissions(search);
            Assert.Contains("No permission found", msg);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllPermissions_WithPermission_ReturnsList()
        {
            var category = new PermissionCategory { CategoryID = "cat1", Description = "Test Category" };
            var permission = new Permission { Id = "p1", Name = "View", Action = "View", Resource = "Test", CategoryID = "cat1", Category = category };
            _context.PermissionCategories.Add(category);
            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();
            var search = new SearchPermissionVM { CurrentPage = 1, PageSize = 10, TextSearch = "" };
            _mockMapper.Setup(m => m.Map<List<PermissionVM>>(It.IsAny<List<Permission>>())).Returns(new List<PermissionVM> { new PermissionVM { Id = "p1", Name = "View" } });
            var (msg, result) = await _service.GetAllPermissions(search);
            Assert.True(string.IsNullOrEmpty(msg));
            Assert.NotNull(result);
            Assert.True(result.Total > 0);
        }

        [Fact]
        public async Task GetAllPermissions_FilterTextSearch_ReturnsFiltered()
        {
            var category = new PermissionCategory { CategoryID = "cat2", Description = "Test Category" };
            var permission = new Permission { Id = "p2", Name = "Edit", Action = "Edit", Resource = "Test", CategoryID = "cat2", Category = category };
            _context.PermissionCategories.Add(category);
            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();
            var search = new SearchPermissionVM { CurrentPage = 1, PageSize = 10, TextSearch = "edit" };
            _mockMapper.Setup(m => m.Map<List<PermissionVM>>(It.IsAny<List<Permission>>())).Returns(new List<PermissionVM> { new PermissionVM { Id = "p2", Name = "Edit" } });
            var (msg, result) = await _service.GetAllPermissions(search);
            Assert.True(string.IsNullOrEmpty(msg));
            Assert.NotNull(result);
            Assert.True(result.Total > 0);
        }

        [Fact]
        public async Task GetOnePermission_IdEmpty_ReturnsError()
        {
            var (msg, permission) = await _service.GetOnePermission("");
            Assert.Contains("cannot be empty", msg);
            Assert.Null(permission);
        }

        [Fact]
        public async Task GetOnePermission_NotFound_ReturnsError()
        {
            var (msg, permission) = await _service.GetOnePermission("notfound");
            Assert.Contains("not found", msg);
            Assert.Null(permission);
        }

        [Fact]
        public async Task GetOnePermission_Success_ReturnsPermission()
        {
            var category = new PermissionCategory { CategoryID = "cat3", Description = "Test Category" };
            var permission = new Permission { Id = "p3", Name = "Delete", Action = "Delete", Resource = "Test", CategoryID = "cat3", Category = category };
            _context.PermissionCategories.Add(category);
            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();
            _mockMapper.Setup(m => m.Map<PermissionVM>(It.IsAny<Permission>())).Returns(new PermissionVM { Id = "p3", Name = "Delete" });
            var (msg, result) = await _service.GetOnePermission("p3");
            Assert.True(string.IsNullOrEmpty(msg));
            Assert.NotNull(result);
            Assert.Equal("p3", result.Id);
        }

        [Fact]
        public async Task CreateUpdatePermission_CategoryNotFound_ReturnsError()
        {
            var input = new CreateUpdatePermissionVM { CategoryID = "notfound", Name = "Test", Action = "View", Resource = "Test" };
            var msg = await _service.CreateUpdatePermission(input, "admin");
            Assert.Contains("not valid", msg);
        }

        [Fact]
        public async Task CreateUpdatePermission_DuplicateName_ReturnsError()
        {
            var category = new PermissionCategory { CategoryID = "cat4", Description = "Test Category" };
            var permission = new Permission { Id = "p4", Name = "Duplicate", Action = "View", Resource = "Test", CategoryID = "cat4", Category = category };
            _context.PermissionCategories.Add(category);
            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();
            var input = new CreateUpdatePermissionVM { CategoryID = "cat4", Name = "Duplicate", Action = "View", Resource = "Test" };
            var msg = await _service.CreateUpdatePermission(input, "admin");
            Assert.Contains("already exists", msg);
        }

        [Fact]
        public async Task CreateUpdatePermission_CreateSuccess_ReturnsEmpty()
        {
            var category = new PermissionCategory { CategoryID = "cat5", Description = "Test Category" };
            _context.PermissionCategories.Add(category);
            await _context.SaveChangesAsync();
            var input = new CreateUpdatePermissionVM { CategoryID = "cat5", Name = "Create", Action = "Create", Resource = "Test" };
            _mockLog.Setup(l => l.WriteActivity(It.IsAny<AddUserLogVM>())).ReturnsAsync("");
            var msg = await _service.CreateUpdatePermission(input, "admin");
            Assert.True(string.IsNullOrEmpty(msg));
        }

        [Fact]
        public async Task DeletePermission_IdEmpty_ReturnsError()
        {
            var msg = await _service.DeletePermission("", "admin");
            Assert.Contains("cannot be empty", msg);
        }

        [Fact]
        public async Task DeletePermission_NotFound_ReturnsError()
        {
            var msg = await _service.DeletePermission("notfound", "admin");
            Assert.Contains("not found", msg);
        }

        [Fact]
        public async Task DeletePermission_Success_ReturnsEmpty()
        {
            var category = new PermissionCategory { CategoryID = "cat6", Description = "Test Category" };
            var permission = new Permission { Id = "p6", Name = "Del", Action = "Del", Resource = "Test", CategoryID = "cat6", Category = category };
            _context.PermissionCategories.Add(category);
            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();
            _mockLog.Setup(l => l.WriteActivity(It.IsAny<AddUserLogVM>())).ReturnsAsync("");
            var msg = await _service.DeletePermission("p6", "admin");
            Assert.True(string.IsNullOrEmpty(msg));
        }

        [Fact]
        public async Task ToggleActivePermission_IdEmpty_ReturnsError()
        {
            var msg = await _service.ToggleActivePermission("", "admin");
            Assert.Contains("cannot be empty", msg);
        }

        [Fact]
        public async Task ToggleActivePermission_NotFound_ReturnsError()
        {
            var msg = await _service.ToggleActivePermission("notfound", "admin");
            Assert.Contains("not found", msg);
        }

        [Fact]
        public async Task ToggleActivePermission_Success_TogglesStatus()
        {
            var category = new PermissionCategory { CategoryID = "cat7", Description = "Test Category" };
            var permission = new Permission { Id = "p7", Name = "Toggle", Action = "Toggle", Resource = "Test", CategoryID = "cat7", Category = category, IsActive = true };
            _context.PermissionCategories.Add(category);
            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();
            _mockLog.Setup(l => l.WriteActivity(It.IsAny<AddUserLogVM>())).ReturnsAsync("");
            var msg = await _service.ToggleActivePermission("p7", "admin");
            Assert.True(string.IsNullOrEmpty(msg));
            var updated = await _context.Permissions.FindAsync("p7");
            Assert.False(updated.IsActive);
        }

        [Fact]
        public async Task GetAllRoles_NoRole_ReturnsNoRoles()
        {
            var (msg, result) = await _service.GetAllRoles();
            Assert.Contains("No roles found", msg);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllRoles_WithRole_ReturnsList()
        {
            var role = new Role { Id = 1, Name = "Admin" };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            var (msg, result) = await _service.GetAllRoles();
            Assert.True(string.IsNullOrEmpty(msg));
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Admin", result[0].Name);
        }

        [Fact]
        public async Task CreateUpdateRole_CreateSuccess_ReturnsEmpty()
        {
            var input = new CreateUpdateRoleVM { Id = 0, Name = "Teacher", Description = "desc", IsActive = true };
            _mockLog.Setup(l => l.WriteActivity(It.IsAny<AddUserLogVM>())).ReturnsAsync("");
            var msg = await _service.CreateUpdateRole(input, "admin");
            Assert.True(string.IsNullOrEmpty(msg));
        }

        [Fact]
        public async Task CreateUpdateRole_DuplicateName_ReturnsError()
        {
            var role = new Role { Id = 2, Name = "Student" };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            var input = new CreateUpdateRoleVM { Id = 0, Name = "Student", Description = "desc", IsActive = true };
            var msg = await _service.CreateUpdateRole(input, "admin");
            Assert.Contains("already exists", msg);
        }

        [Fact]
        public async Task CreateUpdateRole_UpdateNotFound_ReturnsError()
        {
            var input = new CreateUpdateRoleVM { Id = 99, Name = "NotFound", Description = "desc", IsActive = true };
            var msg = await _service.CreateUpdateRole(input, "admin");
            Assert.Contains("not found", msg);
        }

        [Fact]
        public async Task DeleteRole_IdInvalid_ReturnsError()
        {
            var msg = await _service.DeleteRole(0, "admin");
            Assert.Contains("greater than 0", msg);
        }

        [Fact]
        public async Task DeleteRole_NotFound_ReturnsError()
        {
            var msg = await _service.DeleteRole(123, "admin");
            Assert.Contains("not found", msg);
        }

        [Fact]
        public async Task DeleteRole_Success_ReturnsEmpty()
        {
            var role = new Role { Id = 3, Name = "ToDelete" };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            _mockLog.Setup(l => l.WriteActivity(It.IsAny<AddUserLogVM>())).ReturnsAsync("");
            var msg = await _service.DeleteRole(3, "admin");
            Assert.True(string.IsNullOrEmpty(msg));
        }

        [Fact]
        public async Task GetAllCategoryPermissions_NoCategory_ReturnsNoCategory()
        {
            var search = new SearchPermissionVM { CurrentPage = 1, PageSize = 10, TextSearch = "" };
            var (msg, result) = await _service.GetAllCategoryPermissions(search);
            Assert.Contains("No category permission found", msg);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllCategoryPermissions_WithCategory_ReturnsList()
        {
            var category = new PermissionCategory { CategoryID = "catA", Description = "Category A" };
            _context.PermissionCategories.Add(category);
            await _context.SaveChangesAsync();
            var search = new SearchPermissionVM { CurrentPage = 1, PageSize = 10, TextSearch = "" };
            var (msg, result) = await _service.GetAllCategoryPermissions(search);
            Assert.True(string.IsNullOrEmpty(msg));
            Assert.NotNull(result);
            Assert.True(result.Total > 0);
        }

        [Fact]
        public async Task GetAllCategoryPermissions_FilterTextSearch_ReturnsFiltered()
        {
            var category = new PermissionCategory { CategoryID = "catB", Description = "Special Category" };
            _context.PermissionCategories.Add(category);
            await _context.SaveChangesAsync();
            var search = new SearchPermissionVM { CurrentPage = 1, PageSize = 10, TextSearch = "special" };
            var (msg, result) = await _service.GetAllCategoryPermissions(search);
            Assert.True(string.IsNullOrEmpty(msg));
            Assert.NotNull(result);
            Assert.True(result.Total > 0);
        }

        [Fact]
        public async Task GetOneCategoryPermission_IdEmpty_ReturnsError()
        {
            var (msg, result) = await _service.GetOneCategoryPermission("");
            Assert.Contains("cannot be empty", msg);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetOneCategoryPermission_NotFound_ReturnsError()
        {
            var (msg, result) = await _service.GetOneCategoryPermission("notfound");
            Assert.Contains("not found", msg);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetOneCategoryPermission_Success_ReturnsCategory()
        {
            var category = new PermissionCategory { CategoryID = "catC", Description = "Category C" };
            _context.PermissionCategories.Add(category);
            await _context.SaveChangesAsync();
            var (msg, result) = await _service.GetOneCategoryPermission("catC");
            Assert.True(string.IsNullOrEmpty(msg));
            Assert.NotNull(result);
            Assert.Equal("catC", result.CategoryId);
        }

        [Fact]
        public async Task CreateUpdateCategoryPermission_CreateSuccess_ReturnsEmpty()
        {
            var input = new CreateUpdateCategoryPermissionsVM { CategoryID = "", Description = "New Category" };
            _mockLog.Setup(l => l.WriteActivity(It.IsAny<AddUserLogVM>())).ReturnsAsync("");
            var msg = await _service.CreateUpdateCategoryPermission(input, "admin");
            Assert.True(string.IsNullOrEmpty(msg));
        }

        [Fact]
        public async Task CreateUpdateCategoryPermission_DuplicateDescription_ReturnsError()
        {
            var category = new PermissionCategory { CategoryID = "catD", Description = "Dup Category" };
            _context.PermissionCategories.Add(category);
            await _context.SaveChangesAsync();
            var input = new CreateUpdateCategoryPermissionsVM { CategoryID = "", Description = "Dup Category" };
            var msg = await _service.CreateUpdateCategoryPermission(input, "admin");
            Assert.Contains("already exists", msg);
        }

        [Fact]
        public async Task CreateUpdateCategoryPermission_UpdateNotFound_ReturnsError()
        {
            var input = new CreateUpdateCategoryPermissionsVM { CategoryID = "notfound", Description = "Update" };
            var msg = await _service.CreateUpdateCategoryPermission(input, "admin");
            Assert.Contains("not found", msg);
        }

        [Fact]
        public async Task DeleteCategoryPermission_IdEmpty_ReturnsError()
        {
            var msg = await _service.DeleteCategoryPermission("", "admin");
            Assert.Contains("cannot be empty", msg);
        }

        [Fact]
        public async Task DeleteCategoryPermission_NotFound_ReturnsError()
        {
            var msg = await _service.DeleteCategoryPermission("notfound", "admin");
            Assert.Contains("not found", msg);
        }

        [Fact]
        public async Task DeleteCategoryPermission_Success_ReturnsEmpty()
        {
            var category = new PermissionCategory { CategoryID = "catE", Description = "ToDelete" };
            _context.PermissionCategories.Add(category);
            await _context.SaveChangesAsync();
            _mockLog.Setup(l => l.WriteActivity(It.IsAny<AddUserLogVM>())).ReturnsAsync("");
            var msg = await _service.DeleteCategoryPermission("catE", "admin");
            Assert.True(string.IsNullOrEmpty(msg));
        }

        [Fact]
        public async Task AssignRolesToUser_UserNotFound_ReturnsError()
        {
            var input = new AddRoleToUserVM { UserId = "notfound", RoleId = new List<int> { 1 } };
            var msg = await _service.AssignRolesToUser(input, "admin");
            Assert.Contains("User not found", msg);
        }

        [Fact]
        public async Task AssignRolesToUser_NoValidRole_ReturnsError()
        {
            var user = new User { UserId = "u1", Email = "u1@email.com" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            var input = new AddRoleToUserVM { UserId = "u1", RoleId = new List<int> { 99 } };
            var msg = await _service.AssignRolesToUser(input, "admin");
            Assert.Contains("No valid roles found", msg);
        }

        [Fact]
        public async Task AssignRolesToUser_Success_ReturnsEmpty()
        {
            var user = new User { UserId = "u2", Email = "u2@email.com", UserCode = "U2" };
            var role = new Role { Id = 10, Name = "Role10" };
            _context.Users.Add(user);
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            var input = new AddRoleToUserVM { UserId = "u2", RoleId = new List<int> { 10 } };
            _mockLog.Setup(l => l.WriteActivity(It.IsAny<AddUserLogVM>())).ReturnsAsync("");
            var msg = await _service.AssignRolesToUser(input, "admin");
            Assert.True(string.IsNullOrEmpty(msg));
            var userRoles = await _context.UserRoles.ToListAsync();
            Assert.Single(userRoles);
            Assert.Equal("u2", userRoles[0].UserId);
            Assert.Equal(10, userRoles[0].RoleId);
        }

        [Fact]
        public async Task AddPermissionsToRole_RoleNotFound_ReturnsError()
        {
            var input = new AddPermissionToRoleVM { RoleId = 99, Permissions = new List<string> { "p1" } };
            var msg = await _service.AddPermissionsToRole(input, "admin");
            Assert.Contains("Role not found", msg);
        }

        [Fact]
        public async Task AddPermissionsToRole_PermissionEmpty_ReturnsError()
        {
            var role = new Role { Id = 11, Name = "Role11" };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            var input = new AddPermissionToRoleVM { RoleId = 11, Permissions = new List<string>() };
            var msg = await _service.AddPermissionsToRole(input, "admin");
            Assert.Contains("cannot be empty", msg);
        }

        [Fact]
        public async Task AddPermissionsToRole_Success_ReturnsEmpty()
        {
            var role = new Role { Id = 12, Name = "Role12" };
            var perm = new Permission { Id = "p12", Name = "Perm12", Action = "A", Resource = "R", CategoryID = "1" };
            _context.Roles.Add(role);
            _context.Permissions.Add(perm);
            await _context.SaveChangesAsync();
            var input = new AddPermissionToRoleVM { RoleId = 12, Permissions = new List<string> { "p12" } };
            _mockLog.Setup(l => l.WriteActivity(It.IsAny<AddUserLogVM>())).ReturnsAsync("");
            var msg = await _service.AddPermissionsToRole(input, "admin");
            Assert.True(string.IsNullOrEmpty(msg));
            var rolePerms = await _context.RolePermissions.ToListAsync();
            Assert.Single(rolePerms);
            Assert.Equal(12, rolePerms[0].RoleId);
            Assert.Equal("p12", rolePerms[0].PermissionId);
        }

        [Fact]
        public async Task GetPermissionsFromUserId_UserIdEmpty_ReturnsError()
        {
            var (msg, result) = await _service.GetPermissionsFromUserId("");
            Assert.Contains("cannot be empty", msg);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetPermissionsFromUserId_UserHasPermissions_ReturnsList()
        {
            var user = new User { UserId = "u4", Email = "u4@email.com" };
            var role = new Role { Id = 13, Name = "Role13" };
            var perm = new Permission { Id = "p13", Name = "Perm13", Action = "A", Resource = "R", CategoryID = "1" };
            _context.Users.Add(user);
            _context.Roles.Add(role);
            _context.Permissions.Add(perm);
            _context.UserRoles.Add(new UserRole { UserId = "u4", RoleId = 13 });
            _context.RolePermissions.Add(new RolePermission { RoleId = 13, PermissionId = "p13" });
            await _context.SaveChangesAsync();
            _mockMapper.Setup(m => m.Map<List<PermissionVM>>(It.IsAny<List<Permission>>())).Returns(new List<PermissionVM> { new PermissionVM { Id = "p13", Name = "Perm13" } });
            var (msg, result) = await _service.GetPermissionsFromUserId("u4");
            Assert.True(string.IsNullOrEmpty(msg));
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("p13", result[0].Id);
        }

        [Fact]
        public async Task GetUserRoles_UserIdEmpty_ReturnsError()
        {
            var (msg, result) = await _service.GetUserRoles("");
            Assert.Contains("cannot be empty", msg);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserRoles_UserHasNoRole_ReturnsError()
        {
            var user = new User { UserId = "u5", Email = "u5@email.com" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            var (msg, result) = await _service.GetUserRoles("u5");
            Assert.Contains("no roles assigned", msg);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserRoles_UserHasRoles_ReturnsList()
        {
            var user = new User { UserId = "u6", Email = "u6@email.com" };
            var role1 = new Role { Id = 14, Name = "Role14" };
            var role2 = new Role { Id = 15, Name = "Role15" };
            _context.Users.Add(user);
            _context.Roles.Add(role1);
            _context.Roles.Add(role2);
            _context.UserRoles.Add(new UserRole { UserId = "u6", RoleId = 14 });
            _context.UserRoles.Add(new UserRole { UserId = "u6", RoleId = 15 });
            await _context.SaveChangesAsync();
            var (msg, result) = await _service.GetUserRoles("u6");
            Assert.True(string.IsNullOrEmpty(msg));
            Assert.NotNull(result);
            Assert.Equal("u6", result.UserId);
            Assert.Equal(2, result.Roles.Count);
        }
    }
}