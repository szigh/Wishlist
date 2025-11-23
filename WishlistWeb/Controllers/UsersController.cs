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
    public class UsersController(WishlistDbContext context, IMapper _mapper)
        : ControllerBase
    {
        // GET: api/users
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserReadDto>>> GetUsers()
        {
            return await context.Users

                //.Select(u => mapper.Map<UserReadDto>(u)) 
                //inefficient as EF will map one-by-one

                .ProjectTo<UserReadDto>(_mapper.ConfigurationProvider)
                //more efficient as EF will translate mapping to SQL
                //avoids loading unnecessary properties
                .ToListAsync();
        }

        // GET: api/users/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserReadDto>> GetUser(int id)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return _mapper.Map<UserReadDto>(user);
        }

        // GET: api/users/gifts/5
        [Authorize]
        [HttpGet("{userId}/wishlist")]
        public async Task<ActionResult<UserWishlistReadDto>> GetUsersGifts(int userId)
        {
            var user = await context.Users.Include(u => u.Gifts).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();
            return _mapper.Map<UserWishlistReadDto>(user);
        }


        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<UserReadDto>> PutUser(int id, UserUpdateDto dto)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null) return NotFound();

            // Map onto the existing tracked entity
            _mapper.Map(dto, user);

            await context.SaveChangesAsync();

            var readDto = _mapper.Map<UserReadDto>(user);
            return Ok(readDto);
        }

        // DELETE: api/users/5
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null) return NotFound();
            context.Users.Remove(user);
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}