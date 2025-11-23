using AutoMapper;
using AutoMapper.QueryableExtensions;
using WishlistContracts.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WishlistModels;

namespace WishlistWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GiftController(WishlistDbContext _context, IMapper _mapper)
        : BaseApiController
    {

        // GET: api/gifts
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GiftReadDto>>> GetGifts()
        {
            return await _context.Gifts
                .ProjectTo<GiftReadDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        // GET: api/gifts/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<GiftReadDto>> GetGift(int id)
        {
            var gift = await _context.Gifts.FindAsync(id);

            if (gift == null)
            {
                return NotFound();
            }

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

            var gift = _mapper.Map<Gift>(giftDto);
            gift.UserId = userId; // Set the owner to the authenticated user
            _context.Gifts.Add(gift);
            await _context.SaveChangesAsync();

            var readDto = _mapper.Map<GiftReadDto>(gift);
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

            var gift = await _context.Gifts.FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);
            if (gift == null) return NotFound();

            // Map onto the existing tracked entity
            _mapper.Map(updateDto, gift);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Gifts.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
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
                return NotFound();
            }

            // Get the current user's ID from JWT claims
            var error = ValidateAndGetUserId(out int userId);
            if (error != null) return error;

            // Check if the user owns this gift
            if (gift.UserId != userId)
            {
                return Forbid(); // 403 Forbidden - user doesn't own this gift
            }

            _context.Gifts.Remove(gift);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
