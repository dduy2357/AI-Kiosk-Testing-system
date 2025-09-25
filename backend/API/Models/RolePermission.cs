using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class RolePermission
    {
        [Key, Column(Order = 0)]
        [Required]
        [ForeignKey("Role")]
        public int RoleId { get; set; }

        [Key, Column(Order = 1)]
        [Required]
        [ForeignKey("Permission")]
        [StringLength(36)]  
        public string PermissionId { get; set; } = null!; 

        // Navigation properties
        public virtual Role? Role { get; set; }
        public virtual Permission? Permission { get; set; }
    }
}
