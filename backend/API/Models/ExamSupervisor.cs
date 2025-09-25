using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace API.Models
{
    [Index(nameof(SupervisorId))]
    [Index(nameof(ExamId))]
    [Index(nameof(CreatedBy))]
    public class ExamSupervisor
    {
        [Key, Required, StringLength(36)]
        public string ExamSupervisorId { get; set; } = null!;

        [StringLength(36)]
        public string? SupervisorId { get; set; }

        [Required, StringLength(36)]
        public string ExamId { get; set; } = null!;

        [Required, StringLength(36)]
        public string CreatedBy { get; set; } = null!;
        public string? Note { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(SupervisorId))]
        public virtual User? User { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public virtual User? CreatedUser { get; set; }

        [ForeignKey(nameof(ExamId))]
        public virtual Exam? Exam { get; set; }
    }
}
