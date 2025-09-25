using System.ComponentModel.DataAnnotations;

namespace DesktopApp.Constants
{
    public static class Common
    {
        public static readonly string API_BASE = "https://2handshop.id.vn/";
    }
    public static class ExamEventType
    {
        public const string Token = "token";
        public const string StartExam = "startExam";
        public const string CaptureScreenshot = "captureScreenshot";
    }
    public enum LogType
    {
        [Display(Name = "Infor")]
        Info = 0,
        [Display(Name = "Warning")]
        Warning,
        [Display(Name = "Violation")]
        Violation,   // Vi phạm
        [Display(Name = "Critical")]
        Critical,    // Quan trọng
    }

}
