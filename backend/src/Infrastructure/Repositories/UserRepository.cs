using Domain.Entities;
using Domain.Interfaces;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, Domain.Interfaces.UserRepository
{
    public override async Task<User?> GetByIdAsync(string id)
    {
        if (_collection != null)
        {
            var filter = Builders<User>.Filter.Eq("_id", id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
        else
        {
            Console.WriteLine($"[DEBUG] Searching for user by id '{id}' in memory store. Store count: {_inMemoryStore.Count}");
            var user = _inMemoryStore.FirstOrDefault(x => x.Id == id);
            Console.WriteLine($"[DEBUG] Found user: {user?.Username ?? "null"}");
            return user;
        }
    }

    public UserRepository(IMongoDatabase database) : base(database, "applicationUsers")
    {
        Console.WriteLine($"[DEBUG] UserRepository created. Database is null: {database == null}");
        // Crear índices
        CreateIndexes();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        if (_collection != null)
        {
            var filter = Builders<User>.Filter.Eq("email", email.ToLower());
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
        else
        {
            Console.WriteLine($"[DEBUG] Searching for user by email '{email}' in memory store. Store count: {_inMemoryStore.Count}");
            var user = _inMemoryStore.FirstOrDefault(x => x.Email.ToLower() == email.ToLower());
            Console.WriteLine($"[DEBUG] Found user: {user?.Username ?? "null"}");
            return user;
        }
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        if (_collection != null)
        {
            var filter = Builders<User>.Filter.Eq("userName", username.ToLower());
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
        else
        {
            Console.WriteLine($"[DEBUG] Searching for user by username '{username}' in memory store. Store count: {_inMemoryStore.Count}");
            var user = _inMemoryStore.FirstOrDefault(x => x.Username.ToLower() == username.ToLower());
            Console.WriteLine($"[DEBUG] Found user: {user?.Username ?? "null"}");
            return user;
        }
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        if (_collection != null)
        {
            var count = await _collection.CountDocumentsAsync(x => x.Email.ToLower() == email.ToLower());
            return count > 0;
        }
        else
        {
            return _inMemoryStore.Any(x => x.Email.ToLower() == email.ToLower());
        }
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        if (_collection != null)
        {
            var count = await _collection.CountDocumentsAsync(x => x.Username.ToLower() == username.ToLower());
            return count > 0;
        }
        else
        {
            return _inMemoryStore.Any(x => x.Username.ToLower() == username.ToLower());
        }
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        if (_collection == null)
        {
            return new List<User>();
        }
        return await _collection.Find(x => x.IsActive).ToListAsync();
    }

    public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, int limit = 10)
    {
        if (_collection == null)
        {
            return new List<User>();
        }
        var filter = Builders<User>.Filter.Or(
            Builders<User>.Filter.Regex(x => x.Username, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
            Builders<User>.Filter.Regex(x => x.Email, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
            Builders<User>.Filter.Regex(x => x.FirstName, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
            Builders<User>.Filter.Regex(x => x.LastName, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
        );

        return await _collection.Find(filter).Limit(limit).ToListAsync();
    }

    private void CreateIndexes()
    {
        try
        {
            // Obtener índices existentes
            var existingIndexes = _collection.Indexes.List().ToList();
            var indexNames = existingIndexes.Select(idx => idx["name"].AsString).ToList();

            // Crear índice para email solo si no existe
            if (!indexNames.Any(name => name.Contains("email") || name.Contains("Email")))
            {
                var emailIndexKeys = Builders<User>.IndexKeys.Ascending(x => x.Email);
                var emailIndexOptions = new CreateIndexOptions { 
                    Unique = true,
                    Name = "idx_user_email_unique"
                };
                var emailIndexModel = new CreateIndexModel<User>(emailIndexKeys, emailIndexOptions);
                _collection.Indexes.CreateOne(emailIndexModel);
            }

            // Crear índice para username solo si no existe
            if (!indexNames.Any(name => name.Contains("username") || name.Contains("Username")))
            {
                var usernameIndexKeys = Builders<User>.IndexKeys.Ascending(x => x.Username);
                var usernameIndexOptions = new CreateIndexOptions { 
                    Unique = true,
                    Name = "idx_user_username_unique"
                };
                var usernameIndexModel = new CreateIndexModel<User>(usernameIndexKeys, usernameIndexOptions);
                _collection.Indexes.CreateOne(usernameIndexModel);
            }
        }
        catch (Exception)
        {
            // Si hay algún error creando índices, continuar sin fallar
            // Los índices se pueden crear manualmente en MongoDB si es necesario
        }
    }
}
