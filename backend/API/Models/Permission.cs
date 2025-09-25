using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class Permission
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string? Id { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string? Name { get; set; }

        [Required]
        [StringLength(100)]
        public string? Action { get; set; }

        [Required]
        [StringLength(200)]
        public string? Resource { get; set; }

        [StringLength(200)]
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key
        [Required]
        [StringLength(36)]
        [ForeignKey(nameof(Category))]
        public string CategoryID { get; set; } = null!;

        // Navigation properties
        public virtual PermissionCategory? Category { get; set; }
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
