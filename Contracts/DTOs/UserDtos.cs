namespace Contracts.DTOs
{
    // For creating a new user (admin only)
    public class UserCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // plain text, will be hashed
    }

    // For updating user details (e.g. role, username)
    public class UserUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
    }

    // For returning user info (no password hash exposed)
    public class UserReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    // For returning user info with their gifts wishlist
    public class UserWishlistReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<GiftReadDto> Gifts { get; set; } = [];
    }

    // For login request
    public class LoginRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // For login response with token
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
