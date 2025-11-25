using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WishlistContracts.DTOs;
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
            .FirstOrDefaultAsync(v => v.Id == id && v.VolunteerUserId == userId);

        if (volunteer == null) return NotFound(); // Could be not found OR not owned by user
        return volunteer;
    }

    // POST: api/volunteers
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Volunteer>> PostVolunteer(VolunteerCreateDto volunteer)
    {
        // Get the current user's ID from JWT claims
        var error = ValidateAndGetUserId(out int userId);
        if (error != null) return error;

        // Auto-set the volunteer user to the authenticated user

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

        Volunteer entity = new() { VolunteerUserId = userId, GiftId = gift.Id };
        context.Volunteers.Add(entity);

        // Mark gift as taken
        gift.IsTaken = true;

        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetVolunteer), new { id = entity.Id }, entity);
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