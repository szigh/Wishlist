using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class VolunteersController : ControllerBase
{
    private readonly WishlistDbContext _context;

    public VolunteersController(WishlistDbContext context)
    {
        _context = context;
    }

    // GET: api/volunteers
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Volunteer>>> GetVolunteers()
    {
        return await _context.Volunteers
            .Include(v => v.Gift)
            .Include(v => v.VolunteerUser)
            .ToListAsync();
    }

    // GET: api/volunteers/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Volunteer>> GetVolunteer(int id)
    {
        var volunteer = await _context.Volunteers
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
        _context.Volunteers.Add(volunteer);

        // Mark gift as taken
        var gift = await _context.Gifts.FindAsync(volunteer.GiftId);
        if (gift != null) gift.IsTaken = true;

        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetVolunteer), new { id = volunteer.Id }, volunteer);
    }

    // DELETE: api/volunteers/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVolunteer(int id)
    {
        var volunteer = await _context.Volunteers.FindAsync(id);
        if (volunteer == null) return NotFound();

        _context.Volunteers.Remove(volunteer);

        // Reset gift status if needed
        var gift = await _context.Gifts.FindAsync(volunteer.GiftId);
        if (gift != null) gift.IsTaken = false;

        await _context.SaveChangesAsync();
        return NoContent();
    }
}