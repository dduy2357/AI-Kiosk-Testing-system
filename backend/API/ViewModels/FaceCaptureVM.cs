using System.ComponentModel.DataAnnotations;
using API.Helper;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using API.Attributes;

namespace API.ViewModels
{
    public class FaceCaptureVM
    {
        public string UserId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string UserCode { get; set; } = null!;
        public string ExamName { get; set; } = null!;
        public string StudentExamId { get; set; } = null!;
        public List<CaptureImages> Captures { get; set; } = new List<CaptureImages>();

        public class CaptureImages
        {
            public string CaptureId { get; set; } = null!;
            public string ImageUrl { get; set; } = null!;
            public string? Description { get; set; } = null;
            public int LogType { get; set; } = 0;
            public string? Emotions { get; set; } = null;
            public string? DominantEmotion { get; set; } = null;
            public float AvgArousal { get; set; } = 0;
            public float AvgValence { get; set; } = 0;
            public string? InferredState { get; set; }
            public string? Region { get; set; }
            public string? Result { get; set; } = null; // Result of the face detection, e.g., "Detected", "Not Detected"
            public string? Status { get; set; } = null;
            public bool IsDetected { get; set; } = false;  
            public string? ErrorMessage { get; set; } = null;
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }
    }

    public class FaceCaptureRequest
    {
        [Required]
        public string StudentExamId { get; set; } = null!;
        [Required]
        [AllowedExtensions(new[] { ".png", ".jpg", ".jpeg", ".svg" })]
        [MaxFileSize(Constant.IMAGE_FILE_SIZE)]
        public IFormFile ImageCapture { get; set; } = null!;
        public string? Description { get; set; } = null;
        [FromForm(Name = "LogType")]
        [DefaultValue(LogType.Info)]
        public LogType LogType { get; set; } = LogType.Info;
        public string? Emotions { get; set; } = null;
        public string? DominantEmotion { get; set; } = null;
        public float AvgArousal { get; set; } = 0;
        public float AvgValence { get; set; } = 0;
        public string? InferredState { get; set; }  
        public string? Region { get; set;}
        public string? Result { get; set; } = null;
        public string? Status { get; set; } = null;
        public bool IsDetected { get; set; } = false; 
        public string? ErrorMessage { get; set; } = null;
    }

    public class FaceCaptureSearchVM : SearchRequestVM
    {
        [Required] public string? StudentExamId { get; set; } = null!;
        [Required] public string? ExamId { get; set; } = null!;

        [FromQuery(Name = "LogType")]
        public LogType? LogType { get; set; }
    }

    public class AIResponseFaceCaptureVM
    {
        public float Angry { get; set; }
        public float Disgust{ get; set; }
        public float Fear { get; set; }
        public float Happy { get; set; }
        public float Neutral { get; set; }
        public float Sad { get; set; }
        public float Surprise { get; set; }
        public string? Message { get; set; } 
    }
}
