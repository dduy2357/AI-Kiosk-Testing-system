
using API.Attributes;
using API.Helper;
using ExpressiveAnnotations.Attributes;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.ViewModels
{
    public class UserVM
    {
        public string UserId { get; set; } = null!;
        public string? UserCode { get; set; }
        public string? Fullname { get; set; }
        public string? Email { get; set; }
        public string? Avatar { get; set; }
        public int? Sex { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
    public class SearchUserVM : SearchRequestVM
    {
        public RoleEnum? RoleId { get; set; } 
        public UserStatus? Status { get; set; }  
        public string? CampusId { get; set; } 
        public SortType? SortType { get; set; }
    }

    public class BaseUserVM
    {
        public string UserId { get; set; } = null!;
        public string? FullName { get; set; }
        public string? UserCode { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
        public int? Sex { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public string? CreateUser { get; set; }
        public string? UpdateUser { get; set; }
        public int? Status { get; set; }
        public DateTime? LastLogin { get; set; }
        public string? LastLoginIp { get; set; }
        public DateTime Dob { get; set; }
        public string? Address { get; set; }
        public List<int>? RoleId { get; set; } = new List<int>();
    }
    public class UserListVM : BaseUserVM
    {
        public string? Campus { get; set; }
        public string? Department { get; set; }
        public string? Position { get; set; }
        public string? Major { get; set; }
        public string? Specialization { get; set; }
    }
    public class UserDetailVM : BaseUserVM
    {
        public string? CampusId { get; set; }
        public string? DepartmentId { get; set; }
        public string? PositionId { get; set; }
        public string? MajorId { get; set; }
        public string? SpecializationId { get; set; }
    }

    public class UserImportVM
    {
        public string? FullName { get; set; }
        public string? UserCode { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public int? Sex { get; set; }
        public int? Status { get; set; }
        public DateTime Dob { get; set; }
        public string? Address { get; set; }
        public string? CampusId { get; set; }
        public string? DepartmentId { get; set; }
        public string? PositionId { get; set; }
        public string? MajorId { get; set; }
        public string? SpecializationId { get; set; }
        public List<int>? RoleId { get; set; } = new List<int>();
    }

    public abstract class UserBaseVM
    {
        [Required(ErrorMessage = "Fullname cannot be empty")]
        [StringLength(50, ErrorMessage = "Fullname must not exceed 50 characters")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Phone cannot be empty")]
        [StringLength(10, ErrorMessage = "Phone number must not exceed 10 characters")]
        [RegularExpression(@"^\+?[0-9\s]+$", ErrorMessage = "Invalid phone number format")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "UserCode cannot be empty")]
        [StringLength(50, ErrorMessage = "UserCode must not exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "UserCode must not contain special characters")]
        public string UserCode { get; set; } = null!;

        [Required(ErrorMessage = "Gender is required")]
        public int Sex { get; set; }

        [Required(ErrorMessage = "RoleId cannot be empty")]
        public List<int>? RoleId { get; set; }

        [JsonIgnore]
        public DateTime MinAdultDob => DateTime.Today.AddYears(-18);

        [Required(ErrorMessage = "Dob cannot be empty")]
        [AssertThat("Dob <= MinAdultDob", ErrorMessage = "Date of birth must be at least 18 years old.")]
        public DateTime Dob { get; set; }

        [Required(ErrorMessage = "Address cannot be empty")]
        [StringLength(200, ErrorMessage = "Address must not exceed 200 characters")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "A campus is required for Teacher, Supervisor, or Admin roles.")]
        public string? CampusId { get; set; }
        public string? DepartmentId { get; set; }
        public string? PositionId { get; set; }
        public string? MajorId { get; set; }
        public string? SpecializationId { get; set; }
        public int? Status { get; set; }
    }

    public class AddListUserVM : UserBaseVM
    {
        [Required(ErrorMessage = "Email cannot be empty")]
        [StringLength(100, ErrorMessage = "Email must be not exceed 100 characters")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string? Email { get; set; }
    }

    public class CreateUserVM : UserBaseVM
    {
        [Required(ErrorMessage = "Email cannot be empty")]
        [StringLength(100, ErrorMessage = "Email must be not exceed 100 characters")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string? Email { get; set; }
        [AllowedExtensions(new[] { ".png", ".jpg", ".jpeg", ".svg" })]
        [MaxFileSize(Constant.IMAGE_FILE_SIZE)]
        public IFormFile Avatar { get; set; } = null!;
    }

    public class UpdateUserVM : UserBaseVM
    {
        [Required(ErrorMessage = "UserId cannot be empty")]
        public string UserId { get; set; } = null!;
        [AllowedExtensions(new[] { ".png", ".jpg", ".jpeg", ".svg" })]
        [MaxFileSize(Constant.IMAGE_FILE_SIZE)]
        public IFormFile Avatar { get; set; } = null!;
    }

    public class ErrorImport
    {
        public int Row { get; set; }
        public UserImportVM? User { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class ChangeAvatarVM
    {
        [Required(ErrorMessage = "Avatar cannot be empty")]
        [AllowedExtensions(new[] { ".png", ".jpg", ".jpeg", ".svg" })]
        [MaxFileSize(Constant.IMAGE_FILE_SIZE)]
        public IFormFile Avatar { get; set; } = null!;
        [Required(ErrorMessage = "UserId cannot be empty")]
        public string UserId { get; set; } = null!; 
    }
}
