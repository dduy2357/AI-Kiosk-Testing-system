using API.Models;
using ExpressiveAnnotations.Attributes;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.ViewModels
{
    public class StudentExamResultVM
    {
        public string StudentExamId { get; set; } = null!;
        public string ExamId { get; set; } = null!;
        public string? ExamTitle { get; set; }
        public decimal Score { get; set; }
        public DateTime? ExamDate { get; set; }
        public DateTime? SubmitTime { get; set; }
        public int DurationSpent { get; set; }
    }

    public class StudentExamDetailVM
    {
        public string ExamTitle { get; set; } = "";
        public decimal Score { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? SubmitTime { get; set; }
        public int DurationSpent { get; set; }
        public int TotalCorrectAnswers { get; set; }
        public int TotalWrongAnswers { get; set; }
        public List<AnswerDetail>? Answers { get; set; } = new();
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
    }

    public class AnswerDetail
    {
        public string QuestionId { get; set; } = "";
        public string QuestionContent { get; set; } = "";
        public string? UserAnswer { get; set; }
        public string? CorrectAnswer { get; set; }
        public bool? IsCorrect { get; set; }
        public decimal PointsEarned { get; set; }
        public int? TimeSpent { get; set; }
        public string Options { get; set; }
    }

    public class StudentExamRequest
    {
        [Required] public string ExamId { get; set; } = null!;
        [Required] public int OtpCode { get; set; }
    }

    public class SubmitExamRequest
    {
        [Required] public string StudentExamId { get; set; } = null!;
        [Required] public string ExamId { get; set; } = null!;
        public List<StudentAnswerVM> Answers { get; set; } = new();
    }

    public class StudentAnswerVM
    {
        [Required] public string QuestionId { get; set; } = null!;
        public string? UserAnswer { get; set; }
    }

    public class StudentEssayAnswerVM
    {
        public string StudentExamId { get; set; } = null!;
        public string ExamId { get; set; } = null!;
        public int ExamType { get; set; } = 0; // 0: Multiple Choice, 1: Essay
        public int StudentExamStatus { get; set; } = 0; // 0: Not Started, 1: In Progress, 2: Submitted, 3: Failed, 4: Passed
        public string? ExamTitle { get; set; }
        public string SubjectName { get; set; } = string.Empty; 
        public string RoomCode { get; set; } = string.Empty;
        public DateTime? ExamDate { get; set; }
        public DateTime? SubmitTime { get; set; }
        public int DurationSpent { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public string StudentAvatar { get; set; } = string.Empty;
        public bool IsMarked { get; set; } = false;
        public decimal? Score { get; set; } = 0;
        public int TotalQuestions { get; set; }
        public List<EssayAnswerVM> Answers { get; set; } = new();
    }

    public class EssayAnswerVM
    {
        public string? QuestionId { get; set; }
        public string? QuestionContent { get; set; }
        public string? CorrectAnswer { get; set; }
        public string? UserAnswer { get; set; }
        public decimal MaxPoints { get; set; }
        public decimal PointsEarned { get; set; }
    }

    public class MarkEssayRequest
    {
        [Required] public string StudentExamId { get; set; } = null!;
        [Required] public string ExamId { get; set; } = null!;
        [Required] public List<EssayScoreVM> Scores { get; set; } = new();
    }

    public class EssayScoreVM
    {
        [Required] public string QuestionId { get; set; } = null!;
        [Required]
        [Range(0, 10, ErrorMessage = "Points must be between 0 and 10")]
        public decimal PointsEarned { get; set; }
    }

    public class SearchStudentExamVM : SearchRequestVM
    {
        public DateTime? StartDate { get; set; }
        [AssertThat("EndDate >= StartDate", ErrorMessage = "EndDate must be greater than or equal to StartDate")]
        public DateTime? EndDate { get; set; }
    }

    public class ExamDetailVM
    {
        public string ExamId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public int ExamType { get; set; }
        public int Duration { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int TotalQuestions { get; set; }
        public List<ExamQuestionVM> Questions { get; set; } = new();
    }

    public class ExamQuestionVM
    {
        public string QuestionId { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string QuestionType { get; set; } = null!;
        public decimal Point { get; set; }
        public int DifficultLevel { get; set; }
        public List<string> Options { get; set; } = [];
    }


}
