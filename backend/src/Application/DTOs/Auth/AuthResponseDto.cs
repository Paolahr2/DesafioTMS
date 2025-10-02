using Application.DTOs.Users;

namespace Application.DTOs.Auth;

/// <summary>
/// DTO para respuesta de autenticación (login/register)
/// </summary>
public class AuthResponseDto
{
    public bool Success { get; set; }
    public string Token { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public UserDto? User { get; set; }
}
