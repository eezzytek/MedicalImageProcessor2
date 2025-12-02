using Microsoft.AspNetCore.Mvc;
using Supabase;
using BCrypt.Net;
using MedicalImageProcessor.WebApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace MedicalImageProcessor.WebApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly Supabase.Client _supabase;
        private readonly IConfiguration _config;

        public AuthController(Supabase.Client supabase, IConfiguration config)
        {
            _supabase = supabase;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var existing = await _supabase
                .From<UserRecord>()
                .Where(x => x.Username == dto.Username)
                .Single();

            if (existing != null)
                return BadRequest("Користувач вже існує");

            var user = new UserRecord
            {
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password) // Тепер працює!
            };

            await _supabase.From<UserRecord>().Insert(user);

            var token = GenerateJwt(user.Id.ToString(), dto.Username);
            return Ok(new { token });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _supabase
                .From<UserRecord>()
                .Where(x => x.Username == dto.Username)
                .Single();

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)) // Тепер працює!
                return BadRequest("Неправильний логін або пароль");

            var token = GenerateJwt(user.Id.ToString(), dto.Username);
            return Ok(new { token });
        }

        private string GenerateJwt(string userId, string username)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, username)
            };

            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class RegisterDto
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class LoginDto
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}