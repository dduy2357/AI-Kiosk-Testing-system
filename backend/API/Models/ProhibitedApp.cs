using API.ViewModels;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client.Framing.Impl;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    [Index(nameof(AppName))]
    [Index(nameof(ProcessName))]
    [Index(nameof(IsActive))]
    [Index(nameof(RiskLevel))]
    [Index(nameof(Category))]
    [Index(nameof(TypeApp))]
    public class ProhibitedApp
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string AppId { get; set; } = null!;
        [Required]
        [MaxLength(100)]
        public string AppName { get; set; } = null!;
        [Required]
        [MaxLength(200)]
        public string ProcessName { get; set; } = null!;
        [MaxLength(500)]    
        public string? Description { get; set; }
        public string? AppIconUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public int RiskLevel { get; set; } = 0; // 0: Low, 1: Medium, 2: High
        public int Category { get; set; }   
        public int TypeApp { get; set; } = 0;  
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedUser { get; set; }
        public string? UpdatedUser { get; set; }
    }
}
