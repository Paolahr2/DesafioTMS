using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class TaskRepository : GenericRepository<TaskItem>, Domain.Interfaces.TaskRepository
{
    public TaskRepository(IMongoDatabase database) : base(database, "tasks")
    {
        // Crear índices
        CreateIndexes();
    }

    public async Task<IEnumerable<TaskItem>> GetTasksByBoardIdAsync(string boardId)
    {
        if (_collection == null)
        {
            return new List<TaskItem>();
        }
        return await _collection.Find(x => x.BoardId == boardId)
            .SortBy(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetByBoardIdAsync(string boardId)
    {
        return await GetTasksByBoardIdAsync(boardId);
    }

    public async Task<IEnumerable<TaskItem>> GetTasksByAssignedUserAsync(string userId)
    {
        if (_collection == null)
        {
            return new List<TaskItem>();
        }
        return await _collection.Find(x => x.AssignedToId == userId)
            .SortBy(x => x.DueDate ?? DateTime.MaxValue)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetByAssignedUserAsync(string userId)
    {
        return await GetTasksByAssignedUserAsync(userId);
    }

    public async Task<IEnumerable<TaskItem>> GetByCreatorAsync(string userId)
    {
        if (_collection == null)
        {
            return new List<TaskItem>();
        }
        return await _collection.Find(x => x.CreatedById == userId)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetTasksByBoardIdAndStatusAsync(string boardId, Domain.Enums.TaskStatus status)
    {
        if (_collection == null)
        {
            return new List<TaskItem>();
        }
        // TODO: Adaptar para usar ListId en lugar de Status
        // Por ahora, devolver todas las tareas del board
        return await _collection.Find(x => x.BoardId == boardId)
            .SortBy(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetOverdueTasksAsync()
    {
        if (_collection == null)
        {
            return new List<TaskItem>();
        }
        var filter = Builders<TaskItem>.Filter.And(
            Builders<TaskItem>.Filter.Lt(x => x.DueDate, DateTime.UtcNow),
            Builders<TaskItem>.Filter.Ne(x => x.IsCompleted, true)
        );
        return await _collection.Find(filter)
            .SortBy(x => x.DueDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskItem>> SearchTasksAsync(string searchTerm, string userId)
    {
        if (_collection == null)
        {
            return new List<TaskItem>();
        }
        var searchFilter = Builders<TaskItem>.Filter.Or(
            Builders<TaskItem>.Filter.Regex(x => x.Title, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
            Builders<TaskItem>.Filter.Regex(x => x.Description, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
            Builders<TaskItem>.Filter.AnyEq(x => x.Tags, searchTerm)
        );

        var userFilter = Builders<TaskItem>.Filter.Or(
            Builders<TaskItem>.Filter.Eq(x => x.CreatedById, userId),
            Builders<TaskItem>.Filter.Eq(x => x.AssignedToId, userId)
        );

        var filter = Builders<TaskItem>.Filter.And(searchFilter, userFilter);

        return await _collection.Find(filter)
            .SortByDescending(x => x.UpdatedAt)
            .ToListAsync();
    }

    public async Task<bool> UpdateTaskStatusAsync(string taskId, Domain.Enums.TaskStatus newStatus)
    {
        if (_collection == null)
        {
            return false;
        }
        var filter = Builders<TaskItem>.Filter.Eq(x => x.Id, taskId);
        var update = Builders<TaskItem>.Update
            .Set(x => x.IsCompleted, newStatus == Domain.Enums.TaskStatus.Done)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);
        var result = await _collection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> AssignTaskAsync(string taskId, string userId)
    {
        if (_collection == null)
        {
            return false;
        }
        var filter = Builders<TaskItem>.Filter.Eq(x => x.Id, taskId);
        var update = Builders<TaskItem>.Update
            .Set(x => x.AssignedToId, userId)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);
        var result = await _collection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> UpdateTaskPriorityAsync(string taskId, TaskPriority newPriority)
    {
        if (_collection == null)
        {
            return false;
        }
        var filter = Builders<TaskItem>.Filter.Eq(x => x.Id, taskId);
        var update = Builders<TaskItem>.Update
            .Set(x => x.Priority, newPriority)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);
        var result = await _collection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> MarkTaskAsCompletedAsync(string taskId, string userId)
    {
        if (_collection == null)
        {
            return false;
        }
        var filter = Builders<TaskItem>.Filter.Eq(x => x.Id, taskId);
        var update = Builders<TaskItem>.Update
            .Set(x => x.IsCompleted, true)
            .Set(x => x.CompletedAt, DateTime.UtcNow)
            .Set(x => x.CompletedBy, userId)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);
        var result = await _collection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> UpdateTaskPositionAsync(string taskId, int newPosition)
    {
        if (_collection == null)
        {
            return false;
        }
        var filter = Builders<TaskItem>.Filter.Eq(x => x.Id, taskId);
        var update = Builders<TaskItem>.Update
            .Set(x => x.Position, newPosition)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);
        var result = await _collection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    private void CreateIndexes()
    {
        if (_collection == null) return;

        try
        {
            // Índice para BoardId
            var boardIdIndex = Builders<TaskItem>.IndexKeys.Ascending(x => x.BoardId);
            var boardIdIndexModel = new CreateIndexModel<TaskItem>(boardIdIndex);
            _collection.Indexes.CreateOne(boardIdIndexModel);

            // Índice para ListId
            var listIdIndex = Builders<TaskItem>.IndexKeys.Ascending(x => x.ListId);
            var listIdIndexModel = new CreateIndexModel<TaskItem>(listIdIndex);
            _collection.Indexes.CreateOne(listIdIndexModel);

            // Índice para AssignedToId
            var assignedToIndex = Builders<TaskItem>.IndexKeys.Ascending(x => x.AssignedToId);
            var assignedToIndexModel = new CreateIndexModel<TaskItem>(assignedToIndex);
            _collection.Indexes.CreateOne(assignedToIndexModel);

            // Índice para CreatedById
            var createdByIndex = Builders<TaskItem>.IndexKeys.Ascending(x => x.CreatedById);
            var createdByIndexModel = new CreateIndexModel<TaskItem>(createdByIndex);
            _collection.Indexes.CreateOne(createdByIndexModel);

            // Índice compuesto para BoardId y IsCompleted
            var boardCompletedIndex = Builders<TaskItem>.IndexKeys.Ascending(x => x.BoardId).Ascending(x => x.IsCompleted);
            var boardCompletedIndexModel = new CreateIndexModel<TaskItem>(boardCompletedIndex);
            _collection.Indexes.CreateOne(boardCompletedIndexModel);

            // Índice para DueDate
            var dueDateIndex = Builders<TaskItem>.IndexKeys.Ascending(x => x.DueDate);
            var dueDateIndexModel = new CreateIndexModel<TaskItem>(dueDateIndex);
            _collection.Indexes.CreateOne(dueDateIndexModel);

            // Índice de texto para búsqueda
            var textIndex = Builders<TaskItem>.IndexKeys.Text(x => x.Title).Text(x => x.Description);
            var textIndexModel = new CreateIndexModel<TaskItem>(textIndex);
            _collection.Indexes.CreateOne(textIndexModel);
        }
        catch (Exception)
        {
            // Si hay algún error creando índices, continuar sin fallar
            // Los índices se pueden crear manualmente en MongoDB si es necesario
        }
    }

    public async Task<IEnumerable<TaskItem>> GetTasksByStatusAsync(string boardId, Domain.Enums.TaskStatus status)
    {
        if (_collection == null)
        {
            return new List<TaskItem>();
        }
        // TODO: Adaptar para usar ListId en lugar de Status
        // Por ahora, devolver todas las tareas del board
        return await _collection.Find(x => x.BoardId == boardId)
            .SortBy(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetTaskCountByBoardAsync(string boardId)
    {
        if (_collection == null)
        {
            return 0;
        }
        var filter = Builders<TaskItem>.Filter.Eq(x => x.BoardId, boardId);
        return (int)await _collection.CountDocumentsAsync(filter);
    }

    public async Task<bool> UpdateTaskListAsync(string taskId, string newListId, int newPosition)
    {
        if (_collection == null)
        {
            return false;
        }
        var filter = Builders<TaskItem>.Filter.Eq(x => x.Id, taskId);
        var update = Builders<TaskItem>.Update
            .Set(x => x.ListId, newListId)
            .Set(x => x.Position, newPosition)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);
        var result = await _collection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }
}