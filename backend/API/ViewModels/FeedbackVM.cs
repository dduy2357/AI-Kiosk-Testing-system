using API.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.ViewModels
{
    public class FeedbackVM
    {
        public string FeedbackId { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? UserPhone { get; set; }
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsResolved { get; set; } = false;
        public string? ResponseContent { get; set; }
        public DateTime? ResponseAt { get; set; }
    }

    public class CreateUpdateFeedbackVM
    {
        public string? Id { get; set; }  

        [Required, MaxLength(36, ErrorMessage = "Title cannot exceed 120 characters")]
        public string Title { get; set; } = null!;

        [Required, MaxLength(500, ErrorMessage = "Content cannot exceed 500 characters")]
        public string Content { get; set; } = null!;
    }

    public class FeedbackSearchVM : SearchRequestVM
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}
