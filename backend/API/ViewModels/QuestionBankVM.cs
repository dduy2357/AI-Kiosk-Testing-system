using API.Helper;
using API.Models;
using MimeKit.Tnef;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace API.ViewModels
{
    public class QuestionBankVM
    {
        public string QuestionBankId { get; set; }
        public string QuestionBankName { get; set; }
        public string Description { get; set; }
        public string SubjectName { get; set; }
        public int Status { get; set; } // 1: Active, 0: Inactive
        public DateTime CreatedAt { get; set; }
        public string CreateUserId { get; set; }
    }

    public class ShareQuestionBankRequest
    {
        public string QuestionBankId { get; set; } = null!;
        public string TargetUserEmail { get; set; } = null!;
        public int AccessMode { get; set; } // 0 = ReadOnly, 1 = Edit
    }

    public class QuestionBankListVM
    {
        public string? QuestionBankId { get; set; }
        public string? Title { get; set; }
        public string? CreateBy { get; set; }
        public string? SubjectName { get; set; }
        public int TotalQuestions { get; set; }
        public int MultipleChoiceCount { get; set; }
        public int TrueFalseCount { get; set; }
        public int FillInTheBlank { get; set; }
        public int EssayCount { get; set; }
        public int Status { get; set; }
        public string? SharedByName { get; set; }
        public List<string> SharedWithUsers { get; set; } = new();
    }

    public class EditQuestionBankRequest
    {
        public string Title { get; set; } = null!;
        public string SubjectId { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class QuestionBankListResponse : SearchResult
    {
        public int TotalQuestionBanks { get; set; }
        public int TotalQuestionsQB { get; set; }
        public int TotalSubjects { get; set; }
        public int TotalSharedQB { get; set; }
    }
    public class AddQuestionBankRequest
    {
        public string Title { get; set; } = null!;
        public string SubjectId { get; set; } = null!;
        public string? Description { get; set; }
        //public string CreateUserId { get; set; } = null!;
    }

    public class QuestionBankDetailVM
    {
        public string QuestionBankId { get; set; }
        public string QuestionBankName { get; set; }
        public string CreateBy { get; set; }
        public string Description { get; set; }
        public string SubjectId { get; set; }
        public string SubjectName { get; set; }
        public int TotalQuestions { get; set; }
        public int MultipleChoiceCount { get; set; }
        public int EssayCount { get; set; }
        public int Status { get; set; }
        //public double AverageDifficulty { get; set; }
        public List<QuestionVM> Questions { get; set; }= new();
    }

    

    public class QuestionBankFilterVM : SearchRequestVM
    {
        public ActiveStatus? Status { get; set; } // 1: Active, 0: Inactive
        //[Display(Name = "Show only my question banks")]
        [DefaultValue(false)]
        public bool? IsMyQuestion { get; set; } = false;
        public string? filterSubject { get; set; }
    }

    public class PaginatedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
    }

    public class QuestionBankDetailViewModel
    {
        public string QuestionBankId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string SubjectName { get; set; } = null!;
        public int TotalQuestions { get; set; }
        public double PassRate { get; set; }
        public int UsageCount { get; set; }
        public int MultipleChoiceCount { get; set; }
        public int EssayCount { get; set; }

        public List<Question> Questions { get; set; } = new();
    }

    public class ChangeAccessModeRequest
    {
        public string QuestionBankId { get; set; }
        public string TargetUserEmail { get; set; }
        public int NewAccessMode { get; set; }
    }

}
