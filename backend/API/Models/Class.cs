using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace API.Models
{
    [Table("Classes")]
    [Index(nameof(ClassCode), IsUnique = true)]
    [Index(nameof(IsActive))]
    public class Class
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string ClassId { get; set; } = null!;
        [Required]
        [MaxLength(100)]
        public string ClassCode { get; set; } = null!;
        [MaxLength(500)]
        public string? Description { get; set; }
        [Required]
        [StringLength(36)]
        public string CreatedBy { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}
