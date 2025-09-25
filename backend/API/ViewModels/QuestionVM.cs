using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Attributes;
using API.Helper;

namespace API.ViewModels
{
    public class QuestionVM
    {
        public string QuestionId { get; set; } = null!;
        public string SubjectId { get; set; } = null!;
        public string Content { get; set; } = null!;
        public int Type { get; set; }
        public int DifficultLevel { get; set; }
        public decimal Point { get; set; }

        public List<string> Options { get; set; } = null!;
        public string CorrectAnswer { get; set; } = null!;
        public string Explanation { get; set; } = null!;
        public string? ObjectFile { get; set; }
        public int Status { get; set; }
        public string? CreatorId { get; set; }

    }

    public class QuestionListVM
    {
        public string QuestionId { get; set; } = null!;
        public string SubjectId { get; set; } = null!;
        public string? SubjectName { get; set; }
        public string QuestionBankId { get; set; } = null!;
        public string? QuestionBankName { get; set; }

        public string Content { get; set; } = null!;
        public int Type { get; set; }
        public int DifficultLevel { get; set; }
        public decimal Point { get; set; }

        public List<string> Options { get; set; } = null!;
        public string CorrectAnswer { get; set; } = null!;
        public string Explanation { get; set; } = null!;
        public string? ObjectFile { get; set; }
        public int Status { get; set; }
        public string? CreatorId { get; set; }

    }

    public class QuestionRequestVM : SearchRequestVM
    {
        public ActiveStatus? Status { get; set; } // 1: Active, 0: Inactive
        [Display(Name = "Show only my questions")]
        [DefaultValue(false)]
        public bool? IsMyQuestion { get; set; } = false;

        public DifficultyLevel? DifficultyLevel { get; set; }
    }
    public class ToggleQuestionStatusRequest
    {
        [Required]
        public string QuestionId { get; set; } = null!;

        //[Required]
        //public string UserId { get; set; } = null!;
    }

    public class EditQuestionRequest : AddQuestionRequest
    {
        [Required]
        public string QuestionId { get; set; } = null!;
        [Required]
        public string CreateUserId { get; set; } = null!;
    }

    public class ImportQuestionRequest
    {
        public string QuestionBankId { get; set; } = null!;
        public IFormFile File { get; set; } = null!;
    }



    public class AddQuestionRequest
    {
        [Required]
        public string QuestionBankId { get; set; } = null!;

        //[Required]
        //public string SubjectId { get; set; } = null!;

        [Required]
        public string Content { get; set; } = null!;

        [Required]
        [DefaultValue(1)]
        public int Type { get; set; }

        [Required]
        public int DifficultLevel { get; set; }

        [Required]
        [Range(1, 10)]
        [DefaultValue(1)]
        public decimal Point { get; set; }

        [Required]
        public List<string> Options { get; set; } = new();

        [Required]
        public string CorrectAnswer { get; set; } = null!;

        public string? Explanation { get; set; }
        public string? ObjectFile { get; set; }

        public string? Tags { get; set; }
        public string? Description { get; set; }

    }

    public class QuestionListResponse : SearchResult
    {
        public int TotalQuestions { get; set; }
    }

    public class FormatQuestionList
    {
        [Required(ErrorMessage =("QuestionBankId cannot null or empty"))]
        public string QuestionBankId { get; set; } = null!;
        [Required(ErrorMessage =("File cannot null or empty"))]
        [MaxFileSize(Constant.FILE_SIZE)]
        public IFormFile File { get; set; } = null!;
    }
}
