using AutoMapper;
using AutoMapper.QueryableExtensions;
using WishlistContracts.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WishlistModels;
using log4net;

namespace WishlistWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GiftDaysController(WishlistDbContext _context, IMapper _mapper) : BaseApiController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(GiftDaysController));

        // GET: api/giftdays - All users can see all gift days
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GiftDaysReadDto>>> GetGiftDays()
        {
            _logger.Info("Retrieving all gift days");
            var giftDays = await _context.GiftDays
                .Include(gd => gd.User)
                .ProjectTo<GiftDaysReadDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
            _logger.Info($"Retrieved {giftDays.Count} gift days");
            return giftDays;
        }

        // GET: api/giftdays/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<GiftDaysReadDto>> GetGiftDay(int id)
        {
            _logger.Info($"Retrieving gift day with ID: {id}");
            var giftDay = await _context.GiftDays
                .Include(gd => gd.User)
                .FirstOrDefaultAsync(gd => gd.Id == id);

            if (giftDay == null)
            {
                _logger.Warn($"Gift day not found with ID: {id}");
                return NotFound();
            }

            _logger.Info($"Retrieved gift day: {giftDay.Title} (ID: {id})");
            return _mapper.Map<GiftDaysReadDto>(giftDay);
        }

        // GET: api/giftdays/user/5 - Get gift days for a specific user
        [Authorize]
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<GiftDaysReadDto>>> GetGiftDaysByUser(int userId)
        {
            _logger.Info($"Retrieving gift days for user ID: {userId}");
            var giftDays = await _context.GiftDays
                .Include(gd => gd.User)
                .Where(gd => gd.UserId == userId)
                .ProjectTo<GiftDaysReadDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
            _logger.Info($"Retrieved {giftDays.Count} gift days for user ID: {userId}");
            return giftDays;
        }

        // POST: api/giftdays - Any authenticated user can create gift days for any user
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<GiftDaysReadDto>> PostGiftDay(GiftDaysCreateDto giftDayDto)
        {
            // Get the current user's ID from JWT claims
            var error = ValidateAndGetUserId(out int currentUserId);
            if (error != null) return error;

            // Validate day and month
            if (!IsValidDayMonth(giftDayDto.Day, giftDayDto.Month))
            {
                _logger.Warn($"Invalid day/month combination: {giftDayDto.Day}/{giftDayDto.Month}");
                return BadRequest("Invalid day/month combination");
            }

            // If no UserId is specified in the DTO, default to the current user
            int targetUserId = giftDayDto.UserId ?? currentUserId;

            _logger.Info($"User {currentUserId} creating gift day for user ID: {targetUserId}");

            // Verify the target user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == targetUserId);
            if (!userExists)
            {
                _logger.Warn($"Target user not found with ID: {targetUserId}");
                return BadRequest("Target user not found");
            }

            // If creating a protected gift day for another user, check authorization
            if (giftDayDto.Protected && targetUserId != currentUserId)
            {
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                bool isAdmin = userRole?.ToLower() == "admin";
                
                if (!isAdmin)
                {
                    _logger.Warn($"User {currentUserId} cannot create protected gift day for user {targetUserId}");
                    return Forbid();
                }
            }

            var giftDay = _mapper.Map<GiftDays>(giftDayDto);
            giftDay.UserId = targetUserId;
            _context.GiftDays.Add(giftDay);
            await _context.SaveChangesAsync();

            // Load the user for the response
            await _context.Entry(giftDay).Reference(gd => gd.User).LoadAsync();

            var readDto = _mapper.Map<GiftDaysReadDto>(giftDay);
            _logger.Info($"Gift day created successfully with ID: {giftDay.Id}");
            return CreatedAtAction(nameof(GetGiftDay), new { id = giftDay.Id }, readDto);
        }

        // PUT: api/giftdays/5 - Any authenticated user can update any gift day (unless protected)
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGiftDay(int id, GiftDaysUpdateDto updateDto)
        {
            // Get the current user's ID from JWT claims
            var error = ValidateAndGetUserId(out int currentUserId);
            if (error != null) return error;

            // Validate day and month
            if (!IsValidDayMonth(updateDto.Day, updateDto.Month))
            {
                _logger.Warn($"Invalid day/month combination: {updateDto.Day}/{updateDto.Month}");
                return BadRequest("Invalid day/month combination");
            }

            _logger.Info($"User {currentUserId} updating gift day with ID: {id}");

            var giftDay = await _context.GiftDays.FindAsync(id);
            if (giftDay == null)
            {
                _logger.Warn($"Gift day not found with ID: {id}");
                return NotFound();
            }

            // If the gift day is protected, only the owner or admin can update it
            if (giftDay.Protected && giftDay.UserId != currentUserId)
            {
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                bool isAdmin = userRole?.ToLower() == "admin";
                
                if (!isAdmin)
                {
                    _logger.Warn($"Update forbidden: Gift day {id} is protected and user {currentUserId} is not the owner");
                    return Forbid();
                }
            }

            // Map onto the existing tracked entity
            _mapper.Map(updateDto, giftDay);

            try
            {
                await _context.SaveChangesAsync();
                _logger.Info($"Gift day updated successfully - ID: {id}");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!_context.GiftDays.Any(e => e.Id == id))
                {
                    _logger.Error($"Gift day not found during update - ID: {id}", ex);
                    return NotFound();
                }
                else
                {
                    _logger.Error($"Concurrency error updating gift day - ID: {id}", ex);
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/giftdays/5 - Only owner or admin can delete (always)
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGiftDay(int id)
        {
            var giftDay = await _context.GiftDays.FindAsync(id);
            if (giftDay == null)
            {
                _logger.Warn($"Delete failed: Gift day not found - ID: {id}");
                return NotFound();
            }

            // Get the current user's ID and role from JWT claims
            var error = ValidateAndGetUserId(out int currentUserId);
            if (error != null) return error;

            // If the gift day is protected, only the owner or admin can delete it
            if (giftDay.Protected && giftDay.UserId != currentUserId)
            {
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                bool isAdmin = userRole?.ToLower() == "admin";
                
                if (!isAdmin)
                {
                    _logger.Warn($"Delete forbidden: Gift day {id} is protected and user {currentUserId} is not the owner");
                    return Forbid();
                }
            }
            // For non-protected gift days, still only owner or admin can delete
            else if (!giftDay.Protected && giftDay.UserId != currentUserId)
            {
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                bool isAdmin = userRole?.ToLower() == "admin";
                
                if (!isAdmin)
                {
                    _logger.Warn($"Delete forbidden: User {currentUserId} does not own gift day {id}");
                    return Forbid();
                }
            }

            _context.GiftDays.Remove(giftDay);
            await _context.SaveChangesAsync();

            _logger.Info($"Gift day deleted successfully - ID: {id}, User ID: {currentUserId}");
            return NoContent();
        }

        private static bool IsValidDayMonth(int day, int month)
        {
            if (month < 1 || month > 12) return false;
            if (day < 1 || day > 31) return false;

            // Days in each month (allowing Feb 29 for leap year birthdays)
            int[] daysInMonth = { 0, 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
            return day <= daysInMonth[month];
        }
    }
}
