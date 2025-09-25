using API.Configurations;
using System.ComponentModel.DataAnnotations;

namespace API.Helper
{
    public static class Constant
    {
        public static readonly string UrlImagePath = "wwwroot/img";                                             
        public static readonly IList<string> IMAGE_EXTENDS = new List<string> { ".png", ".jpg", ".jpeg" }.AsReadOnly();

        public const long IMAGE_FILE_SIZE = 1 * 1024 * 1024;
        public const long FILE_SIZE = 2 * 1024 * 1024;

        public const string PERMISSIONS = "001";
        public const string USER_PERMISSION_KEYS = "all_user_permission";
        public const string USER_PERMISSION = "user_permission";
        public const string ROLES = "ROLES";
        public const string CAMPUS = "CAMPUS";
        public const string MAJOR = "MAJOR";
        public const string SPECIALIZATION = "SPECIALIZATION";
        public const string POSITION = "POSITION";
        public const string DFEPARTMENT = "DFEPARTMENT";
        public const string SUBJECT = "SUBJECT";


    }

    public static class ConstMessage
    {  
        public static readonly string ACCOUNT_UNVERIFIED = "Tài khoản chưa được xác minh.";
        public static readonly string EMAIL_EXISTED = "Email này đã tồn tại.";

        public static readonly string LOG_EXAM_VIEWERS = "log_exam_viewer";
        public static readonly string LOG_VIEWERS = "log_viewer";



    }

    public static class ActionLog
    {
        public const string CREATE = "create";
        public const string UPDATE = "update";
        public const string DELETE = "delete";
        public const string LOGIN = "login";

    }

    public static class UrlS3
    {
        public static readonly string UrlMain = ConfigManager.gI().UrlS3Key;
        public static readonly string Camera = "cameras/";
        public static readonly string Log = "logs/";
        public static readonly string Question = "question/";
        public static readonly string Violations = "stviolation/";
        public static readonly string Avatar = "avatar/";


    }

    public enum UserStatus
    {
        Inactive = 0, // Người dùng không hoạt động
        Active,   // Người dùng đang hoạt động
    }

    public enum Gender
    {
        Male = 0,
        Female,
        Other,
    }

    public enum RoleEnum
    {
        [Display(Name = "Student")]
        Student = 1,
        [Display(Name = "Lecture")]
        Lecture,
        [Display(Name = "Supervisor")]
        Supervisor,
        [Display(Name = "Admin")]
        Admin,
    }

    public enum DifficultyLevel
    {
        Easy = 1,
        Medium,
        Hard,
        VeryHard,
    }

    public enum QuestionType
    {
        Essay = 0,
        MultipleChoice,
        TrueFalse,      // Đúng/Sai
        FillInTheBlank, // Điền vào chỗ trống
        ShortAnswer,    // Trả lời ngắn
        Matching,       // Nối
    }
    public enum ExamLiveStatus
    {
        Inactive = 0, // Không hoạt động
        Upcoming,     // Sắp diễn ra
        Ongoing,      // Đang diễn ra
        Completed,    // Đã hoàn thành
    }

    public enum QuestionTypeChoose
    {
        [Display(Name = "Tự Luận")]
        Essay = 0,
        [Display(Name = "Trắc nghiệm")]
        MultipleChoice,
        [Display(Name = "Đúng/Sai")]
        TrueFalse,      // Đúng/Sai
        [Display(Name = "Điền vào chỗ trống")]
        FillInTheBlank, // Điền vào chỗ trống
        [Display(Name = "Trả lời ngắn")]
        ShortAnswer,    // Trả lời ngắn
        [Display(Name = "Nối")]
        Matching,       // Nối
    }

    public enum ActiveStatus
    {
        [Display(Name = "Inactive")]
        Inactive = 0,
        [Display(Name = "Active")]
        Active,
        [Display(Name = "Pending")]
        Pending,
    }

    public enum VerifyCamStatus
    {
        TurnOff = 0, // Bật camera
        TurnOn,    // Tắt camera
    }

    public enum ExamStatus
    {
        [Display(Name = "Draft")]
        Draft = 0, // Bản nháp
        [Display(Name = "Published")]
        Published, // Đã xuất bản
        [Display(Name = "Finished")]
        Finished, // Đã hoàn thành
    }

    public enum RiskLevel
    {
        [Display(Name = "Low")]
        Low = 0,
        [Display(Name = "Medium")]
        Medium,
        [Display(Name = "High")]
        High,
    }

    public enum LogType
    {
        [Display(Name = "Info")]
        Info = 0,
        [Display(Name = "Warning")]
        Warning,
        [Display(Name = "Violation")]
        Violation,   // Vi phạm
        [Display(Name = "Critical")]
        Critical,    // Quan trọng
    }

    public enum LogStatus
    {
        [Display(Name = "Success")]
        Success = 0,

        [Display(Name = "Failed")]
        Failed = 1,

        [Display(Name = "Warning")]
        Warning = 2
    }

    public enum CategoryApp
    {
        [Display(Name = "Website")]
        Website = 1,
        [Display(Name = "Application")]
        Application,
    }

    public enum TypeApp
    {
        [Display(Name = "Prohibited")]
        Prohibited = 0, // Ứng dụng bị cấm
        [Display(Name = "Whitelisted")]
        Whitelisted,    // Ứng dụng được cho phép
    }

    public enum StudentExamStatus
    {
        [Display(Name = "Not Started")]
        NotStarted = 0, // Chưa bắt đầu
        [Display(Name = "In Progress")]
        InProgress,     // Đang làm bài
        [Display(Name = "Submitted")]
        Submitted,      // Đã nộp bài
        [Display(Name = "Failed")]
        Failed,         // Thi trượt
        [Display(Name = "Passed")]
        Passed,         // Thi đỗ
    }

    public enum ShareAccessMode
    {
        ViewOnly = 0,   // Chỉ được xem
        CanEdit = 1     // Có thể chỉnh sửa
    }

    public enum SortType
    {
        [Display(Name = "Ascending")]
        Ascending = 0,
        [Display(Name = "Descending")]
        Descending = 1,
    }
}
