using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Services;

public interface IJwtService
{
    string GenerateToken(string userId, string email, string username);
    string GenerateToken(string userId, string email, string username, string role);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(string userId, string email, string username)
    {
        return GenerateToken(userId, email, username, "User");
    }

    public string GenerateToken(string userId, string email, string username, string role)
    {
        var secretKey = _configuration["JWT:SecretKey"] ?? "TaskManager2025-SecretKey-256bits-Required-For-HS256";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:Issuer"] ?? "TaskManager",
            audience: _configuration["JWT:Audience"] ?? "TaskManagerUsers",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(int.Parse(_configuration["JWT:ExpirationInHours"] ?? "24")),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
