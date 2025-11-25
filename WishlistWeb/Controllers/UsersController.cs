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
    public class UsersController(WishlistDbContext _context, IMapper _mapper) : ControllerBase
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UsersController));

        // GET: api/users
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserReadDto>>> GetUsers()
        {
            _logger.Info("Retrieving all users");
            var users = await _context.Users
                //.Select(u => mapper.Map<UserReadDto>(u)) 
                //inefficient as EF will map one-by-one

                .ProjectTo<UserReadDto>(_mapper.ConfigurationProvider)
                //more efficient as EF will translate mapping to SQL
                //avoids loading unnecessary properties
                .ToListAsync();

            _logger.Info($"Retrieved {users.Count} users");
            return users;
        }

        // GET: api/users/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserReadDto>> GetUser(int id)
        {
            _logger.Info($"Retrieving user with ID: {id}");
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                _logger.Warn($"User not found with ID: {id}");
                return NotFound();
            }

            _logger.Info($"Retrieved user: {user.Name} (ID: {id})");
            return _mapper.Map<UserReadDto>(user);
        }

        // GET: api/users/gifts/5
        [Authorize]
        [HttpGet("{userId}/wishlist")]
        public async Task<ActionResult<UserWishlistReadDto>> GetUsersGifts(int userId)
        {
            _logger.Info($"Retrieving wishlist for user ID: {userId}");
            var user = await _context.Users.Include(u => u.Gifts).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                _logger.Warn($"User not found with ID: {userId}");
                return NotFound();
            }

            _logger.Info($"Retrieved wishlist for user: {user.Name} with {user.Gifts.Count} gifts");
            return _mapper.Map<UserWishlistReadDto>(user);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<UserReadDto>> PutUser(int id, UserUpdateDto dto)
        {
            _logger.Info($"Updating user with ID: {id}");
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                _logger.Warn($"Update failed: User not found with ID: {id}");
                return NotFound();
            }

            // Map onto the existing tracked entity
            _mapper.Map(dto, user);

            await _context.SaveChangesAsync();

            var readDto = _mapper.Map<UserReadDto>(user);
            _logger.Info($"User updated successfully - ID: {id}");
            return Ok(readDto);
        }

        // DELETE: api/users/5
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            _logger.Info($"Deleting user with ID: {id}");
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                _logger.Warn($"Delete failed: User not found with ID: {id}");
                return NotFound();
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            _logger.Info($"User deleted successfully - ID: {id}");
            return NoContent();
        }
    }
}