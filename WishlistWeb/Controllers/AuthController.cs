using AutoMapper;
using Contracts.DTOs;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace WishlistWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(WishlistDbContext context, IMapper mapper) : ControllerBase
    {
        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<UserReadDto>> Register(RegisterDto dto)
        {
            // Check if user already exists
            var existingUser = context.Users.FirstOrDefault(u => u.Name == dto.Name);
            if (existingUser != null)
            {
                return BadRequest("User already exists");
            }

            // Create new user
            var user = new User
            {
                Name = dto.Name,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "User" // Default role for self-registered users
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Return user info without password
            var readDto = mapper.Map<UserReadDto>(user);
            return CreatedAtAction("GetUser", "Users", new { id = user.Id }, readDto);
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<UserReadDto>> Login(LoginDto dto)
        {
            var user = context.Users.FirstOrDefault(u => u.Name == dto.Name);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid credentials");
            }

            var readDto = mapper.Map<UserReadDto>(user);
            return Ok(readDto);
        }
    }
}
