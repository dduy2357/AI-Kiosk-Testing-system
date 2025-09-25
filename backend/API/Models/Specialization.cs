using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class Specialization
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string Id { get; set; } = null!;
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;
        [StringLength(500)]
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        public bool Status { get; set; } = true;
    }
}
