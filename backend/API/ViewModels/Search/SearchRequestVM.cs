using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace API.ViewModels
{
    public class SearchRequestVM
    {
        [DefaultValue(50)]
        public int PageSize { get; set; } = 50;

        [DefaultValue(1)]
        public int CurrentPage { get; set; } = 1;

        [DefaultValue("")]
        [Display(Name = "Text Search", Description = "Search by name, code...")]
        public string? TextSearch
        {
            get => _textSearch;
            set => _textSearch = value?.Trim();
        }
        private string? _textSearch = string.Empty;
    }

    public class PagingVM
    {
        public int PageSize { get; set; } = 20;
        public int CurrentPage { get; set; }
        public int Start => CurrentPage <= 1 ? 0 : (CurrentPage - 1) * PageSize;
    }
    public class SearchResult 
    {
        public object? Result { get; set; }
        public int TotalPage { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int Total { get; set; }
    }
}
