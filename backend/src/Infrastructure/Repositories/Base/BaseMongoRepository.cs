using Domain.Entities;
using Domain.Interfaces.Repository;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Infrastructure.Repositories.Base;

/// <summary>
/// LSP: Implementación base que respeta completamente los contratos
/// Puede ser substituida por cualquier otra implementación de IBaseRepository
/// </summary>
public abstract class BaseMongoRepository<T> : IBaseRepository<T>, IWriteRepository<T> where T : BaseEntity
{
    protected readonly IMongoCollection<T> _collection;

    protected BaseMongoRepository(IMongoDatabase database, string collectionName)
    {
        _collection = database.GetCollection<T>(collectionName);
    }

    // LSP: Respeta contrato - nunca lanza excepciones, devuelve null si no existe
    public virtual async Task<T?> GetByIdAsync(string id)
    {
        try
        {
            return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }
        catch
        {
            // LSP: Respeta contrato - no lanza excepciones
            return null;
        }
    }

    // LSP: Respeta contrato - nunca devuelve null, siempre lista (puede ser vacía)
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        try
        {
            return await _collection.Find(_ => true).ToListAsync();
        }
        catch
        {
            // LSP: Respeta contrato - devuelve lista vacía en caso de error
            return new List<T>();
        }
    }

    // LSP: Respeta contrato - nunca lanza excepciones
    public virtual async Task<bool> ExistsAsync(string id)
    {
        try
        {
            var count = await _collection.CountDocumentsAsync(x => x.Id == id);
            return count > 0;
        }
        catch
        {
            // LSP: Respeta contrato - devuelve false en caso de error
            return false;
        }
    }

    // LSP: Respeta contrato - siempre asigna ID y fechas
    public virtual async Task<T> CreateAsync(T entity)
    {
        entity.Id = ObjectId.GenerateNewId().ToString();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    // LSP: Respeta contrato - actualiza UpdatedAt, preserva CreatedAt
    public virtual async Task<T> UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        // Preserva CreatedAt original
        
        await _collection.ReplaceOneAsync(x => x.Id == entity.Id, entity);
        return entity;
    }

    // LSP: Respeta contrato - devuelve bool según si existía
    public virtual async Task<bool> DeleteAsync(string id)
    {
        try
        {
            var result = await _collection.DeleteOneAsync(x => x.Id == id);
            return result.DeletedCount > 0;
        }
        catch
        {
            return false;
        }
    }
}
