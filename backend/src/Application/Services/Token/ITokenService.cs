namespace Application.Services.Token;

/// <summary>
/// Interfaz base para cualquier servicio de generación de tokens
/// Abierto para extensión, cerrado para modificación
/// </summary>
public interface ITokenService
{
    string GenerateToken(TokenClaims claims);
    bool ValidateToken(string token);
    TokenClaims? ExtractClaims(string token);
}

/// <summary>
/// Claims genéricos para tokens - Extensible
/// </summary>
public class TokenClaims
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public Dictionary<string, string> CustomClaims { get; set; } = new();
    public DateTime? Expiration { get; set; }
}
