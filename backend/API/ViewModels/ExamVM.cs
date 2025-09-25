using API.Helper;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace API.ViewModels
{
    public class ExamVM
    {
    }

    public class AddExamRequest
    {
        public string QuestionBankId { get; set; } = null!;
        public string RoomId { get; set; }
        //public string QuestionBankId { get; set; }
        [MinLength(2, ErrorMessage = "There must be at least 2 QuestionIds.")]
        public List<string> QuestionIds { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        [Required]
        [Range(1, 2880, ErrorMessage = "Duration must be between 1 and 2880 minutes (48 hours).")]
        public int Duration { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsShowResult { get; set; }
        public bool IsShowCorrectAnswer { get; set; }
        public int Status { get; set; }
        [Required]
        public int ExamType { get; set; }
        public string? GuideLines { get; set; }
        public bool VerifyCamera { get; set; } = true; // true: Verify camera, false: Do not verify camera
    }

    public class ExamListRequest : SearchRequestVM
    {
        public ExamStatus? Status { get; set; }
        [Display(Name = "Show only my questions")]
        [DefaultValue(false)]
        public bool? IsMyQuestion { get; set; } = false;
        [DefaultValue(false)]
        public bool? IsExamResult { get; set; } = false; // true: Get exam result, false: Get exam list
        public string? FilterSubject { get; set; }
    }

    public class ExamOtpVM
    {
        public string? ExamOtpId { get; set; }
        public string ExamId { get; set; } = null!;
        public string OtpCode { get; set; } = null!;
        public int TimeValid { get; set; } = 0;
        public DateTime ExpiredAt { get; set; }
    }

    public class CreateExamOtpVM
    {
        [Required] public string ExamId { get; set; } = null!;
        [Required] public int TimeValid { get; set; } = 0;
    }

    public class ExamListVM
    {
        public string ExamId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; }
        public string RoomName { get; set; }
        public int TotalQuestions { get; set; }
        public decimal TotalPoints { get; set; }
        public int Duration { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedById { get; set; } = null!;
        public int Status { get; set; }
        public bool IsShowResult { get; set; }
        public bool IsShowCorrectAnswer { get; set; }
        public int ExamType { get; set; }
        public string? GuideLines { get; set; }
        public int LiveStatus { get; set; } // 0: Inactive, 1: Upcoming, 2: Ongoing, 3: Completed

        public bool verifyCamera { get; set; }
    }

    public class ExamDetail
    {
        public string ExamId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string RoomId { get; set; }
        public string RoomName { get; set; }
        public int TotalQuestions { get; set; }
        public decimal TotalPoints { get; set; }
        public int Duration { get; set; } // in minutes
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string CreatedBy { get; set; }
        public int Status { get; set; }
        public bool IsShowResult { get; set; }
        public bool IsShowCorrectAnswer { get; set; }
        public int ExamType { get; set; }
        public string? GuideLines { get; set; }
        public string? LiveStatus { get; set; }
        public bool verifyCamera { get; set; }
        public List<SelectedQuestionDto> Questions { get; set; } = new();
    }

    public class ExamResultVM
    {
        public string SubjectName { get; set; } = null!;
        public string ExamDate { get; set; } = null!;
        public int DurationMinutes { get; set; }
        public string QuestionType { get; set; } = null!;
        public decimal TotalPoints { get; set; }
        public string CreatedBy { get; set; } = null!;
        public int TotalStudents { get; set; }
        public decimal AverageScore { get; set; }
        public string AverageDuration { get; set; }
        public string LiveStatus { get; set; } = null!;
        public List<StudentExamResult> StudentResults { get; set; } = new();
    }
    public class StudentExamResult
    {
        public string StudentExamId { get; set; } = null!;
        public bool IsMarked { get; set; } = false;
        public string FullName { get; set; } = null!;
        public string ClassName { get; set; } = null!;
        public decimal? Score { get; set; }
        public string SubmitTime { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string WorkingTime { get; set; } = null!;
    }

    public class UpdateExamRequest
    {
        public string ExamId { get; set; } = null!;
        public string QuestionBankId { get; set; } = null!; // ID của ngân hàng câu hỏi
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string RoomId { get; set; } = null!;
        //public int TotalQuestions { get; set; }
        //public decimal TotalPoints { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Duration { get; set; }
        public bool IsShowResult { get; set; }
        public bool IsShowCorrectAnswer { get; set; }
        public int Status { get; set; }
        public string? GuideLines { get; set; }
        public bool VerifyCamera { get; set; }
        public int ExamType { get; set; }               // 0: Essay, 1: MultipleChoice, ...
        public List<string> QuestionIds { get; set; } = new(); // Danh sách câu hỏi cần cập nhật
    }


    public class StudentExamDetailDto
    {
        // Thông tin học sinh
        public string StudentName { get; set; } = null!;
        public string ClassName { get; set; } = null!;
        public string StudentCode { get; set; } = null!;

        // Thông tin bài thi
        public string ExamTitle { get; set; } = null!;
        public DateTime ExamDate { get; set; }
        public int TotalQuestions { get; set; }
        public decimal TotalPoints { get; set; }
        public int Duration { get; set; }

        // Kết quả
        public decimal Score { get; set; }
        public int CorrectAnswers { get; set; }
        public int WrongAnswers { get; set; }
        public string TimeTaken { get; set; } // đơn vị: phút

        public List<StudentAnswerDetailDto> Answers { get; set; } = new();
    }

    public class StudentAnswerDetailDto
    {
        public string QuestionContent { get; set; } = null!;
        public List<string> Options { get; set; } = new();
        public string UserAnswer { get; set; } = null!;
        public string CorrectAnswer { get; set; } = null!;
        public bool IsCorrect { get; set; }
        public decimal PointsEarned { get; set; }
        public int TimeSpent { get; set; } // đơn vị: giây
        public string Explanation { get; set; } = null!;
    }

    public class SelectedQuestionDto
    {
        public string QuestionId { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DifficultyLevel Difficulty { get; set; } 
        public QuestionTypeChoose Type { get; set; }
        public string QuestionBankId { get; set; } = null!;
        public string QuestionBankName { get; set; } = null!;
    }

    public class ChangeStatusRequest
    {
        public int NewStatus { get; set; }
    }

}
