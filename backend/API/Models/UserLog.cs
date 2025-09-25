using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace API.Models
{
    [Index(nameof(UserId))]
    [Index(nameof(ActionType))]
    [Index(nameof(ObjectId))]
    [Index(nameof(CreatedAt))]
    [Index(nameof(Status))]
    [Index(nameof(Description))]
    public class UserLog
    {
        [Key, StringLength(36)]
        public string LogId { get; set; } = null!;

        [Required, StringLength(36), ForeignKey(nameof(User))]
        public string UserId { get; set; } = null!;

        [Required, StringLength(100)]
        public string? ActionType { get; set; }   // LOGIN, LOGOUT, CREATE_EXAM, START_EXAM, SUBMIT_EXAM, VIEW_QUESTION, ANSWER_QUESTION, etc.
        [StringLength(36)] public string? ObjectId { get; set; }
        [StringLength(50)] public string? IpAddress { get; set; }
        [StringLength(255)] public string? BrowserInfo { get; set; }
        public string? Description { get; set; }
        public int Status { get; set; } //-- SUCCESS, FAILED, WARNING
        public string? Metadata { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual User? User { get; set; }
    }
}
