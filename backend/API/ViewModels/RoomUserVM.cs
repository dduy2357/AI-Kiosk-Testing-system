using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using API.Models;
using API.Helper;
using Microsoft.AspNetCore.Mvc;

namespace API.ViewModels
{
    public class RoomWithUserVM
    {
        public string RoomId { get; set; } = null!;
        public string? RoomCode { get; set; }
        public string? Description { get; set; }
        public bool IsRoomActive { get; set; }
        public List<UserInRoomVM> Users { get; set; } = new();
    }

    public class UserInRoomVM
    {
        public string RoomId { get; set; } = null!;
        public string RoomUserId { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public int Role { get; set; }
        public int UserStatus { get; set; }
        public DateTime JoinTime { get; set; }
        public DateTime UpdatedAt { get; set; }
        public UserVM User { get; set; } = null!;
    }

    public class UpdateRoomUserVM
    {
        [Required] public string? RoomUserId { get; set; }
        [Required] public string RoomId { get; set; } = null!;
        [Required] public string UserId { get; set; } = null!;
        [Required] public RoleEnum RoleId { get; set; } // 0: Student, 1: Teacher
        [Required] public UserStatus Status { get; set; } // 0: Inactive, 1: Active, 2: Pending
    }

    public class SearchRoomUserVM : SearchRequestVM
    {
        [Required] public string? RoomId { get; set; }
        [FromQuery(Name = "Role")] // <- đảm bảo lấy đúng từ query string
        public List<RoleEnum>? Role { get; set; } // 0: Student, 1: Teacher
        public UserStatus? Status { get; set; } // 0: Inactive, 1: Active, 2: Pending
    }

    public class UpdateUserInRoomVM
    {
        [Required] public string RoomId { get; set; } = null!;
        [Required] public string UserId { get; set; } = null!;
        [Required] public int Role { get; set; } // 0: Student, 1: Teacher
        [Required] public bool IsToggleStatus { get; set; }
    }

    public class ExportUserRoom
    {
        public string RoomId { get; set; } = null!;
        public string RoomCode { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string? UserCode { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Avatar { get; set; }
        public int? Sex { get; set; }
        public int Status { get; set; }
        public string? SubjectName { get; set; }
        public string? ClassName { get; set; }
    }

    public class RoomUserVM
    {
        public string UserId { get; set; } = null!;
        public string? UserCode { get; set; }
        public string? FullName { get; set; }
        public string? Major { get; set; }
        public int RoleId { get; set; } 
    }

    public class ToggleActiveRoomUserVM
    {
        [Required] public string RoomId { get; set; } = null!;
        [Required] public List<string> StudentId { get; set; } = null!;
        [Required] public UserStatus Status { get; set; }
    }

    public class ImportRoomUser
    {
        public string? UserCode { get; set; }
    }

    public class SearchUserRoomExamVM : SearchRequestVM
    {
        [Required] public string RoomId { get; set; } = null!;
    }
}
