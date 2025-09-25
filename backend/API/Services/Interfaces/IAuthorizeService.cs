using API.ViewModels;

namespace API.Services.Interfaces
{
    public interface IAuthorizeService
    {
        public Task<(string, SearchResult?)> GetAllPermissions(SearchPermissionVM search);
        public Task<(string, List<PermissionVM>)> GetAllPermissions();
        public Task<(string, PermissionVM?)> GetOnePermission(string permissionId);
        public Task<string> CreateUpdatePermission(CreateUpdatePermissionVM permission, string usertoken);
        public Task<string> DeletePermission(string permissionId, string usertoken);
        public Task<string> ToggleActivePermission(string permissionId, string usertoken);
        public Task<(string, List<RoleVM>?)> GetAllRoles();
        public Task<(string, SearchResult)> GetAllRolesPermissions(RoleSearchVM search);
        public Task<string> CreateUpdateRole(CreateUpdateRoleVM role, string usertoken);
        public Task<string> DeleteRole(int roleId, string usertoken);
        public Task<string> ToggleActive(int roleId, string usertoken);
        public Task<string> AssignRolesToUser(AddRoleToUserVM userRole, string usertoken);
        public Task<string> AddPermissionsToRole(AddPermissionToRoleVM rolePermission, string usertoken);
        public Task<string> RemovePermissionsFromRole(AddPermissionToRoleVM rolePermission, string usertoken);
        public Task<string> AddCategoryPermissionsToRole(AddCategoryPermissionToRoleVM permission, string userToken);
        public Task<(string, UserRoleVM?)> GetUserRoles(string userId);
        public Task<(string, List<PermissionVM>?)> GetPermissionsFromUserId(string? userId);
        public Task<(string, SearchResult?)> GetAllCategoryPermissions(SearchRequestVM search);
        public Task<(string, CategoryPermissionsVM?)> GetOneCategoryPermission(string categoryId);
        public Task<string> CreateUpdateCategoryPermission(CreateUpdateCategoryPermissionsVM category, string usertoken);
        public Task<string> DeleteCategoryPermission(string categoryId, string usertoken);
    }
}
