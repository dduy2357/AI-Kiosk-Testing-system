using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace API.Models
{
    [Index(nameof(SubjectName))]
    [Index(nameof(Status))]
    public class Subject
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string SubjectId { get; set; } = null!;

        [Required]
        [StringLength(500)]
        public string SubjectName { get; set; } = null!;

        [StringLength(500)]
        public string? SubjectDescription { get; set; }

        [Required]
        [StringLength(100)]
        public string SubjectCode { get; set; } = null!;

        [Column(TypeName = "TEXT")]
        public string? SubjectContent { get; set; }
        public bool Status { get; set; } = true;
        public int Credits { get; set; }    
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();

        // add question bank by anhnt(17/06/2025)
        public virtual ICollection<QuestionBank> QuestionBanks { get; set; } = new List<QuestionBank>();
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    }

}
