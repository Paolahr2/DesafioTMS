using Domain.Entities;
using Domain.Interfaces.Users;
using Domain.Interfaces.Repository;
using Infrastructure.Repositories.Base;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

/// <summary>
/// LSP: UserRepository que es completamente substituible por BaseMongoRepository
/// Respeta todos los contratos de la clase base y interfaces
/// DIP: Depende de abstracciones (interfaces) no de implementaciones
/// </summary>
public class LSPCompliantUserRepository : BaseMongoRepository<User>, 
    IUserEmailQueries, 
    IUserUsernameQueries, 
    IUserSearchQueries
{
    public LSPCompliantUserRepository(IMongoDatabase database) : base(database, "users")
    {
        _ = Task.Run(CreateIndexesAsync);
    }

    // LSP: Mantiene comportamiento de clase base - nunca lanza excepciones
    public async Task<User?> GetByEmailAsync(string email)
    {
        if (_collection == null)
        {
            return null;
        }
        try
        {
            return await _collection
                .Find(x => x.Email.ToLower() == email.ToLower())
                .FirstOrDefaultAsync();
        }
        catch
        {
            return null; // LSP: Respeta contrato
        }
    }

    // LSP: Mantiene comportamiento de clase base
    public async Task<User?> GetByUsernameAsync(string username)
    {
        if (_collection == null)
        {
            return null;
        }
        try
        {
            return await _collection
                .Find(x => x.Username.ToLower() == username.ToLower())
                .FirstOrDefaultAsync();
        }
        catch
        {
            return null; // LSP: Respeta contrato
        }
    }

    // ISP: Método específico de interface segregada
    public async Task<bool> EmailExistsAsync(string email)
    {
        if (_collection == null)
        {
            return false;
        }
        try
        {
            var count = await _collection
                .CountDocumentsAsync(x => x.Email.ToLower() == email.ToLower());
            return count > 0;
        }
        catch
        {
            return false; // LSP: Respeta contrato
        }
    }

    // ISP: Método específico de interface segregada
    public async Task<bool> UsernameExistsAsync(string username)
    {
        if (_collection == null)
        {
            return false;
        }
        try
        {
            var count = await _collection
                .CountDocumentsAsync(x => x.Username.ToLower() == username.ToLower());
            return count > 0;
        }
        catch
        {
            return false; // LSP: Respeta contrato
        }
    }

    // ISP: Interface segregada para búsquedas
    public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, int limit = 10)
    {
        if (_collection == null)
        {
            return new List<User>();
        }
        try
        {
            var filter = Builders<User>.Filter.Or(
                Builders<User>.Filter.Regex(x => x.Username, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<User>.Filter.Regex(x => x.FirstName, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<User>.Filter.Regex(x => x.LastName, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
            );

            return await _collection
                .Find(filter)
                .Limit(limit)
                .ToListAsync();
        }
        catch
        {
            return new List<User>(); // LSP: Respeta contrato
        }
    }

    // ISP: Interface segregada para consultas específicas
    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        if (_collection == null)
        {
            return new List<User>();
        }
        try
        {
            return await _collection
                .Find(x => x.IsActive)
                .ToListAsync();
        }
        catch
        {
            return new List<User>(); // LSP: Respeta contrato
        }
    }

    private async Task CreateIndexesAsync()
    {
        if (_collection == null)
        {
            return;
        }
        try
        {
            // Crear índices de manera asíncrona sin bloquear constructor
            await Task.Run(async () =>
            {
                await _collection.Indexes.CreateOneAsync(
                    new CreateIndexModel<User>(Builders<User>.IndexKeys.Ascending(x => x.Email))
                );
                
                await _collection.Indexes.CreateOneAsync(
                    new CreateIndexModel<User>(Builders<User>.IndexKeys.Ascending(x => x.Username))
                );
            });
        }
        catch
        {
            // LSP: No lanza excepciones desde constructor
        }
    }
}
