using Application.Services.Token;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Services.Token;

/// <summary>
/// Implementación JWT del servicio de tokens
/// Extensible para otros tipos de tokens sin modificar código existente
/// </summary>
public class JwtTokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(TokenClaims tokenClaims)
    {
        var secretKey = _configuration["JWT:SecretKey"] ?? "TaskManager2025-SecretKey-256bits-Required-For-HS256";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = BuildClaims(tokenClaims);

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:Issuer"] ?? "TaskManager",
            audience: _configuration["JWT:Audience"] ?? "TaskManagerUsers",
            claims: claims,
            expires: tokenClaims.Expiration ?? DateTime.UtcNow.AddHours(int.Parse(_configuration["JWT:ExpirationInHours"] ?? "24")),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = _configuration["JWT:SecretKey"] ?? "TaskManager2025-SecretKey-256bits-Required-For-HS256";
            var key = Encoding.UTF8.GetBytes(secretKey);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _configuration["JWT:Issuer"] ?? "TaskManager",
                ValidAudience = _configuration["JWT:Audience"] ?? "TaskManagerUsers",
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public TokenClaims? ExtractClaims(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);

            return new TokenClaims
            {
                UserId = jsonToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? "",
                Email = jsonToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value ?? "",
                Username = jsonToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value ?? "",
                CustomClaims = jsonToken.Claims
                    .Where(c => !IsStandardClaim(c.Type))
                    .ToDictionary(c => c.Type, c => c.Value)
            };
        }
        catch
        {
            return null;
        }
    }

    private static List<Claim> BuildClaims(TokenClaims tokenClaims)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, tokenClaims.UserId),
            new(ClaimTypes.Email, tokenClaims.Email),
            new(ClaimTypes.Name, tokenClaims.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        // Agregar claims personalizados
        claims.AddRange(tokenClaims.CustomClaims.Select(kvp => new Claim(kvp.Key, kvp.Value)));

        return claims;
    }

    private static bool IsStandardClaim(string claimType)
    {
        return claimType == ClaimTypes.NameIdentifier ||
               claimType == ClaimTypes.Email ||
               claimType == ClaimTypes.Name ||
               claimType == JwtRegisteredClaimNames.Jti ||
               claimType == JwtRegisteredClaimNames.Iat;
    }
}
