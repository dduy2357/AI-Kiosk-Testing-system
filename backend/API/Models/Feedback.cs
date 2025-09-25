using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Index(nameof(UserId))]
    [Index(nameof(Title))]
    [Index(nameof(Content))]
    [Index(nameof(CreatedAt))]
    public class Feedback
    {
        [Key, Required, StringLength(36)]
        public string Id { get; set; } = null!;

        [Required, StringLength(120)]
        public string Title { get; set; } = null!;

        [Required, StringLength(500)]
        public string Content { get; set; } = null!;

        [Required, StringLength(36), ForeignKey(nameof(User))]
        public string UserId { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual User User { get; set; } = null!;
    }
}
