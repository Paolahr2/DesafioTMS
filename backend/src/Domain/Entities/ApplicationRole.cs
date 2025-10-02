using AspNetCore.Identity.MongoDbCore.Models;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

/// <summary>
/// Rol de aplicación que extiende MongoIdentityRole para ASP.NET Identity con MongoDB
/// </summary>
public class ApplicationRole : MongoIdentityRole<string>
{
    /// <summary>
    /// Descripción del rol
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Constructor por defecto
    /// </summary>
    public ApplicationRole() { }

    /// <summary>
    /// Constructor con nombre del rol
    /// </summary>
    public ApplicationRole(string roleName) : base(roleName)
    {
        NormalizedName = roleName.ToUpper();
    }

    /// <summary>
    /// Constructor con nombre y descripción
    /// </summary>
    public ApplicationRole(string roleName, string? description) : base(roleName)
    {
        Description = description;
        NormalizedName = roleName.ToUpper();
    }
}