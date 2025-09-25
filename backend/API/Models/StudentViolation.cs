using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Index(nameof(StudentExamId))]
    [Index(nameof(CreatedBy))]
    [Index(nameof(ViolationName))]
    public class StudentViolation
    {
        [Key, Required, StringLength(36)]
        public string Id { get; set; } = null!;

        [Required, StringLength(36), ForeignKey(nameof(StudentExam))]
        public string StudentExamId { get; set; } = null!;

        [Required, StringLength(36), ForeignKey(nameof(CreatedUser))]
        public string CreatedBy { get; set; } = null!;

        [Required, StringLength(120)]
        public string ViolationName { get; set; } = null!;

        [StringLength(500)]
        public string? Message { get; set; }

        [StringLength(500)]
        public string? ScreenshotPath { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual StudentExam? StudentExam { get; set; }
        public virtual User? CreatedUser { get; set; }  
    }
}
