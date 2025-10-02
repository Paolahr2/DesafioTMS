namespace Application.DTOs.Users;

/// <summary>
/// DTO para actualización de usuario
/// </summary>
public class UpdateUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
