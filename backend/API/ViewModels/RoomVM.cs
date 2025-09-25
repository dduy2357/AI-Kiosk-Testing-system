using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.ViewModels
{
    public class RoomVM
    {
        public string RoomId { get; set; } = null!;
        public bool IsRoomActive { get; set; } = true;
        public string? RoomDescription { get; set; }
        public string RoomCode { get; set; } = null!;
        public int Capacity { get; set; } = 30;

        public string ClassId { get; set; } = null!;
        public string ClassCode { get; set; } = null!;
        public string? ClassDescription { get; set; }
    //    public int ClassMaxStudent { get; set; }
        public string ClassCreatedBy { get; set; } = null!;
        public bool IsClassActive { get; set; } = true;
        public DateTime ClassStartDate { get; set; }
        public DateTime ClassEndDate { get; set; }

        public string SubjectId { get; set; } = null!;
        public string SubjectName { get; set; } = null!;
        public string? SubjectDescription { get; set; }
        public string SubjectCode { get; set; } = null!;
        public string? SubjectContent { get; set; }
        public bool SubjectStatus { get; set; } = true;

        // Room Users
        public int TotalUsers { get; set; }
        public List<string?> RoomTeachers { get; set; } = new();

        public DateTime RoomCreatedAt { get; set; } 
        public DateTime RoomUpdatedAt { get; set; } 
    }

    public class CreateUpdateRoomVM
    {
        public string RoomId { get; set; } = null!;
        [Required] public string ClassId { get; set; } = null!;
        [Required] public string SubjectId { get; set; } = null!;
        [Required, MaxLength(50, ErrorMessage = "RoomCode cannot exceed 50 characters!")]
        public string RoomCode { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        [MaxLength(500, ErrorMessage = "RoomDescription cannot exceed 500 characters!")]
        public string? RoomDescription { get; set; }
        [Range(1, 100, ErrorMessage = "Capacity must be between 1 and 100.")]
        public int Capacity { get; set; } = 30;
    }

    public class SearchRoomVM : SearchRequestVM
    {
        public string? ClassId { get; set; }
        public string? SubjectId { get; set; }
        public bool? IsActive { get; set; }
    }
}
