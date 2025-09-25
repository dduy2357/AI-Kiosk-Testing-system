using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.ViewModels
{
    public class PositionVM
    {
        public string Id { get; set; } = null!;
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public string? DepartmentId { get; set; } = null!;
    }
}
