using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DesktopApp.Constants;

namespace DesktopApp.Models
{
    public class ExamEventMessage
    {
        [JsonPropertyName("event")]
        public string? Type { get; set; }

        [JsonPropertyName("examId")]
        public string? ExamId { get; set; }

        [JsonPropertyName("studentExamId")]
        public string? StudentExamId { get; set; }

        [JsonPropertyName("timestamp")]
        public string? Timestamp { get; set; }

        [JsonPropertyName("userId")]
        public string? UserId { get; set; }

        [JsonPropertyName("token")]
        public string? Token { get; set; }
    }

    public class CaptureRequest
    {
        public string StudentExamId { get; set; } = null!;
        public byte[] ImageCapture { get; set; } = null!;
        public bool IsDetected { get; set; } = false;
        public string? Description { get; set; }
        public string? Emotions { get; set; }
        public string? DominantEmotion { get; set; }
        public LogType LogType { get; set; } = LogType.Info;

        public float AvgArousal { get; set; } = 0;
        public float AvgValence { get; set; } = 0;
        public string? InferredState { get; set; }
        public string? Region { get; set; }
        public string? Result { get; set; } = null;
        public string? Status { get; set; } = null;
        public string? ErrorMessage { get; set; } = null;
    }

}
