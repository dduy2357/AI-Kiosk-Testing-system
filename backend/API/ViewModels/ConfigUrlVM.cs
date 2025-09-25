using System.ComponentModel.DataAnnotations;

namespace API.ViewModels
{
    public class ConfigUrlVM
    {
        public string Id { get; set; } = null!;
        public string? Name { get; set; }
        public string? Url { get; set; }
        public string? Version { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class CreateUpdateConfigUrlVM
    {
        public string? Id { get; set; }
        [Required, StringLength(100)]
        public string? Name { get; set; }
        [Required, StringLength(200)]
        public string? Url { get; set; }
        public string? Version { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }


}
