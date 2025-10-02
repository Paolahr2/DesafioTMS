using Domain.Entities;

namespace Domain.Interfaces.Users;

/// <summary>
/// ISP: Interface específica para consultas por username
/// </summary>
public interface IUserUsernameQueries
{
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> UsernameExistsAsync(string username);
}
