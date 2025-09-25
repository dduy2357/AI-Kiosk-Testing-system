using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace API.Models
{
    [Index(nameof(ClassId))]
    [Index(nameof(SubjectId))]
    [Index(nameof(IsActive))]
    [Index(nameof(CreatedAt))]
    public class Room
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string RoomId { get; set; } = null!;
        [Required]
        [StringLength(36)]
        [ForeignKey("Class")]
        public string ClassId { get; set; } = null!;
        [Required]
        [StringLength(36)]
        [ForeignKey("Subject")]
        public string SubjectId { get; set; } = null!;
        [MaxLength(500)]
        public string? Description { get; set; }
        [Required, MaxLength(50)]
        public string? RoomCode { get; set; }  
        [Required]
        public int Capacity { get; set; } = 30;
        [StringLength(50)]
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Class Class { get; set; } = null!;
        public virtual Subject Subject { get; set; } = null!;
        public virtual ICollection<RoomUser> RoomUsers { get; set; } = new List<RoomUser>();
    }
}
