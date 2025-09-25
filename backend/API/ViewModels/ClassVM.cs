using ExpressiveAnnotations.Attributes;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace API.ViewModels
{
    public class ClassVM
    {
        public string ClassId { get; set; } = null!;
        public string ClassCode { get; set; } = null!;
        public string? Description { get; set; }
     //   public int MaxStudent { get; set; }
        public string CreatedBy { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class CreateUpdateClassVM
    {
        public string ClassId { get; set; } = null!;

        [Required]
        [MaxLength(20, ErrorMessage = "ClassCode cannot exceed 20 characters!")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "ClassCode must be letters and numbers only.")]
        public string ClassCode { get; set; } = null!;

        [StringLength(500, ErrorMessage = "ClassDescription cannot exceed 500 characters!")]
        public string? Description { get; set; }
    //    public int MaxStudent { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class SearchClassVM : SearchRequestVM
    {
        public DateTime? FromDate { get; set; }

        [AssertThat("ToDate >= FromDate", ErrorMessage = "To Date must be greater than or equal to From Date")]
        public DateTime? ToDate { get; set; }
        public bool? IsActive { get; set; }
    }
}
