using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth;

/// <summary>
/// DTO para solicitud de login de usuario
/// </summary>
public class LoginRequestDto
{
    [Required(ErrorMessage = "El email o username es requerido")]
    public string EmailOrUsername { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida")]
    public string Password { get; set; } = string.Empty;
}
