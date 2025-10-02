using Domain.Entities;

namespace Domain.Interfaces.Users;

/// <summary>
/// ISP: Interface específica para consultas por email
/// </summary>
public interface IUserEmailQueries
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
}
