using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using API.Helper;

namespace API.Models
{
    [Index(nameof(RoomId))]
    [Index(nameof(CreateUser))]
    public class Exam
    {
        [Key, StringLength(36)]
        public string ExamId { get; set; } = null!;

        [Required, StringLength(36)]
        [ForeignKey(nameof(Room))]
        public string RoomId { get; set; } = null!;

        [StringLength(255)]
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int Duration { get; set; }
        public decimal TotalPoints { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int MaxScore { get; set; }
        public int ExamType { get; set; }
        public int TotalQuestions { get; set; }
        public bool IsShowResult { get; set; }
        public bool IsShowCorrectAnswer { get; set; }
        //public bool IsShuffle { get; set; }
        //public bool IsShuffleAnswer { get; set; }
        public int Status { get; set; }
        [NotMapped]
        public ExamLiveStatus LiveStatus
        {
            get
            {
                if (Status == 0)
                    return ExamLiveStatus.Inactive;

                var now = DateTime.UtcNow;

                if (now < StartTime)
                    return ExamLiveStatus.Upcoming;

                if (now >= StartTime && now <= EndTime)
                    return ExamLiveStatus.Ongoing;

                return ExamLiveStatus.Completed;
            }
        }

        public string? GuildeLines { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [Required, StringLength(36)]
        [ForeignKey(nameof(Creator))]
        public string CreateUser { get; set; } = null!;

        [StringLength(36)]
        [ForeignKey(nameof(Updater))]
        public string? UpdateUser { get; set; }
        public bool VerifyCamera { get; set; }

        public virtual Room? Room { get; set; }
        public virtual User? Creator { get; set; }
        public virtual User? Updater { get; set; }
        [InverseProperty(nameof(ExamSupervisor.Exam))]
        public virtual ICollection<ExamSupervisor> ExamSupervisors { get; set; } = new List<ExamSupervisor>();
        public virtual ICollection<ExamQuestion> ExamQuestions { get; set; } = new List<ExamQuestion>();
    }
}
