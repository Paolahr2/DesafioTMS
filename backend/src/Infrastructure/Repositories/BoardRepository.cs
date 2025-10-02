using Domain.Entities;
using Domain.Interfaces;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class BoardRepository : GenericRepository<Board>, Domain.Interfaces.BoardRepository
{
    public BoardRepository(IMongoDatabase database) : base(database, "boards")
    {
        // Crear índices
        CreateIndexes();
    }

    public async Task<IEnumerable<Board>> GetUserBoardsAsync(string userId)
    {
        if (_collection == null)
        {
            return new List<Board>();
        }
        // Usar filtro directo en lugar de expresiones LINQ para evitar problemas de serialización
        var filter = Builders<Board>.Filter.Or(
            Builders<Board>.Filter.Eq("OwnerId", userId),
            Builders<Board>.Filter.AnyEq("MemberIds", userId)
        );

        return await _collection.Find(filter).SortByDescending(x => x.UpdatedAt).ToListAsync();
    }

    public async Task<IEnumerable<Board>> GetPublicBoardsAsync(int page = 1, int pageSize = 10)
    {
        if (_collection == null)
        {
            return new List<Board>();
        }
        var filter = Builders<Board>.Filter.And(
            Builders<Board>.Filter.Eq(x => x.IsPublic, true),
            Builders<Board>.Filter.Eq(x => x.IsArchived, false)
        );

        return await _collection.Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Board>> GetBoardsByOwnerAsync(string ownerId)
    {
        if (_collection == null)
        {
            return new List<Board>();
        }
        return await _collection.Find(x => x.OwnerId == ownerId)
            .SortByDescending(x => x.UpdatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Board>> GetBoardsByOwnerIdAsync(string ownerId)
    {
        return await GetBoardsByOwnerAsync(ownerId);
    }

    public async Task<IEnumerable<Board>> GetBoardsByMemberIdAsync(string memberId)
    {
        if (_collection == null)
        {
            return new List<Board>();
        }
        return await _collection.Find(x => x.MemberIds.Contains(memberId))
            .SortByDescending(x => x.UpdatedAt)
            .ToListAsync();
    }

    public async Task<bool> UserHasAccessAsync(string boardId, string userId)
    {
        return await IsMemberAsync(boardId, userId);
    }

    public async Task<bool> AddMemberAsync(string boardId, string userId)
    {
        if (_collection == null)
        {
            return false;
        }
        var filter = Builders<Board>.Filter.Eq(x => x.Id, boardId);
        var update = Builders<Board>.Update
            .AddToSet(x => x.MemberIds, userId)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> RemoveMemberAsync(string boardId, string userId)
    {
        if (_collection == null)
        {
            return false;
        }
        var filter = Builders<Board>.Filter.Eq(x => x.Id, boardId);
        var update = Builders<Board>.Update
            .Pull(x => x.MemberIds, userId)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> IsMemberAsync(string boardId, string userId)
    {
        if (_collection == null)
        {
            return false;
        }
        var filter = Builders<Board>.Filter.And(
            Builders<Board>.Filter.Eq(x => x.Id, boardId),
            Builders<Board>.Filter.Or(
                Builders<Board>.Filter.Eq(x => x.OwnerId, userId),
                Builders<Board>.Filter.AnyEq(x => x.MemberIds, userId)
            )
        );

        var count = await _collection.CountDocumentsAsync(filter);
        return count > 0;
    }

    public async Task<IEnumerable<Board>> SearchBoardsAsync(string searchTerm, string? userId = null)
    {
        if (_collection == null)
        {
            return new List<Board>();
        }
        var searchFilter = Builders<Board>.Filter.Or(
            Builders<Board>.Filter.Regex(x => x.Title, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
            Builders<Board>.Filter.Regex(x => x.Description, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
        );

        FilterDefinition<Board> accessFilter;
        if (!string.IsNullOrEmpty(userId))
        {
            // Usuario puede ver: tableros públicos, propios o donde es miembro
            accessFilter = Builders<Board>.Filter.Or(
                Builders<Board>.Filter.Eq(x => x.IsPublic, true),
                Builders<Board>.Filter.Eq(x => x.OwnerId, userId),
                Builders<Board>.Filter.AnyEq(x => x.MemberIds, userId)
            );
        }
        else
        {
            // Solo tableros públicos para usuarios no autenticados
            accessFilter = Builders<Board>.Filter.Eq(x => x.IsPublic, true);
        }

        var combinedFilter = Builders<Board>.Filter.And(searchFilter, accessFilter);

        return await _collection.Find(combinedFilter)
            .SortByDescending(x => x.UpdatedAt)
            .Limit(20)
            .ToListAsync();
    }

    private void CreateIndexes()
    {
        try
        {
            // Obtener índices existentes
            var existingIndexes = _collection.Indexes.List().ToList();
            var indexNames = existingIndexes.Select(idx => idx["name"].AsString).ToList();

            // Crear índice para owner solo si no existe
            if (!indexNames.Any(name => name.Contains("owner") || name.Contains("Owner")))
            {
                var ownerIndexKeys = Builders<Board>.IndexKeys.Ascending(x => x.OwnerId);
                var ownerIndexOptions = new CreateIndexOptions { Name = "idx_board_owner" };
                var ownerIndexModel = new CreateIndexModel<Board>(ownerIndexKeys, ownerIndexOptions);
                _collection.Indexes.CreateOne(ownerIndexModel);
            }

            // Crear índice para tableros públicos solo si no existe
            if (!indexNames.Any(name => name.Contains("public") || name.Contains("Public")))
            {
                var publicIndexKeys = Builders<Board>.IndexKeys.Ascending(x => x.IsPublic);
                var publicIndexOptions = new CreateIndexOptions { Name = "idx_board_public" };
                var publicIndexModel = new CreateIndexModel<Board>(publicIndexKeys, publicIndexOptions);
                _collection.Indexes.CreateOne(publicIndexModel);
            }

            // Crear índice para miembros solo si no existe
            if (!indexNames.Any(name => name.Contains("member") || name.Contains("Member")))
            {
                var membersIndexKeys = Builders<Board>.IndexKeys.Ascending(x => x.MemberIds);
                var membersIndexOptions = new CreateIndexOptions { Name = "idx_board_members" };
                var membersIndexModel = new CreateIndexModel<Board>(membersIndexKeys, membersIndexOptions);
                _collection.Indexes.CreateOne(membersIndexModel);
            }

            // Crear índice de texto solo si no existe
            if (!indexNames.Any(name => name.Contains("text") || name.Contains("Text")))
            {
                var textIndexKeys = Builders<Board>.IndexKeys.Text(x => x.Title).Text(x => x.Description);
                var textIndexOptions = new CreateIndexOptions { Name = "idx_board_text" };
                var textIndexModel = new CreateIndexModel<Board>(textIndexKeys, textIndexOptions);
                _collection.Indexes.CreateOne(textIndexModel);
            }
        }
        catch (Exception)
        {
            // Si hay algún error creando índices, continuar sin fallar
            // Los índices se pueden crear manualmente en MongoDB si es necesario
        }
    }
}
