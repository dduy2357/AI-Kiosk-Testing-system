using System.ComponentModel.DataAnnotations;

namespace API.ViewModels
{
    public class CampusVM
    {
        public string Id { get; set; } = null!;
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }

    }
}
