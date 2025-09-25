using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DesktopApp.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
    public static class DataStorage
    {
        public static string? AccessToken { get; set; }
        public static string? ExamId { get; set; }
        public static string? StudentExamId { get; set; }
        public static string? UserId { get; set; }
    }

}
