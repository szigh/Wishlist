using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;

[ApiController]
[Route("api/[controller]")]
public class VolunteersController(WishlistDbContext context) : ControllerBase
{
    // GET: api/volunteers
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Volunteer>>> GetVolunteers()
    {
        return await context.Volunteers
            .Include(v => v.Gift)
            .Include(v => v.VolunteerUser)
            .ToListAsync();
    }

    // GET: api/volunteers/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Volunteer>> GetVolunteer(int id)
    {
        var volunteer = await context.Volunteers
            .Include(v => v.Gift)
            .Include(v => v.VolunteerUser)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (volunteer == null) return NotFound();
        return volunteer;
    }

    // POST: api/volunteers
    [HttpPost]
    public async Task<ActionResult<Volunteer>> PostVolunteer(Volunteer volunteer)
    {
        context.Volunteers.Add(volunteer);

        // Mark gift as taken
        var gift = await context.Gifts.FindAsync(volunteer.GiftId);
        gift?.IsTaken = true;

        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetVolunteer), new { id = volunteer.Id }, volunteer);
    }

    // DELETE: api/volunteers/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVolunteer(int id)
    {
        var volunteer = await context.Volunteers.FindAsync(id);
        if (volunteer == null) return NotFound();

        context.Volunteers.Remove(volunteer);

        // Reset gift status if needed
        var gift = await context.Gifts.FindAsync(volunteer.GiftId);
        gift?.IsTaken = false;

        await context.SaveChangesAsync();
        return NoContent();
    }
}