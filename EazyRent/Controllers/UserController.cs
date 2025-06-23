using AutoMapper;
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
        private readonly IMapper mapper;

        public UserController(RentalDBContext rentalDBContext, IJwtTokenService jwtTokenService,IMapper mapper)
        {
            _dbContext = rentalDBContext;
            _jwtTokenService = jwtTokenService;
            this.mapper = mapper;
        }
        [HttpPost("register")]
        public async Task<IActionResult> RegisterOwner(RegistrationDTO dto)
        {
            try
            {
                var user = mapper.Map<User>(dto);

                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();

                if (user.Role == "Owner")
                {
                    return Ok(new { message = "Owner registered", user });
                }
                else if (user.Role == "Tenant")
                {
                    return Ok(new { message = "Tenant registered", user });
                }
                else
                {
                    throw new Exception("Invalid role");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred during registration.", Details = ex.Message });
            }
        }

        [HttpPost("login")]
        public IActionResult Login(LoginDTO dto)
        {
            var user = _dbContext.Users.SingleOrDefault(t => t.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid credentials" });

            var token = _jwtTokenService.GenerateJwtToken(user.Email, user.Role, user.UserId, user.FullName); 
            return Ok(new { token });
        }
    }
}
