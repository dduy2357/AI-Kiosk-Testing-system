using System.ComponentModel.DataAnnotations;

namespace API.ViewModels
{
    public class ExamSupervisorVM
    {
        public string ExamId { get; set; } = null!;
        public string ExamTitle { get; set; } = null!;
        public string SubjectName { get; set; } = null!;
        public string ClassCode { get; set; } = null!;
        public string RoomCode { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<SupervisorVM> SupervisorVMs { get; set; } = new();
    }

    public class SupervisorVM
    {
        public string UserId { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public string? Department { get; set; }
        public string? Major { get; set; }
        public string? Specialization { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string UserCode { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public DateTime AssignAt { get; set; }
    }

    public class EditExamSupervisorVM
    {
        [Required] public List<string> SupervisorId { get; set; } = new();
        [Required] public string ExamId { get; set; } = null!;
        public string? Note { get; set; }  
    }
}
