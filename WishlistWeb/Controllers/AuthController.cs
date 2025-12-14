using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WishlistContracts.DTOs;
using WishlistModels;
using log4net;
using WishlistWeb.Services;

namespace WishlistWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(WishlistDbContext _context, IConfiguration _configuration, IJwtTokenService _jwtTokenService) 
        : ControllerBase
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AuthController));

        [HttpGet("Ping")]
        public IActionResult Ping()
        {
            return Ok();
        }

        [HttpGet("config-check")]
        public IActionResult CheckConfiguration()
        {
            var config = new
            {
                JwtKeyConfigured = !string.IsNullOrEmpty(_configuration["Jwt:Key"]),
                JwtKeyLength = _configuration["Jwt:Key"]?.Length ?? 0,
                ConnectionStringsConfigured = !string.IsNullOrEmpty(_configuration.GetConnectionString("DefaultConnection")),
                AutomapperKeyConfigured= !string.IsNullOrEmpty(_configuration["AutomapperKey"])
            };

            return Ok(config);
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request)
        {
            var validationResult = ValidateLoginRequest(request);
            if (validationResult != null)
            {
                _logger.Warn($"Login validation failed for user: {request?.Name}");
                return validationResult;
            }
            _logger.Info($"Login attempt for user: {request.Name}");

            // Find user by name
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Name == request.Name);

            if (user == null)
            {
                _logger.Warn($"Login failed: User not found - {request.Name}");
                return Unauthorized(new { message = "Invalid username or password" });
            }

            // Verify password
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                _logger.Warn($"Login failed: Invalid password for user - {request?.Name}");
                return Unauthorized(new { message = "Invalid username or password" });
            }

            var token = _jwtTokenService.GenerateToken(user);

            _logger.Info($"Login successful for user: {request.Name} (ID: {user.Id})");
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
                _logger.Warn($"Registration validation failed for user: {request?.Name}");
                return validationResult;
            }
            _logger.Info($"Registration attempt for user: {request.Name}");

            // Check if username already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Name == request.Name);

            if (existingUser != null)
            {
                _logger.Warn($"Registration failed: Username already exists - {request.Name}");
                return BadRequest(new { message = "Username already exists" });
            }

            // Create new user
            var user = new User
            {
                Name = request!.Name,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "user"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _jwtTokenService.GenerateToken(user);

            _logger.Info($"Registration successful for user: {request.Name} (ID: {user.Id})");
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
            var userName = User.FindFirst(JwtRegisteredClaimNames.Name)?.Value;
            _logger.Info($"Logout attempt for user: {userName}");
            // logout is handled in frontend
            return Ok(new { message = "Logged out successfully" });
        }

#pragma warning disable CA1859 // Use concrete types when possible for improved performance
        private ActionResult? ValidateLoginRequest(LoginRequestDto request)
#pragma warning restore CA1859 // Use concrete types when possible for improved performance
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
    }
}
