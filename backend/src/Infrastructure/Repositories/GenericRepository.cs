using Domain.Entities;
using Domain.Interfaces;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class GenericRepository<T> : Domain.Interfaces.GenericRepository<T> where T : BaseEntity
{
    protected readonly IMongoCollection<T> _collection;
    protected readonly IMongoDatabase _database;
    protected static readonly List<T> _inMemoryStore = new();

    public GenericRepository(IMongoDatabase database, string collectionName)
    {
        _database = database;
        if (database != null)
        {
            _collection = database.GetCollection<T>(collectionName);
        }
        else
        {
            _collection = null!;
        }
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        if (_collection != null)
        {
            Console.WriteLine($"[DEBUG] Creating entity of type {typeof(T).Name} in MongoDB");
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            await _collection.InsertOneAsync(entity);
            Console.WriteLine($"[DEBUG] Entity created with ID: {entity.Id}");
            return entity;
        }
        else
        {
            // Fallback to in-memory storage for development
            Console.WriteLine($"[DEBUG] Using in-memory storage for {typeof(T).Name}. Current store count: {_inMemoryStore.Count}");
            entity.Id = Guid.NewGuid().ToString();
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            _inMemoryStore.Add(entity);
            Console.WriteLine($"[DEBUG] Added {typeof(T).Name} with ID {entity.Id}. New store count: {_inMemoryStore.Count}");
            return entity;
        }
    }

    public virtual async Task<T?> GetByIdAsync(string id)
    {
        if (_collection != null)
        {
            return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }
        else
        {
            return _inMemoryStore.FirstOrDefault(x => x.Id == id);
        }
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        if (_collection != null)
        {
            return await _collection.Find(_ => true).ToListAsync();
        }
        else
        {
            return _inMemoryStore;
        }
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await _collection.ReplaceOneAsync(x => x.Id == entity.Id, entity);
        return entity;
    }

    public virtual async Task<bool> DeleteAsync(string id)
    {
        var result = await _collection.DeleteOneAsync(x => x.Id == id);
        return result.DeletedCount > 0;
    }

    public virtual async Task<IEnumerable<T>> FindAsync(FilterDefinition<T> filter)
    {
        return await _collection.Find(filter).ToListAsync();
    }

    public virtual async Task<long> CountAsync(FilterDefinition<T>? filter = null)
    {
        filter ??= Builders<T>.Filter.Empty;
        return await _collection.CountDocumentsAsync(filter);
    }

    public virtual async Task<bool> ExistsAsync(string id)
    {
        var count = await _collection.CountDocumentsAsync(x => x.Id == id);
        return count > 0;
    }
}
