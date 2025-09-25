using API.Helper;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace API.ViewModels
{
    public class ProhibitedAppVM
    {
        public string AppId { get; set; } = null!;
        public string AppName { get; set; } = null!;
        public string ProcessName { get; set; } = null!;
        public string? Description { get; set; }
        public string? AppIconUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public int RiskLevel { get; set; } = 0; // 0: Low, 1: Medium, 2: High
        public int Category { get; set; }
        public int TypeApp { get; set; } = 0;  
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class CreateUpdateProhibitedAppVM
    {
        public string? AppId { get; set; } 
        [Required]
        [MaxLength(100, ErrorMessage = "AppName cannot exceed 100 characters!")]
        public string AppName { get; set; } = null!;
        [Required]
        [MaxLength(200, ErrorMessage = "ProcessName cannot exceed 200 characters!")]
        public string ProcessName { get; set; } = null!;
        public string? Description { get; set; }
        public string? AppIconUrl { get; set; }
        [Required(ErrorMessage = "IsActive is required!")]
        public bool IsActive { get; set; } = true;
        [Required(ErrorMessage = "RiskLevel is required!")]
        public RiskLevel RiskLevel { get; set; } = 0;
        [Required(ErrorMessage = "Category is required!")]
        public CategoryApp Category { get; set; }
        [Required(ErrorMessage = "TypeApp is required!")]
        public TypeApp TypeApp { get; set; } = 0;  // Default type of application
    }

    public class ProhibitedAppSearchVM : SearchRequestVM
    {
        public bool? IsActive { get; set; }
        [FromQuery(Name = "RiskLevel")]
        public RiskLevel? RiskLevel { get; set; }   // 0: Low, 1: Medium, 2: High
        [FromQuery(Name = "Category")]
        public CategoryApp? Category { get; set; }  // Default category
        [FromQuery(Name = "TypeApp")]
        public TypeApp? TypeApp { get; set; }  // Default type of application
    }
}
