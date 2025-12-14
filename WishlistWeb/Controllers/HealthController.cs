using Microsoft.AspNetCore.Mvc;

namespace WishlistWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : BaseApiController
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
