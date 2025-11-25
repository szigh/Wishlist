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
    public class GiftController(WishlistDbContext _context, IMapper _mapper) : BaseApiController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(GiftController));

        // GET: api/gifts
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GiftReadDto>>> GetGifts()
        {
            _logger.Info("Retrieving all gifts");
            var gifts = await _context.Gifts
                .ProjectTo<GiftReadDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
            _logger.Info($"Retrieved {gifts.Count} gifts");
            return gifts;
        }

        // GET: api/gifts/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<GiftReadDto>> GetGift(int id)
        {
            _logger.Info($"Retrieving gift with ID: {id}");
            var gift = await _context.Gifts.FindAsync(id);

            if (gift == null)
            {
                _logger.Warn($"Gift not found with ID: {id}");
                return NotFound();
            }

            _logger.Info($"Retrieved gift: {gift.Title} (ID: {id})");
            return _mapper.Map<GiftReadDto>(gift);
        }

        // POST: api/gifts
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<GiftReadDto>> PostGift(GiftCreateDto giftDto)
        {
            // Get the current user's ID from JWT claims
            var error = ValidateAndGetUserId(out int userId);
            if (error != null) return error;

            _logger.Info($"Creating new gift: {giftDto.Title} for user ID: {userId}");

            var gift = _mapper.Map<Gift>(giftDto);
            gift.UserId = userId; // Set the owner to the authenticated user
            _context.Gifts.Add(gift);
            await _context.SaveChangesAsync();

            var readDto = _mapper.Map<GiftReadDto>(gift);
            _logger.Info($"Gift created successfully with ID: {gift.Id}");
            return CreatedAtAction(nameof(GetGift), new { id = gift.Id }, readDto);
        }

        // PUT: api/gifts/5
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGift(int id, GiftUpdateDto updateDto)
        {
            // Get the current user's ID from JWT claims
            var error = ValidateAndGetUserId(out int userId);
            if (error != null) return error;

            _logger.Info($"Updating gift with ID: {id} for user ID: {userId}");

            var gift = await _context.Gifts.FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);
            if (gift == null)
            {
                _logger.Warn($"Gift not found or unauthorized - ID: {id}, User ID: {userId}");
                return NotFound();
            }

            // Map onto the existing tracked entity
            _mapper.Map(updateDto, gift);

            try
            {
                await _context.SaveChangesAsync();
                _logger.Info($"Gift updated successfully - ID: {id}");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!_context.Gifts.Any(e => e.Id == id))
                {
                    _logger.Error($"Gift not found during update - ID: {id}", ex);
                    return NotFound();
                }
                else
                {
                    _logger.Error($"Concurrency error updating gift - ID: {id}", ex);
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/gifts/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGift(int id)
        {
            var gift = await _context.Gifts.FindAsync(id);
            if (gift == null)
            {
                _logger.Warn($"Delete failed: Gift not found - ID: {id}");
                return NotFound();
            }

            // Get the current user's ID from JWT claims
            var error = ValidateAndGetUserId(out int userId);
            if (error != null) return error;

            // Check if the user owns this gift
            if (gift.UserId != userId)
            {
                _logger.Warn($"Delete forbidden: User {userId} does not own gift {id}");
                return Forbid(); // 403 Forbidden - user doesn't own this gift
            }

            _context.Gifts.Remove(gift);
            await _context.SaveChangesAsync();

            _logger.Info($"Gift deleted successfully - ID: {id}, User ID: {userId}");
            return NoContent();
        }
    }
}
