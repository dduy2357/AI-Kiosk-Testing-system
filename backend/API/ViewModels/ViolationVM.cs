using System.ComponentModel.DataAnnotations;

namespace API.ViewModels
{
    public class ViolationVM
    {
        public string Id { get; set; } = null!;
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class CreateUpdateViolationVM
    {
        public string? Id { get; set; }  
        [Required, StringLength(120)]
        public string Name { get; set; } = null!;
        [StringLength(500)]
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class SendViolationRequest
    {
        [Required] public string ExamId { get; set; } = null!;
        [Required] public string StudentId { get; set; } = null!;
        [Required] public string? Message { get; set; } = string.Empty;
        [Required] public string ViolateType { get; set; } = null!;
        public string? ScreenshotPath { get; set; } = string.Empty;
    }
}
