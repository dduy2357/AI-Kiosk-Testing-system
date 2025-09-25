using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace API.Models
{
    [Index(nameof(StudentExamId))]
    [Index(nameof(UserId))]
    [Index(nameof(ExamLogId), IsUnique = true)]
    public class ExamLog
    {
        [Key, StringLength(36)]
        public string ExamLogId { get; set; } = null!;

        [Required, StringLength(36), ForeignKey(nameof(StudentExam))]
        public string StudentExamId { get; set; } = null!;

        [Required, StringLength(36), ForeignKey(nameof(User))]
        public string UserId { get; set; } = null!;
        [Required] public string? ActionType { get; set; } // e.g., START_EXAM, SUBMIT_EXAM, VIEW_QUESTION, ANSWER_QUESTION, etc.
        [StringLength(500)] public string? Description { get; set; }
        [StringLength(50)] public string? IpAddress { get; set; }
        [StringLength(255)] public string? BrowserInfo { get; set; }
        [StringLength(255)] public string? DeviceId { get; set; }
        [StringLength(255)] public string? DeviceUsername { get; set; }
        [StringLength(500)] public string? ScreenshotPath { get; set; }
        public int LogType { get; set; }  // 0: Info, 1: Warning, 2: Error
        public string? MetaData { get; set; }  
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual StudentExam? StudentExam { get; set; }
        public virtual User? User { get; set; }
    }
}
