using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    public class ExamOtp
    {
        [Key, Required, StringLength(36)]
        public string ExamOtpId { get; set; } = null!;

        [Required, StringLength(36)]
        [ForeignKey(nameof(Exam))]
        public string ExamId { get; set; } = null!;
        public int OtpCode { get; set; }
        public int TimeValid { get; set; } = 0; // Time in minutes for which the OTP is valid
       
        [Required, StringLength(36)]
        [ForeignKey(nameof(CreatedByUser))]
        public string CreatedBy { get; set; } = null!; // User who created the OTP
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiredAt { get; set; }  

        // Navigation
        public virtual User CreatedByUser { get; set; } = null!;
        public virtual Exam Exam { get; set; } = null!;
    }
}
