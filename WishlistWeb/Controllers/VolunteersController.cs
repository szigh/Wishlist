using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WishlistModels;
using log4net;
using WishlistContracts.DTOs;

namespace WishlistWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VolunteersController(WishlistDbContext _context) : BaseApiController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(VolunteersController));

        // GET: api/volunteers
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Volunteer>>> GetVolunteers()
        {
            // Get the current user's ID from JWT claims
            var error = ValidateAndGetUserId(out int userId);
            if (error != null) return error;

            _logger.Info($"Retrieving volunteers for user ID: {userId}");

            // Only return claims where the user is the volunteer
            var volunteers = await _context.Volunteers
                .Where(v => v.VolunteerUserId == userId)
                .Include(v => v.Gift)
                .Include(v => v.VolunteerUser)
                .ToListAsync();

            _logger.Info($"Retrieved {volunteers.Count} volunteers for user ID: {userId}");
            return volunteers;
        }

        // GET: api/volunteers/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Volunteer>> GetVolunteer(int id)
        {
            // Get the current user's ID from JWT claims
            var error = ValidateAndGetUserId(out int userId);
            if (error != null) return error;

            _logger.Info($"Retrieving volunteer with ID: {id} for user ID: {userId}");

            var volunteer = await _context.Volunteers
                .Include(v => v.Gift)
                .Include(v => v.VolunteerUser)
                .FirstOrDefaultAsync(v => v.Id == id && v.VolunteerUserId == userId);

            if (volunteer == null)
            {
                _logger.Warn($"Volunteer not found or unauthorized - ID: {id}, User ID: {userId}");
                return NotFound(); // Could be not found OR not owned by user
            }

            _logger.Info($"Retrieved volunteer ID: {id}");
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

            _logger.Info($"Creating volunteer for gift ID: {volunteer.GiftId}, user ID: {userId}");

            // Check if gift exists and is not already taken
            var gift = await _context.Gifts.FindAsync(volunteer.GiftId);
            if (gift == null)
            {
                _logger.Warn($"Gift not found - ID: {volunteer.GiftId}");
                return NotFound("Gift not found");
            }
            if (gift.IsTaken)
            {
                _logger.Warn($"Gift already claimed - ID: {volunteer.GiftId}");
                return BadRequest("This gift has already been claimed");
            }

            Volunteer entity = new() { GiftId = volunteer.GiftId, VolunteerUserId = userId };
            _context.Volunteers.Add(entity);

            // Mark gift as taken
            gift.IsTaken = true;

            try
            {
                await _context.SaveChangesAsync();
                _logger.Info($"Volunteer created successfully - ID: {entity.Id}, Gift ID: {entity.GiftId}");
                return CreatedAtAction(nameof(GetVolunteer), new { id = entity.Id }, entity);
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.Warn($"Gift was claimed concurrently - ID: {volunteer.GiftId}");
                return BadRequest("This gift has already been claimed by someone else.");
            }
        }

        // DELETE: api/volunteers/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVolunteer(int id)
        {
            // Get the current user's ID from JWT claims
            var error = ValidateAndGetUserId(out int userId);
            if (error != null) return error;

            _logger.Info($"Deleting volunteer with ID: {id} for user ID: {userId}");

            var volunteer = await _context.Volunteers.FirstOrDefaultAsync(v => v.Id == id && v.VolunteerUserId == userId);
            if (volunteer == null)
            {
                _logger.Warn($"Delete failed: Volunteer not found or unauthorized - ID: {id}, User ID: {userId}");
                return NotFound();
            }

            _context.Volunteers.Remove(volunteer);

            // Reset gift status
            var gift = await _context.Gifts.FindAsync(volunteer.GiftId);
            if (gift != null)
            {
                gift.IsTaken = false;
            }

            await _context.SaveChangesAsync();
            _logger.Info($"Volunteer deleted successfully - ID: {id}, Gift ID: {volunteer.GiftId}");
            return NoContent();
        }
    }
}