using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace API.Models
{
    [Index(nameof(ExamId))]
    [Index(nameof(QuestionId))]
    public class ExamQuestion
    {
        [Key, Required, StringLength(36)]
        public string ExamQuestionId { get; set; } = null!;
        [Required, StringLength(36)]
        public string ExamId { get; set; } = null!;
        [Required, StringLength(36)]
        public string QuestionId { get; set; } = null!;
        public decimal Points { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual Exam? Exam { get; set; }
        public virtual Question? Question { get; set; }
    }
}
