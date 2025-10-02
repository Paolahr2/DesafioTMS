using Domain.Entities;

namespace Domain.Interfaces.Repository;

/// <summary>
/// LSP: Contrato base que debe ser respetado por todas las implementaciones
/// Las clases derivadas DEBEN ser substituibles sin romper funcionalidad
/// </summary>
public interface IBaseRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Obtiene una entidad por ID. Debe devolver null si no existe.
    /// CONTRATO: Las implementaciones NUNCA deben lanzar excepciones por entidad no encontrada
    /// </summary>
    Task<T?> GetByIdAsync(string id);
    
    /// <summary>
    /// Obtiene todas las entidades. Debe devolver lista vacía si no hay elementos.
    /// CONTRATO: Las implementaciones NUNCA deben devolver null
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();
    
    /// <summary>
    /// Verifica si existe una entidad por ID.
    /// CONTRATO: Las implementaciones NUNCA deben lanzar excepciones
    /// </summary>
    Task<bool> ExistsAsync(string id);
}

/// <summary>
/// LSP: Repository para operaciones de escritura con contratos específicos
/// </summary>
public interface IWriteRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Crea una nueva entidad.
    /// CONTRATO: Debe asignar ID único y fechas CreatedAt/UpdatedAt
    /// </summary>
    Task<T> CreateAsync(T entity);
    
    /// <summary>
    /// Actualiza una entidad existente.
    /// CONTRATO: Debe actualizar UpdatedAt, debe preservar CreatedAt
    /// </summary>
    Task<T> UpdateAsync(T entity);
    
    /// <summary>
    /// Elimina una entidad por ID.
    /// CONTRATO: Debe devolver true si se eliminó, false si no existía
    /// </summary>
    Task<bool> DeleteAsync(string id);
}
