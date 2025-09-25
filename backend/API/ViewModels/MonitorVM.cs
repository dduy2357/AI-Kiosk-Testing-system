using System.ComponentModel.DataAnnotations;
using API.Helper;
using Microsoft.AspNetCore.Mvc;

namespace API.ViewModels
{
    public class MonitorExamsVM
    {
        public string ExamId { get; set; } = null!;
        public string RoomId { get; set; } = null!;
        public string RoomCode { get; set; } = null!;
        public string ClassId { get; set; } = null!;
        public string ClassCode { get; set; } = null!;
        public string SubjectId { get; set; } = null!;
        public string SubjectName { get; set; } = null!;
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int Duration { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int ExamType { get; set; }
        public int Status { get; set; }
        public bool IsCompleted { get; set; } = false;  
        public string CreateUserId { get; set; } = null!;
        public string CreateUserName { get; set; } = null!;
        public string CreateEmail { get; set; } = null!;

        public int MaxCapacity { get; set; }
        public int StudentDoing { get; set; }
        public int StudentCompleted { get; set; }

        public List<string> StudentIds { get; set; } = new List<string>();  
    }

    public class MonitorExamSearchVM : SearchRequestVM
    {
        public string? SubjectId { get; set; }
        public ExamStatus? ExamStatus { get; set; }
    }

    public class MonitorExamVM
    {
        public string ExamId { get; set; } = null!;
        public string SubjectName { get; set; } = null!;
        public string RoomCode { get; set; } = null!;
        public string Title { get; set; } = null!;
        public int Duration { get; set; }
        public DateTime ExamStartTime { get; set; }
        public DateTime ExamEndTime { get; set; }
        public int ExamType { get; set; }
        public string? ExamLive { get; set; } 
        public string CreateUserName { get; set; } = null!;
        public string CreateEmail { get; set; } = null!;
        public int MaxCapacity { get; set; } = 0; // Maximum number of students allowed in the exam
        public int StudentDoing { get; set; } = 0; // Number of students currently taking the exam
        public int StudentCompleted { get; set; } = 0; // Number of students who have completed the exam
        public List<MonitorExamDetailVM> Students { get; set; } = [];
    }

    public class MonitorExamDetailVM
    {
        public string StudentExamId { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string UserCode { get; set; } = null!;
        public string? Email { get; set; } = null!;
        public string? IpAddress { get; set; }
        public string? BrowserInfo { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? SubmitTime { get; set; }
        public int StudentExamStatus { get; set; } = 0; // 0: In Progress, 1: Completed, 2: Abandoned
        public int TotalQuestions { get; set; } = 0;
        public int AnsweredQuestions { get; set; } = 0;
        public decimal? Score { get; set; } = null;
        public int WarningCount { get; set; } = 0; // Number of warnings for this exam
        public int ViolinCount { get; set; } = 0;
    }

    public class MonitorExamDetailSearchVM : SearchRequestVM
    {
        [Required] public string? ExamId { get; set; } = null!;
        [FromQuery(Name = "StudentExamStatus")]
        public StudentExamStatus? StudentExamStatus { get; set; }
    }

    public class StudentExamExtraTime
    {
        [Required] public string StudentExamId { get; set; } = null!;
        [Required, Range(1, 60, ErrorMessage = "Extra time must be between 1 and 60 minutes.")]
        public int ExtraMinutes { get; set; } = 0;
    }

    public class ExamExtraTime
    {
        [Required] public string ExamId { get; set; } = null!;
        [Required] public string RoomId { get; set; } = null!;
        [Required, Range(1, 60, ErrorMessage = "Extra time must be between 1 and 60 minutes.")]
        public int ExtraMinutes { get; set; }
    }

    public class ReAssignExam
    {
        [Required] public string ExamId { get; set; } = null!;
        [Required] public List<string> StudentIds { get; set; } = null!;
    }
    public class ReAssignStudent
    {
        [Required] public string ExamId { get; set; } = null!;
        [Required] public string StudentId { get; set; } = null!;
    }
    public class FinishExam
    {
        [Required] public string ExamId { get; set; } = null!;
    }

    public class FinishStudentExam
    {
        [Required] public string ExamId { get; set; } = null!;
        [Required] public string StudentExamId { get; set; } = null!;
    }
}
