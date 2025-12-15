using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WishlistContracts.DTOs;

namespace WishlistWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController(IConfiguration _configuration) : BaseApiController
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new HealthResponseDto
            {
                Status = "healthy",
                Timestamp = DateTime.UtcNow
            });
        }

        [Authorize]
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
    }
}
