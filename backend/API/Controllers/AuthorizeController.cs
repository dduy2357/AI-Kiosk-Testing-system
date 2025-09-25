using API.Attributes;
using API.Services.Interfaces;
using API.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AuthenPermission]
    public class AuthorizeController : Authentication.Authentication
    {
        private readonly IAuthorizeService _permissionService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AuthorizeController(IAuthorizeService permissionService, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _permissionService = permissionService;
        }

        [HttpGet("get-all-permission")]
        public async Task<IActionResult> GetAllPermissions([FromQuery] SearchPermissionVM search)
        {
            var (message, result) = await _permissionService.GetAllPermissions(search);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Lấy permissions thành công.", data = result });
        }

        [HttpGet("get-one-permission/{permissionId}")]
        public async Task<IActionResult> GetOnePermission(string permissionId)
        {
            var (message, permission) = await _permissionService.GetOnePermission(permissionId);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Lấy permission thành công.", data = permission });
        }

        [HttpPost("create-update-permission")]
        public async Task<IActionResult> CreateUpdatePermission([FromBody] CreateUpdatePermissionVM permission)
        {
            string message = await _permissionService.CreateUpdatePermission(permission, UserToken.UserID);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Tạo hoặc cập nhật permission thành công." });
        }

        [HttpDelete("delete-permission/{permissionId}")]
        public async Task<IActionResult> DeletePermission(string permissionId)
        {
            string message = await _permissionService.DeletePermission(permissionId, UserToken.UserID);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Xóa permission thành công." });
        }

        [HttpPost("toggle-active-permission/{permissionId}")]
        public async Task<IActionResult> ToggleActivePermission(string permissionId)
        {
            string message = await _permissionService.ToggleActivePermission(permissionId, UserToken.UserID);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Kích hoạt hoặc vô hiệu hóa permission thành công." });
        }

        [HttpGet("get-all-roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var (message, roles) = await _permissionService.GetAllRoles();
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Lấy danh sách roles thành công.", data = roles });
        }

        [HttpGet("get-all-roles-permissions")]
        public async Task<IActionResult> GetAllRolesPermissions([FromQuery] RoleSearchVM search)
        {
            var (message, result) = await _permissionService.GetAllRolesPermissions(search);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Lấy danh sách roles cùng permissions thành công.", data = result });
        }

        [HttpPost("create-update-role")]
        public async Task<IActionResult> CreateUpdateRole([FromBody] CreateUpdateRoleVM role)
        {
            string message = await _permissionService.CreateUpdateRole(role, UserToken.UserID);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Tạo hoặc cập nhật role thành công." });
        }

        [HttpDelete("delete-role/{roleId}")]
        public async Task<IActionResult> DeleteRole(int roleId)
        {
            string message = await _permissionService.DeleteRole(roleId, UserToken.UserID);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Xóa role thành công." });
        }

        [HttpPost("toggle-active-role/{roleId}")]
        public async Task<IActionResult> ToggleActiveRole(int roleId)
        {
            string message = await _permissionService.ToggleActive(roleId, UserToken.UserID);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Kích hoạt hoặc vô hiệu hóa role thành công." });
        }

        [HttpPost("assign-roles-to-user")]
        public async Task<IActionResult> AssignRolesToUser([FromBody] AddRoleToUserVM userRole)
        {
            string message = await _permissionService.AssignRolesToUser(userRole, UserToken.UserID);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Gán roles cho người dùng thành công." });
        }

        [HttpPost("add-permissions-to-role")]
        public async Task<IActionResult> AddPermissionsToRole([FromBody] AddPermissionToRoleVM rolePermission)
        {
            string message = await _permissionService.AddPermissionsToRole(rolePermission, UserToken.UserID);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Thêm permissions vào role thành công." });
        }

        [HttpDelete("remove-permissions-from-role")]
        public async Task<IActionResult> RemovePermissionsFromRole([FromBody] AddPermissionToRoleVM rolePermission)
        {
            string message = await _permissionService.RemovePermissionsFromRole(rolePermission, UserToken.UserID);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Xóa permissions khỏi role thành công." });
        }

        [HttpPost("add-category-permissions-to-role")]
        public async Task<IActionResult> AddCategoryPermissionsToRole([FromBody] AddCategoryPermissionToRoleVM permission)
        {
            string message = await _permissionService.AddCategoryPermissionsToRole(permission, UserToken.UserID);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Thêm category permissions vào role thành công." });
        }

        [HttpGet("get-user-roles/{userId}")]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            var (message, userRoles) = await _permissionService.GetUserRoles(userId);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Lấy roles của người dùng thành công.", data = userRoles });
        }

        [HttpGet("get-permissions-from-user/{userId}")]
        public async Task<IActionResult> GetPermissionsFromUserId(string userId)
        {
            var (message, permissions) = await _permissionService.GetPermissionsFromUserId(userId);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Lấy permissions từ user thành công.", data = permissions });
        }

        [HttpGet("get-all-category-permissions")]
        public async Task<IActionResult> GetAllCategoryPermissions([FromQuery] SearchRequestVM search)
        {
            var (message, result) = await _permissionService.GetAllCategoryPermissions(search);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Lấy danh sách category permissions thành công.", data = result });
        }

        [HttpGet("get-one-category-permission/{categoryId}")]
        public async Task<IActionResult> GetOneCategoryPermission(string categoryId)
        {
            var (message, category) = await _permissionService.GetOneCategoryPermission(categoryId);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Lấy category permission thành công.", data = category });
        }

        [HttpPost("create-update-category-permission")]
        public async Task<IActionResult> CreateUpdateCategoryPermission([FromBody] CreateUpdateCategoryPermissionsVM category)
        {
            string message = await _permissionService.CreateUpdateCategoryPermission(category, UserToken.UserID);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Tạo hoặc cập nhật category permission thành công." });
        }

        [HttpDelete("delete-category-permission/{categoryId}")]
        public async Task<IActionResult> DeleteCategoryPermission(string categoryId)
        {
            string message = await _permissionService.DeleteCategoryPermission(categoryId, UserToken.UserID);
            if (message.Length > 0)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Xóa category permission thành công." });
        }


    }
}
