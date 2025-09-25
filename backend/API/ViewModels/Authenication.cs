using System.ComponentModel.DataAnnotations;

namespace API.ViewModels
{
    public class UserLogin
    {
        [Required(ErrorMessage = "Email cannot be empty")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password cannot be empty")]
        public string? Password { get; set; }
        [Required(ErrorMessage = "Please choose a campus before logging in")]
        public string? CampusId { get; set; } = string.Empty;
    }

    public class UserRegister
    {
        [StringLength(50, ErrorMessage = "Fullname must not exceed 20 characters")]
        public string? Fullname { get; set; }

        [Required(ErrorMessage = "Email cannot be empty")]
        [StringLength(100, ErrorMessage = "Email is too long")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password cannot be empty")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Confirm password cannot be empty")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        public string? RePassword { get; set; }
    }

    public class ForgetPassword
    {
        [Required(ErrorMessage = "Email cannot be empty")]
        [StringLength(60, ErrorMessage = "Email is too long")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }
    }

    public class VerifyOTP
    {
        [Required(ErrorMessage = "Please enter the captcha code")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Invalid code length. Please try again.")]
        public int OTP { get; set; }

        [Required(ErrorMessage = "Email cannot be empty")]
        [StringLength(60, ErrorMessage = "Email is too long")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ExpiredDate { get; set; }
    }

    public class ResetPassword
    {
        public string? UserId { get; set; }

        [Required(ErrorMessage = "Password cannot be empty")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Confirm password cannot be empty")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        public string? RePassword { get; set; }
    }

    public class ChangePassword
    {
        public string? UserId { get; set; }

        [Required(ErrorMessage = "Old password cannot be empty")]
        public string? ExPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password cannot be empty")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@#?!])[A-Za-z\d@#?!]{8,}$",
          ErrorMessage = "Password must contain at least 8 characters, including at least one lowercase letter, one uppercase letter, one number, and one special character.")]
        public string? Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password cannot be empty")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        public string? RePassword { get; set; } = string.Empty;
    }
}
