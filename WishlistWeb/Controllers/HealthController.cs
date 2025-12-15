using Microsoft.AspNetCore.Mvc;
using WishlistContracts.DTOs;

namespace WishlistWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : BaseApiController
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
    }
}
