using EazyRent.Data;
using EazyRent.Models.Domains;
using EazyRent.Models.DTO;
using EazyRent.Models.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EazyRent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly RentalDBContext _dbContext;
        private readonly IJwtTokenService _jwtTokenService;
        public UserController(RentalDBContext rentalDBContext, IJwtTokenService jwtTokenService)
        {
            _dbContext = rentalDBContext;
            _jwtTokenService = jwtTokenService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> RegisterOwner(RegistrationDTO dto)
        {
            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                PhoneNumber = dto.PhoneNumber,
                Role=dto.Role
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            return Ok("Owner registered");
        }

        [HttpPost("login")]
        public IActionResult Login(LoginDTO dto)
        {
            var user = _dbContext.Users.SingleOrDefault(t => t.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials");

            var token = _jwtTokenService.GenerateJwtToken(user.Email, user.Role, user.UserId);
            return Ok(new { token });
        }
    }
}
