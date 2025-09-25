using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class Campus
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string Id { get; set; } = null!;
        [Required]
        public string? Code { get; set; }
        [Required]
        public string? Name { get; set; }
        public string? Description { get; set; }

        public DateTime CreateAt {  get; set; }
        public DateTime UpdateAt { get; set; } = DateTime.UtcNow;
    }
}
