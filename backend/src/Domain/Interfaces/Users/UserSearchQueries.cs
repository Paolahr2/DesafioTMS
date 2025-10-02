using Domain.Entities;

namespace Domain.Interfaces.Users;

/// <summary>
/// ISP: Interface específica para búsquedas de usuarios
/// </summary>
public interface IUserSearchQueries
{
    Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, int limit = 10);
    Task<IEnumerable<User>> GetActiveUsersAsync();
}
