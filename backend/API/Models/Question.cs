using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    [Index(nameof(QuestionBankId))]
    [Index(nameof(DifficultLevel))]
    [Index(nameof(Type))]
    [Index(nameof(SubjectId))]
    public class Question
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string QuestionId { get; set; } = null!;

        [Required]
        [ForeignKey(nameof(Subject))]
        [StringLength(36)]
        public string SubjectId { get; set; } = null!;

        [Required]
        [ForeignKey(nameof(QuestionBank))]
        [StringLength(36)]
        public string QuestionBankId { get; set; } = null!;

        public string Content { get; set; } = null!;
        public int Type { get; set; }
        public int DifficultLevel { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal Point { get; set; }

        public string Options { get; set; } = null!;
        public string CorrectAnswer { get; set; } = null!;
        public string Explanation { get; set; } = null!;
        public string? ObjectFile { get; set; }
        public int Status { get; set; }

        [ForeignKey(nameof(CreatedByUser))]
        public string CreateUser { get; set; } = null!;

        [ForeignKey(nameof(UpdatedByUser))]
        public string UpdateUser { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual Subject Subject { get; set; } = null!;
        public virtual QuestionBank QuestionBank { get; set; } = null!;
        public virtual User CreatedByUser { get; set; } = null!;
        public virtual User UpdatedByUser { get; set; } = null!;
    }


}
