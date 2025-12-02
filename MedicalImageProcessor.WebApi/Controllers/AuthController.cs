using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Collections.Concurrent;  // Для ConcurrentDictionary

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private static readonly ConcurrentDictionary<string, UserRecord> Users = new();  // ФІКС: In-memory users (username → hashed password)

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterDto dto)
    {
        if (string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
            return BadRequest("Username and password required");

        if (Users.ContainsKey(dto.Email))
            return BadRequest("User already exists");

        var userId = Guid.NewGuid().ToString();
        // ФІКС: Зберігаємо hashed password (проста хеш, в production — BCrypt)
        var hashedPassword = SimpleHash(dto.Password);  // Заміни на реальний hash
        Users[dto.Email] = new UserRecord(userId, hashedPassword);

        return Ok(new { Token = GenerateToken(dto.Email, userId) });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto dto)
    {
        if (!Users.TryGetValue(dto.Email, out var user))
            return Unauthorized("Invalid credentials");

        if (SimpleHash(dto.Password) != user.HashedPassword)
            return Unauthorized("Invalid credentials");

        return Ok(new { Token = GenerateToken(dto.Email, user.UserId) });
    }

    private string GenerateToken(string username, string userId)
    {
        
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.NameIdentifier, userId)
        };
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "your-secret-key-min-32-chars"));
        
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "MedicalAPI",
            audience: "Users",
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Проста хеш (для демо; в production — BCrypt.Net)
    private static string SimpleHash(string password)
    {
        return Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(password)));
    }
}

public class LoginDto { public string Email { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
public class RegisterDto { public string Email { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
public record UserRecord (string UserId, string HashedPassword);