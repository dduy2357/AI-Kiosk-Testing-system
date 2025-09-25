using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models;

[Index(nameof(Email), IsUnique = true)]
[Index(nameof(UserId))]
[Index(nameof(GoogleId))]
[Index(nameof(Phone), IsUnique = true)]
[Index(nameof(UserCode), IsUnique = true)]
public partial class User
{
    [Key]
    [Required]
    [StringLength(36)]
    public string UserId { get; set; } = null!;
    [MaxLength(255)] public string? UserCode { get; set; }
    [MaxLength(255)]  public string? FullName { get; set; }
    [MaxLength(255)] public string? Email { get; set; }
    [MaxLength(255)] public string? Password { get; set; }
    [MaxLength(255)] public string? GoogleId { get; set; }
    public string? AvatarUrl { get; set; } 
    //public bool TwoFactorEnabled { get; set; } = false;
    public int? Sex { get; set; }
    public DateTime Dob{ get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    [ForeignKey("Campus")]
    public string? CampusId { get; set; }

    [ForeignKey("Department")]
    public string? DepartmentId { get; set; }

    [ForeignKey("Position")]
    public string? PositionId { get; set; }

    [ForeignKey("Major")]
    public string? MajorId { get; set; }

    [ForeignKey("Specialization")]
    public string? SpecializationId { get; set; }
    public int? Status { get; set; }         
    //public DateTime? BlockUntil { get; set; }
    public DateTime? LastLogin { get; set; }
    public string? LastLoginIp { get; set; }
    public DateTime? CreateAt { get; set; }
    public DateTime? UpdateAt { get; set; }
    public string? CreateUser { get; set; }
    public string? UpdateUser { get; set; }

    // Navigation properties
    public virtual Campus? Campus { get; set; }
    public virtual Department? Department { get; set; }
    public virtual Position? Position { get; set; }
    public virtual Major? Major { get; set; }
    public virtual Specialization? Specialization { get; set; }
    public virtual ICollection<RoomUser> RoomUsers { get; set; } = new List<RoomUser>(); 
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    // add question bank by anhnt(17/06/2025)
    public virtual ICollection<QuestionBank> QuestionBanks { get; set; } = new List<QuestionBank>();
}
