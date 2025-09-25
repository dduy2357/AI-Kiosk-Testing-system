using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using API.ViewModels;
using Microsoft.EntityFrameworkCore;
using API.Helper;

namespace API.Models
{
    [Index(nameof(QuestionBankId))]
    [Index(nameof(SharedWithUserId))]
    public class QuestionShare
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string QuestionShareId { get; set; } = null!;

        [Required]
        [StringLength(36)]
        public string QuestionBankId { get; set; } = null!;

        [Required]
        [StringLength(36)]
        [ForeignKey(nameof(SharedWithUser))]
        public string SharedWithUserId { get; set; } = null!;

        [Required]
        public int AccessMode { get; set; } 
        [ForeignKey(nameof(QuestionBankId))]
        public virtual QuestionBank QuestionBank { get; set; } = null!;
        [ForeignKey(nameof(SharedWithUserId))]
        public virtual User SharedWithUser { get; set; } = null!;
    }
}
