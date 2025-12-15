using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using WishlistContracts.DTOs;
using WishlistModels;
using log4net;
using WishlistWeb.Services;

namespace WishlistWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(WishlistDbContext _context, IJwtTokenService _jwtTokenService) 
        : ControllerBase
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AuthController));


        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto? request)
        {
            if (!TryValidateRequest(request, out var validRequest, out var error))
            {
                _logger.Warn($"Login validation failed for user: {request?.Name}");
                return error;
            }

            _logger.Info($"Login attempt for user: {validRequest.Name}");

            // Find user by name
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Name == validRequest.Name);

            if (user == null)
            {
                _logger.Warn($"Login failed: User not found - {validRequest.Name}");
                return Unauthorized(new { message = "Invalid username or password" });
            }

            // Verify password
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(validRequest.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                _logger.Warn($"Login failed: Invalid password for user - {validRequest.Name}");
                return Unauthorized(new { message = "Invalid username or password" });
            }

            var token = _jwtTokenService.GenerateToken(user);

            _logger.Info($"Login successful for user: {validRequest.Name} (ID: {user.Id})");
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
        public async Task<ActionResult<LoginResponseDto>> Register(LoginRequestDto? request)
        {
            if (!TryValidateRequest(request, out var validRequest, out var error))
            {
                _logger.Warn($"Registration validation failed for user: {request?.Name}");
                return error;
            }

            _logger.Info($"Registration attempt for user: {validRequest.Name}");

            // Check if username already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Name == validRequest.Name);

            if (existingUser != null)
            {
                _logger.Warn($"Registration failed: Username already exists - {validRequest.Name}");
                return BadRequest(new { message = "Username already exists" });
            }

            // Create new user
            var user = new User
            {
                Name = validRequest.Name,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(validRequest.Password),
                Role = "user"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _jwtTokenService.GenerateToken(user);

            _logger.Info($"Registration successful for user: {validRequest.Name} (ID: {user.Id})");
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

        private bool TryValidateRequest(
            LoginRequestDto? request,
            [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out LoginRequestDto? validRequest,
            [System.Diagnostics.CodeAnalysis.NotNullWhen(false)] out ActionResult? error)
        {
            error = null;

            if (request == null)
            {
                validRequest = null;
                error = BadRequest(new { message = "Request cannot be null" });
                return false;
            }

            validRequest = request;

            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Password))
            {
                error = BadRequest(new { message = "Username and password are required" });
                return false;
            }

            return true;
        }
    }
}
