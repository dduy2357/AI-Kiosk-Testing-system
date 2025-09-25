using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    [Index(nameof(CategoryID), IsUnique = true)]
    public class PermissionCategory
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string? CategoryID { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string? Description { get; set; }
    }
}
