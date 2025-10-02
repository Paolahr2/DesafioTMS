using Domain.Entities;
using Domain.Interfaces;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class ListRepository : GenericRepository<List>, Domain.Interfaces.ListRepository
{
    public ListRepository(IMongoDatabase database) : base(database, "lists")
    {
        // Crear índices
        CreateIndexes();
    }

    public async Task<IEnumerable<List>> GetListsByBoardIdAsync(string boardId)
    {
        if (_collection == null)
        {
            return new List<List>();
        }
        var filter = Builders<List>.Filter.Eq("BoardId", boardId);
        return await _collection.Find(filter).SortBy(x => x.Order).ToListAsync();
    }

    public async Task<List?> GetListByIdAsync(string id)
    {
        if (_collection == null)
        {
            return null;
        }
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<bool> DeleteListsByBoardIdAsync(string boardId)
    {
        if (_collection == null)
        {
            return false;
        }
        var filter = Builders<List>.Filter.Eq("BoardId", boardId);
        var result = await _collection.DeleteManyAsync(filter);
        return result.DeletedCount > 0;
    }

    private void CreateIndexes()
    {
        if (_collection == null) return;

        var boardIdIndex = Builders<List>.IndexKeys.Ascending("BoardId");
        var indexModel = new CreateIndexModel<List>(boardIdIndex);
        _collection.Indexes.CreateOne(indexModel);
    }
}