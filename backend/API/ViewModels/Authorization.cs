using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.ComponentModel;
using API.Helper;

namespace API.ViewModels
{
    public class PermissionVM
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Action { get; set; }
        public string? Resource { get; set; }
        public string? CategoryID { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class SearchPermissionVM : SearchRequestVM
    {
        public SortType? SortType { get; set; }
    }
    public class CategoryPermissionsVM
    {
        public string? CategoryId { get; set; }
        public string? Description { get; set; }
        public List<PermissionVM> Permissions { get; set; } = new();
    }

    public class RoleSearchVM : SearchRequestVM
    {
        public int RoleId { get; set; } = 0;
    }

    public class RoleWithPermissionsVM
    {
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public List<CategoryPermissionsVM> Categories { get; set; } = new();
    }

    public class CreateUpdateCategoryPermissionsVM
    {
        public string? CategoryID { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(200, ErrorMessage = "Description can not exceed 200 characters.")]
        public string? Description { get; set; } = null!;
    }

    public class CreateUpdatePermissionVM
    {
        public string? Id { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "Name Permission can not exceed 100 characters.")]
        public string Name { get; set; } = null!;

        [Required]
        [StringLength(100, ErrorMessage = "Action Permission can not exceed 100 characters.")]
        public string Action { get; set; } = null!;

        [Required]
        [StringLength(200, ErrorMessage = "Resource Permission can not exceed 200 characters.")]
        public string Resource { get; set; } = null!;

        [Required(ErrorMessage = "CategoryID is required.")]
        public string CategoryID { get; set; } = null!;
        [StringLength(200, ErrorMessage = "Description can not exceed 200 characters.")]
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UserRoleVM
    {
        public string? UserId { get; set; } = null!;
        public List<RoleVM> Roles { get; set; } = new();
    }

    public class RoleVM
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class CreateUpdateRoleVM
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "Name Role can not exceed 100 characters.")]
        public string? Name { get; set; }
        [StringLength(200, ErrorMessage = "Description can not exceed 200 characters.")]
        public string? Description { get; set; } = null!;
        [DefaultValue(true)]
        public bool IsActive { get; set; } = true;
    }

    public class AddRoleToUserVM
    {
        [Required(ErrorMessage = "Usertoken not found! Try login again.")]
        public string? UserId { get; set; } = null!;

        [Required]
        public List<int> RoleId { get; set; } = new();
    }

    public class AddPermissionToRoleVM
    {
        [Required]
        public int RoleId { get; set; }

        [Required]
        public List<string>? Permissions { get; set; } = new();
    }

    public class AddCategoryPermissionToRoleVM
    {
        [Required]
        public int RoleId { get; set; }

        [Required]
        public string CategoryId { get; set; }  = null!;
    }
}
