using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace API.Models
{
    [Index(nameof(StudentExamId))]
    [Index(nameof(QuestionId))]
    [Index(nameof(StudentAnswerId), IsUnique = true)]
    public class StudentAnswer
    {
        [Key, StringLength(36), Required]
        public string StudentAnswerId { get; set; } = null!;

        [Required, StringLength(36), ForeignKey(nameof(StudentExam))]
        public string StudentExamId { get; set; } = null!;

        [Required, StringLength(36), ForeignKey(nameof(Question))]
        public string QuestionId { get; set; } = null!;
        public string? UserAnswer { get; set; } = null!;
        public bool? IsCorrect { get; set; }
        public decimal? PointsEarned { get; set; }
        public int? TimeSpent { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual StudentExam? StudentExam { get; set; }
        public virtual Question? Question { get; set; }
    }

}
