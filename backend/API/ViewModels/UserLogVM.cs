using API.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using API.Helper;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace API.ViewModels
{
    public class UserLogVM
    {
        public string LogId { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? FullName { get; set; }
        public string? UserCode { get; set; }
        public string? ActionType { get; set; }
        public string? ObjectId { get; set; }
        public string? Description { get; set; }
        public int Status { get; set; }
        public string? Metadata { get; set; }
        public string? IpAddress { get; set; }
        public string? BrowserInfo { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }
    }

    public class UserLogListVM
    {
        public string LogId { get; set; } = null!;
        public string? FullName { get; set; }
        public string UserCode { get; set; } = null!;
        public string? ActionType { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class AddUserLogVM
    {
        public string UserId { get; set; } = null!;
        public string? ActionType { get; set; }
        public string? ObjectId { get; set; }
        public string? Description { get; set; }
        public int Status { get; set; }
        public string? Metadata { get; set; }
    }

    public class ExamLogVM
    {
        public string? ExamLogId { get; set; }
        public string? StudentExamId { get; set; }
        public string? UserId { get; set; }
        public string? UserCode { get; set; }
        public string? FullName { get; set; }
        public string? ActionType { get; set; }
        public string? Description { get; set; }
        public string? IpAddress { get; set; }
        public string? BrowserInfo { get; set; }
        public string? ScreenshotPath { get; set; }
        public string? DeviceId { get; set; }
        public string? DeviceUsername { get; set; }
        public string? Metadata { get; set; }
        public int LogType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ExamLogListVM
    {
        public string? ExamLogId { get; set; }
        public string? UserCode { get; set; }
        public string? FullName { get; set; }
        public string? ActionType { get; set; }
        public string? Description { get; set; }
        public string? Metadata { get; set; }
        public int LogType { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AddExamLogVM
    {
        [Required] public string StudentExamId { get; set; } = null!;
        [Required] public string UserId { get; set; } = null!;
        public string? Metadata { get; set; }
        [Required] public string? ActionType { get; set; }
        [Required] public string? Description { get; set; }
        public IFormFile? ScreenshotPath { get; set; }
        [Required] public LogType LogType { get; set; }
        public string? DeviceId { get; set; }
        public string? DeviceUsername { get; set; } = null!;
    }

    public class LogFilterVM : SearchRequestVM
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        [FromQuery(Name = "RoleEnum")]
        public RoleEnum? RoleId { get; set; }
    }

    public class ExamLogFilterVM : SearchRequestVM
    {
        public string? StudentExamId { get; set; } = string.Empty!;
    }

    public class UserLogFilterVM : LogFilterVM
    {
        [FromQuery(Name = "LogStatus")]
        public LogStatus? LogStatus { get; set; }
        public string? UserId { get; set; }
    }
}
