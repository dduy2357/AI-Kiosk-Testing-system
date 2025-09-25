using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    public class Position
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string Id { get; set; } = null!;
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? Code { get; set; }

        public string? Description { get; set; }

        [StringLength(36), ForeignKey(nameof(Department))]
        public string? DepartmentId { get; set; } = null!;

        // Navigation property
        public virtual Department? Department { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdateAt { get; set; } = DateTime.UtcNow;
    }
}
