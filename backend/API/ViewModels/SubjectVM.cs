using System.ComponentModel.DataAnnotations;

namespace API.ViewModels
{
    public class SubjectVM
    {
        public string SubjectId { get; set; } = null!;
        public string SubjectCode { get; set; } = null!;
        public string? SubjectName { get; set; }
        public string? SubjectDescription { get; set; }
        public bool Status { get; set; }
        public int Credits { get; set; }
        public DateTime CreatedAt { get; set; } 
        public DateTime UpdatedAt { get; set; }  
    }

    public class CreateUpdateSubjectVM
    {
        public string? SubjectId { get; set; } 

        [Required]
        [MaxLength(500, ErrorMessage = "SubjectName cannot exceed 500 characters!")]
        public string SubjectName { get; set; } = null!;

        [StringLength(500, ErrorMessage = "SubjectDescription cannot exceed 500 characters!")]
        public string? SubjectDescription { get; set; }

        [Required]
        [MaxLength(20, ErrorMessage = "SubjectCode cannot exceed 20 characters!")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "SubjectCode must be letters and numbers only.")]
        public string SubjectCode { get; set; } = null!;
        public string? SubjectContent { get; set; }
        public bool Status { get; set; } = true;
        [Range(0, 100, ErrorMessage = "Credits must be positive integer between 0 and 100.")]
        public int Credits { get; set; } = 0;
    }

    public class SearchSubjectVM : SearchRequestVM
    {
        public bool? Status { get; set; }
    }
}
