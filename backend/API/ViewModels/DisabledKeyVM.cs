using API.Helper;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace API.ViewModels
{
    public class DisabledKeyVM
    {
        public string KeyId { get; set; } = null!;
        public string KeyCode { get; set; } = null!;
        public string KeyCombination { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public int RiskLevel { get; set; } = 0; // 0: Low, 1: Medium, 2: High
        public bool IsActive { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedUser { get; set; }  
        public string? UpdatedUser { get; set; } 
    }

    public class CreateUpdateDisabledKeyVM
    { 
        public string? KeyId { get; set; } 
        [Required]
        [MaxLength(20, ErrorMessage = "KeyCode cannot exceed 20 characters!")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "KeyCode must be alphanumeric!")]
        public string KeyCode { get; set; } = null!;
        [MaxLength(50, ErrorMessage = "KeyCombination cannot exceed 50 characters!")]
        public string KeyCombination { get; set; } = null!;
        [MaxLength(255, ErrorMessage = "Description cannot exceed 255 characters!")]
        public string Description { get; set; } = string.Empty;
        [Required(ErrorMessage = "RiskLevel is required!")]
        public RiskLevel RiskLevel { get; set; } = 0;
        [Required(ErrorMessage = "IsActive is required!")]
        public bool IsActive { get; set; } = false;
    }

    public class DisabledKeySearchVM : SearchRequestVM
    {
        public bool? IsActive { get; set; }
        [FromQuery(Name = "RiskLevel")]
        public RiskLevel? RiskLevel { get; set; }
    }

    public class ListKeyActiveVM
    {
        public string ShortcutKeys { get; set; } = null!;
    }
}
