using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace API.Models
{
    [Index(nameof(RoomId))]
    [Index(nameof(UserId))]
    [Index(nameof(RoleId))]
    [Index(nameof(Status))]
    [Index(nameof(RoomId), nameof(UserId), IsUnique = true)]
    public class RoomUser
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string RoomUserId { get; set; } = null!;

        [Required]
        [StringLength(36)]
        [ForeignKey("Room")]
        public string RoomId { get; set; } = null!;

        [Required]
        [StringLength(36)]
        [ForeignKey("User")]
        public string UserId { get; set; } = null!;
        [ForeignKey("Role")]
        public int RoleId { get; set; }
        public int Status { get; set; } // 0: Inactive, 1: Active, 2: Pending
        public DateTime JoinTime { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Room? Room { get; set; }  
        public virtual User? User { get; set; }
        public virtual Role? Role { get; set; }

    }
}
