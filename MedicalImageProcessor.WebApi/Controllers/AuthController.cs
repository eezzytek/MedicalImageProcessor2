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
    private static readonly ConcurrentDictionary<string, string> Users = new();  // ФІКС: In-memory users (username → hashed password)

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterDto dto)
    {
        if (string.IsNullOrEmpty(dto.Username) || string.IsNullOrEmpty(dto.Password))
            return BadRequest("Username and password required");

        if (Users.ContainsKey(dto.Username))
            return BadRequest("User already exists");

        // ФІКС: Зберігаємо hashed password (проста хеш, в production — BCrypt)
        var hashedPassword = SimpleHash(dto.Password);  // Заміни на реальний hash
        Users[dto.Username] = hashedPassword;

        return Ok(new { Token = GenerateToken(dto.Username) });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto dto)
    {
        if (string.IsNullOrEmpty(dto.Username) || string.IsNullOrEmpty(dto.Password))
            return BadRequest("Username and password required");

        if (!Users.TryGetValue(dto.Username, out var hashedPassword) || SimpleHash(dto.Password) != hashedPassword)
            return Unauthorized("Invalid credentials");  // ФІКС: Перевірка з hash

        return Ok(new { Token = GenerateToken(dto.Username) });
    }

    private string GenerateToken(string username)
    {
        var claims = new[] { new Claim(ClaimTypes.Name, username) };
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

public class LoginDto { public string Username { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
public class RegisterDto { public string Username { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }