using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    [Index(nameof(Name))]
    [Index(nameof(Url), IsUnique = true)]
    public class ConfigUrl
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string Id { get; set; } = null!;
        [Required, StringLength(100)]
        public string? Name { get; set; }
        [Required, StringLength(200)]
        public string? Url { get; set; }
        public string? Version { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
