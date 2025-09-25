using API.Attributes;
using API.Helper;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace API.ViewModels
{
    public class StudentViolationVM
    {
        public string ViolationId { get; set; } = null!;
        public string? CreatorName { get; set; }  
        public string? CreatorEmail { get; set; }  
        public string? CreatorCode{ get; set; }  
        public string? StudentName { get; set; } = string.Empty;
        public string? StudentCode { get; set; } = string.Empty;
        public string? StudentEmail { get; set; } = string.Empty;
        public string StudentExamId { get; set; } = null!;

        public string? ViolationName { get; set; } = string.Empty;
        public string? Message { get; set; } = string.Empty;
        public string? ScreenshotPath { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class SendStudentViolationVM
    {
        [Required] public string StudentExamId { get; set; } = null!;
        
        [Required, MaxLength(120, ErrorMessage = "Violation name cannot exceed 120 characters.")]
        public string ViolateName { get; set; } = null!;
        [MaxLength(500, ErrorMessage = "Message cannot exceed 500 characters.")]
        public string? Message { get; set; } = string.Empty;

        [AllowedExtensions(new[] { ".png", ".jpg", ".jpeg", ".svg" })]
        [MaxFileSize(Constant.IMAGE_FILE_SIZE)]
        public IFormFile? ScreenshotPath { get; set; } 
        [DefaultValue(true)] public bool IsSendMail { get; set; } = true;
    }

    public class SearchStudentViolation : SearchRequestVM
    {
        [Required] public string ExamId { get; set; } = null!;
        public string? StudentExamId { get; set; }  

    }
}
