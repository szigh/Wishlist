using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;

namespace WishlistWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController(WishlistDbContext context, IMapper _mapper)
        : ControllerBase
    {

        // GET: api/users
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
        [HttpGet("{id}")]
        public async Task<ActionResult<UserReadDto>> GetUser(int id)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return _mapper.Map<UserReadDto>(user);
        }

        // GET: api/users/gifts/5
        [HttpGet("gifts/{id}")]
        public async Task<ActionResult<UserReadDto>> GetUsersGifts(int id)
        {
            var user = await context.Users.Include(u => u.Gifts).FirstAsync(u => u.Id == id);
            if (user == null) return NotFound();
            return _mapper.Map<UserReadDto>(user);
        }

        // POST: api/users
        [HttpPost]
        public async Task<ActionResult<UserReadDto>> PostUser(UserCreateDto dto)
        {
            var user = _mapper.Map<User>(dto);
            var hasher = new PasswordHasher<UserCreateDto>();

            // Hash password before saving
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var readDto = _mapper.Map<UserReadDto>(user);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, readDto);
        }


        //TODO only allow admin to update users
        //[HttpPut("{id}")]
        //public async Task<ActionResult<UserReadDto>> PutUser(int id, UserUpdateDto dto)
        //{
        //    var user = await context.Users.FindAsync(id);
        //    if (user == null) return NotFound();

        //    // Map onto the existing tracked entity
        //    _mapper.Map(dto, user);

        //    await context.SaveChangesAsync();

        //    var readDto = _mapper.Map<UserReadDto>(user);
        //    return Ok(readDto);
        //}

        // DELETE: api/users/5
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