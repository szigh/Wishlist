using Microsoft.AspNetCore.Mvc;

namespace WishlistWeb.Controllers
{
    /// <summary>
    /// Base controller providing common functionality for API controllers
    /// </summary>
    public abstract class BaseApiController : ControllerBase
    {
        /// <summary>
        /// Gets the current user's ID from JWT claims
        /// </summary>
        /// <returns>The user ID if valid, null otherwise</returns>
        protected int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim != null && int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return null;
        }

        /// <summary>
        /// Gets the current user's ID from JWT claims, returning an Unauthorized result if invalid
        /// </summary>
        /// <param name="userId">The parsed user ID</param>
        /// <returns>Unauthorized ActionResult if invalid, null if valid</returns>
        protected ActionResult? ValidateAndGetUserId(out int userId)
        {
            userId = 0;
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out userId))
            {
                return Unauthorized("Invalid user token");
            }
            return null;
        }
    }
}
