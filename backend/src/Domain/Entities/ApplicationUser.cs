using AspNetCore.Identity.MongoDbCore.Models;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

/// <summary>
/// Usuario de aplicación que extiende MongoIdentityUser para ASP.NET Identity con MongoDB
/// </summary>
public class ApplicationUser : MongoIdentityUser<string>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsActive { get; set; } = true;
    public string? Avatar { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Propiedades de auditoría (equivalentes a BaseEntity)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Propiedad de navegación para compatibilidad
    [BsonIgnore]
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Constructor por defecto
    /// </summary>
    public ApplicationUser() { }

    /// <summary>
    /// Constructor con parámetros básicos
    /// </summary>
    public ApplicationUser(string userName, string email, string firstName, string lastName)
    {
        UserName = userName;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        NormalizedUserName = userName.ToUpper();
        NormalizedEmail = email.ToUpper();
        IsActive = true;
    }
}