using API.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    [Index(nameof(KeyId))]
    [Index(nameof(KeyCode))]
    [Index(nameof(IsActive))]
    [Index(nameof(RiskLevel))]
    public class DisabledKey
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string KeyId { get; set; } = null!;
        [Required]
        [MaxLength(100)]
        public string KeyCode { get; set; } = null!;
        public string KeyCombination { get; set; } = null!;
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        public int RiskLevel { get; set; } = 0; // 0: Low, 1: Medium, 2: High
        public bool IsActive { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedUser { get; set; } 
        public string? UpdatedUser { get; set; }  
    }
}
