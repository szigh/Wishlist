namespace Contracts.DTOs
{
    // For user registration - separate from admin user creation
    public class RegisterDto
    {
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // plain text, will be hashed immediately
    }

    // For user login
    public class LoginDto
    {
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
