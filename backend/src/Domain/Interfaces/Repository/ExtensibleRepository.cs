using Domain.Entities;

namespace Domain.Interfaces.Repository;

/// <summary>
/// OCP: Base repository extensible para diferentes tipos de consultas
/// Abierto para extensión, cerrado para modificación
/// </summary>
public interface IQueryableRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(string id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<bool> ExistsAsync(string id);
}

/// <summary>
/// OCP: Repository para operaciones de escritura
/// </summary>
public interface IWritableRepository<T> where T : BaseEntity
{
    Task<T> CreateAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(string id);
}

/// <summary>
/// OCP: Repository extensible que combina lectura y escritura
/// </summary>
public interface IExtensibleRepository<T> : IQueryableRepository<T>, IWritableRepository<T> where T : BaseEntity
{
    // Permite extensiones específicas sin modificar las interfaces base
}
