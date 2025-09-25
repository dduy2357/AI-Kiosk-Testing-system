using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Index(nameof(SendToId))]
    [Index(nameof(CreatedBy))]
    [Index(nameof(Type))]
    [Index(nameof(IsRead))]
    public class Notification
    {
        [Key, Required, StringLength(36)]
        public string NotifyId { get; set; } = null!;  
        [Required, StringLength(500)]
        public string Message { get; set; } = string.Empty;
        [StringLength(36), Required, ForeignKey(nameof(User))]
        public string SendToId{ get; set; } = null!; 
        [StringLength(36), Required, ForeignKey(nameof(CreatedUser))]
        public string CreatedBy { get; set; } = null!; 
        public bool IsRead { get; set; } = false;
        public string? Type { get; set; }  
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual User? User { get; set; } 
        public virtual User? CreatedUser { get; set; }  
    }
}
