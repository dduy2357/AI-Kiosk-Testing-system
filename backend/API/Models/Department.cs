using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class Department
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string? Id { get; set; }
        [Required]
        [StringLength(100)]
        public string? Name { get; set; }
        [Required]
        [StringLength(100)]
        public string? Code { get; set; }
        [StringLength(255)]
        public string? Description { get; set; }
        
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdateAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Position> Positions { get; set; } = new List<Position>();
    }

}
