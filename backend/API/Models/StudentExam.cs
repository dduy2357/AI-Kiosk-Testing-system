using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace API.Models
{
    [Index(nameof(StudentId))]
    [Index(nameof(ExamId))]
    [Index(nameof(StudentExamId), nameof(ExamId), nameof(Status))]
    [Index(nameof(StudentExamId), nameof(Status))]
    [Index(nameof(StudentExamId), nameof(ExamId))]
    [Index(nameof(StudentExamId), nameof(StudentId))]
    [Index(nameof(ExamId), nameof(StudentId))]
    public class StudentExam
    {
        [Key, StringLength(36)]
        public string StudentExamId { get; set; } = null!;

        [Required, StringLength(36), ForeignKey(nameof(User))]
        public string StudentId { get; set; } = null!;

        [Required, StringLength(36), ForeignKey(nameof(Exam))]
        public string ExamId { get; set; } = null!;
        public DateTime? StartTime { get; set; }
        public DateTime? SubmitTime { get; set; }
        public int? ExtraTimeMinutes { get; set; } // Thời gian gia hạn thêm
        public decimal? Score { get; set; }
        public int? TotalQuestions { get; set; }
        public int? Status { get; set; }
        public string? IpAddress { get; set; }
        public string? BrowserInfo { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual User? User { get; set; }
        public virtual Exam? Exam { get; set; }
        public virtual ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();
    }

}
