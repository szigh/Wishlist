using System.ComponentModel.DataAnnotations;

namespace Contracts.DTOs
{
    // For user registration - separate from admin user creation
    public class RegisterDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty; // plain text, will be hashed immediately
    }

    // For user login
    public class LoginDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
