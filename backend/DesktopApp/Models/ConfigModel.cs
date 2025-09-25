using DesktopApp.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace DesktopApp.Models
{
    public class ConfigModel
    {
        public List<string>? ShortcutKeys { get; set; }
        public List<string>? BlockedApps { get; set; }
        public List<string>? WhitelistApps { get; set; }
        public int MaxDurationMinutes { get; set; }
        public bool EnableFullscreen { get; set; }
        public bool DisableTouchpad { get; set; } = true;
        public string ProtectedUrl { get; set; } = string.Empty; // URL website ReactJS
        public bool PreventScreenLock { get; set; } = true;
        public bool BlockVirtualMachines { get; set; } = true;
    }

    public class ConfigModelVM
    {
        public List<string>? ShortcutKeys { get; set; }
        public List<string>? BlockedApps { get; set; }
        public List<string>? WhiteListApps { get; set; }
        public string ProtectedUrl { get; set; } = string.Empty; // URL website ReactJS
    }

    public class LogVM
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

    public class AddExamLogVM
    {
        [Required] public string StudentExamId { get; set; } = null!;
        [Required] public string UserId { get; set; } = null!;
        public string? Metadata { get; set; }
        [Required] public string? ActionType { get; set; }
        [Required] public string? Description { get; set; } = string.Empty;
        public byte[]? ScreenshotPath { get; set; }
        [Required] public LogType LogType { get; set; }
    }

}
