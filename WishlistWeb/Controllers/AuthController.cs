using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WishlistContracts.DTOs;
using WishlistModels;
using WishlistWeb.Services;

namespace WishlistWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly WishlistDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ITokenBlacklistService _tokenBlacklistService;

        public AuthController(
            WishlistDbContext context,
            IConfiguration configuration,
            ITokenBlacklistService tokenBlacklistService)
        {
            _context = context;
            _configuration = configuration;
            _tokenBlacklistService = tokenBlacklistService;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request)
        {
            var validationResult = ValidateLoginRequest(request);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Find user by name
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Name == request.Name);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            // Verify password
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            // Generate JWT token
            var token = GenerateJwtToken(user);

            return Ok(new LoginResponseDto
            {
                Token = token,
                UserId = user.Id,
                Name = user.Name,
                Role = user.Role
            });
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<LoginResponseDto>> Register(LoginRequestDto request)
        {
            var validationResult = ValidateLoginRequest(request);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Check if username already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Name == request.Name);

            if (existingUser != null)
            {
                return BadRequest(new { message = "Username already exists" });
            }

            // Create new user
            var user = new User
            {
                Name = request.Name,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "user"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generate JWT token and auto-login
            var token = GenerateJwtToken(user);

            return Ok(new LoginResponseDto
            {
                Token = token,
                UserId = user.Id,
                Name = user.Name,
                Role = user.Role
            });
        }

        // POST: api/auth/logout
        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Extract token JTI (unique ID) from claims
            var tokenId = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            if (tokenId == null)
            {
                return BadRequest(new { message = "Invalid token" });
            }

            // Get token expiration from claims
            var expClaim = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
            if (expClaim == null)
            {
                return BadRequest(new { message = "Invalid token expiration" });
            }

            // Convert Unix timestamp to DateTime
            var expiration = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim)).UtcDateTime;

            // Blacklist the token
            _tokenBlacklistService.BlacklistToken(tokenId, expiration);

            return Ok(new { message = "Logged out successfully" });
        }

        private ActionResult? ValidateLoginRequest(LoginRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Request cannot be null" });
            }

            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Username and password are required" });
            }
            return null;
        }

        /// <summary>
        /// Generates a JWT (JSON Web Token) for authenticating the specified user.
        /// </summary>
        /// <param name="user">The user for whom to generate the authentication token.</param>
        /// <returns>A signed JWT token string that can be used for bearer authentication.</returns>
        /// <remarks>
        /// This method creates a JWT token with the following security characteristics:
        /// <list type="bullet">
        /// <item><description>Signed using HMAC-SHA256 algorithm with the configured secret key</description></item>
        /// <item><description>Includes standard claims: Subject (user ID), Name, Role, and JTI (unique token identifier)</description></item>
        /// <item><description>Token expiration is controlled by the Jwt:ExpirationMinutes configuration setting</description></item>
        /// <item><description>The JTI claim enables token revocation via the blacklist service</description></item>
        /// </list>
        /// Security considerations:
        /// <list type="bullet">
        /// <item><description>Tokens are stateless and cannot be revoked until they expire, unless blacklisted via the logout endpoint</description></item>
        /// <item><description>The signing key must be kept secure and should be stored in a secure configuration (e.g., user secrets, Azure Key Vault)</description></item>
        /// <item><description>Token expiration should be set appropriately to balance security and user experience</description></item>
        /// </list>
        /// </remarks>
        private string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];
            var jwtExpirationMinutes = _configuration.GetValue<int>("Jwt:ExpirationMinutes");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtExpirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
