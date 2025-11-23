using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using WishlistModels;

namespace WishlistWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VolunteersController(WishlistDbContext context) : BaseApiController
{
    // GET: api/volunteers
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Volunteer>>> GetVolunteers()
    {
        // Get the current user's ID from JWT claims
        var error = ValidateAndGetUserId(out int userId);
        if (error != null) return error;

        // Only return claims where the user is the volunteer
        return await context.Volunteers
            .Where(v => v.VolunteerUserId == userId)
            .Include(v => v.Gift)
            .Include(v => v.VolunteerUser)
            .ToListAsync();
    }

    // GET: api/volunteers/5
    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<Volunteer>> GetVolunteer(int id)
    {
        // Get the current user's ID from JWT claims
        var error = ValidateAndGetUserId(out int userId);
        if (error != null) return error;

        var volunteer = await context.Volunteers
            .Include(v => v.Gift)
            .Include(v => v.VolunteerUser)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (volunteer == null) return NotFound();

        // Verify the user owns this claim
        if (volunteer.VolunteerUserId != userId)
        {
            return Forbid(); // 403 Forbidden - user doesn't own this claim
        }

        return volunteer;
    }

    // POST: api/volunteers
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Volunteer>> PostVolunteer(Volunteer volunteer)
    {
        // Get the current user's ID from JWT claims
        var error = ValidateAndGetUserId(out int userId);
        if (error != null) return error;

        // Auto-set the volunteer user to the authenticated user
        volunteer.VolunteerUserId = userId;

        // Check if gift exists and is not already taken
        var gift = await context.Gifts.FindAsync(volunteer.GiftId);
        if (gift == null)
        {
            return NotFound("Gift not found");
        }
        if (gift.IsTaken)
        {
            return BadRequest("This gift has already been claimed");
        }

        context.Volunteers.Add(volunteer);

        // Mark gift as taken
        gift.IsTaken = true;

        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetVolunteer), new { id = volunteer.Id }, volunteer);
    }

    // DELETE: api/volunteers/5
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVolunteer(int id)
    {
        // Get the current user's ID from JWT claims
        var error = ValidateAndGetUserId(out int userId);
        if (error != null) return error;

        var volunteer = await context.Volunteers.FirstOrDefaultAsync(v => v.Id == id && v.VolunteerUserId == userId);
        if (volunteer == null) return NotFound();

        context.Volunteers.Remove(volunteer);

        // Reset gift status
        var gift = await context.Gifts.FindAsync(volunteer.GiftId);
        if (gift != null)
        {
            gift.IsTaken = false;
        }

        await context.SaveChangesAsync();
        return NoContent();
    }
    }
}