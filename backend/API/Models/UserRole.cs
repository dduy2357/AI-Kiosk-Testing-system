using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class UserRole
    {
        [Key, Column(Order = 0)]
        [StringLength(36)]
        [ForeignKey("User")]
        public string UserId { get; set; } = null!;

        [Key, Column(Order = 1)]
        [ForeignKey("Role")]
        public int RoleId { get; set; } 

        // Navigation properties
        public virtual User? User { get; set; }
        public virtual Role? Role { get; set; }
    }
}
