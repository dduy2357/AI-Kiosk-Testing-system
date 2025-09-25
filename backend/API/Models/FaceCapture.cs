using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Index(nameof(StudentExamId))]
    [Index(nameof(ImageUrl))]
    [Index(nameof(DominantEmotion))]
    [Index(nameof(LogType))]
    public class FaceCapture
    {
        [Key, Required, StringLength(36)]
        public string CaptureId { get; set; } = null!;
        [Required, StringLength(36), ForeignKey(nameof(StudentExam))]
        public string StudentExamId { get; set; } = null!;
        [Required, StringLength(255)]
        public string ImageUrl { get; set; } = null!;
        [StringLength(500)]
        public string? Description { get; set; } = null;
        public int LogType { get; set; } = 0;
        public string? Emotions { get; set; } = null;
        public string? DominantEmotion { get; set; } = null;
        public float AvgArousal { get; set; } = 0;
        public float AvgValence { get; set; } = 0;
        public string? InferredState { get; set; }
        public string? Region { get; set; } = null;
        public string? Result { get; set; } = null;
        public string? Status { get; set; } = null;
        public bool IsDetected { get; set; } = false; 
        public string? ErrorMessage { get; set; }

        public virtual StudentExam? StudentExam { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
