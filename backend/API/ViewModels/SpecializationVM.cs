using System.ComponentModel.DataAnnotations;

namespace API.ViewModels
{
    public class SpecializationVM
    {
        public string Id { get; set; } = null!;
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool Status { get; set; } = true;
    }
}
