using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    //[Index(nameof(CreatedBy))]
    [Index(nameof(SubjectId))]
    [Index(nameof(CreateUserId))]
    [Index(nameof(Title))]
    public class QuestionBank
    {
        [Key]
        [Required]
        [StringLength(36)]     
        public string QuestionBankId { get; set; } = null!;

        [Required]
        [ForeignKey(nameof(Subject))]
        [StringLength(36)]
        public string SubjectId { get; set; } = null!;

        [Required]
        [StringLength(36)]
        [ForeignKey(nameof(CreatedByUser))]
        public string CreateUserId { get; set; } = null!;

        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual Subject Subject { get; set; } = null!;
        public virtual User CreatedByUser { get; set; } = null!;
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    }
}