using API.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.ViewModels
{
    public class NotificationVM
    {
        public string Id { get; set; } = null!;
        public string Message { get; set; } = string.Empty;
        public string SendToId { get; set; } = null!;
        public string CreatedName { get; set; } = null!;
        public string? CreatedAvatar { get; set; } 
        public string CreatedEmail { get; set; } = null!;
        public string CreatedBy { get; set; } = null!;
        public bool IsRead { get; set; } = false;
        public string? Type { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class NotifyDetailVM
    {
        public string Id { get; set; } = null!;
        public string? Type { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public string StudentName { get; set; } = null!;
        public string? StudentAvatar { get; set; }
        public string StudentUserCode { get; set; } = null!;
        public string CreatedName { get; set; } = null!;
        public string? CreatedAvatar { get; set; }
        public string CreatedUserCode { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class NotificationCreateVM
    {
        [Required, StringLength(500, ErrorMessage = "Message cannot exceed 500 characters.")]
        public string Message { get; set; } = string.Empty;
        [Required, StringLength(36)]
        public string SendToId { get; set; } = null!;
        public string? Type { get; set; }
    }

    public class  NotifySearchVM : SearchRequestVM
    {
        public bool? IsRead { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class NotifyRequest
    {
        [Required, StringLength(36)]
        public string NotifyId { get; set; } = null!;
    }
}
