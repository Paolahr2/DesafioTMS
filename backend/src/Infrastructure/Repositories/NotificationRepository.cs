using Domain.Entities;
using Domain.Interfaces;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class NotificationRepository : GenericRepository<Notification>, Domain.Interfaces.NotificationRepository
{
    private new readonly IMongoCollection<Notification> _collection;

    public NotificationRepository(IMongoDatabase database)
        : base(database, "Notifications")
    {
        _collection = database.GetCollection<Notification>("Notifications");
    }

    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, bool onlyUnread = false, int limit = 50)
    {
        var filter = onlyUnread 
            ? Builders<Notification>.Filter.And(
                Builders<Notification>.Filter.Eq(n => n.UserId, userId),
                Builders<Notification>.Filter.Eq(n => n.IsRead, false)
              )
            : Builders<Notification>.Filter.Eq(n => n.UserId, userId);

        return await _collection
            .Find(filter)
            .SortByDescending(n => n.CreatedAt)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        var filter = Builders<Notification>.Filter.And(
            Builders<Notification>.Filter.Eq(n => n.UserId, userId),
            Builders<Notification>.Filter.Eq(n => n.IsRead, false)
        );

        return (int)await _collection.CountDocumentsAsync(filter);
    }

    public async Task<int> DeleteOldNotificationsAsync(DateTime cutoffDate)
    {
        var filter = Builders<Notification>.Filter.Lt(n => n.CreatedAt, cutoffDate);
        var result = await _collection.DeleteManyAsync(filter);
        return (int)result.DeletedCount;
    }

    public async Task<Notification?> GetRecentReminderAsync(string userId, string taskId, DateTime since)
    {
        var filter = Builders<Notification>.Filter.And(
            Builders<Notification>.Filter.Eq(n => n.UserId, userId),
            Builders<Notification>.Filter.Eq("Data.TaskId", taskId),
            Builders<Notification>.Filter.Gte(n => n.CreatedAt, since)
        );

        return await _collection.Find(filter).FirstOrDefaultAsync();
    }
}
