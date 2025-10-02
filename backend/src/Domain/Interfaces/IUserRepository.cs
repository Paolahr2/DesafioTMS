using Domain.Entities;

namespace Domain.Interfaces;

// Repositorio específico para usuarios con métodos de consulta adicionales
public interface IUserRepository : GenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, int limit = 10);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> UsernameExistsAsync(string username);
}