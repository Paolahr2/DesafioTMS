using Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

// Usuario del sistema con información de autenticación y perfil
[BsonIgnoreExtraElements]
public class User : BaseEntity
{
    [BsonElement("firstName")]
    public string FirstName { get; set; } = string.Empty;
    
    [BsonElement("lastName")]
    public string LastName { get; set; } = string.Empty;
    
    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;
    
    [BsonElement("userName")]
    public string Username { get; set; } = string.Empty;
    
    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;
    
    [BsonElement("role")]
    public UserRole Role { get; set; } = UserRole.User;
    
    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;
    
    [BsonElement("emailConfirmed")]
    public bool EmailConfirmed { get; set; } = false;
    
    public string? Avatar { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Para navegación fácil
    public string FullName => $"{FirstName} {LastName}";
}
