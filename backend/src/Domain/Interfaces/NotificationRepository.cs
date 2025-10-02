using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces;

// Repositorio para notificaciones del sistema
public interface NotificationRepository : GenericRepository<Notification>
{
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, bool onlyUnread = false, int limit = 50);
    Task<int> GetUnreadCountAsync(string userId);
    Task<int> DeleteOldNotificationsAsync(DateTime cutoffDate);
    Task<Notification?> GetRecentReminderAsync(string userId, string taskId, DateTime since);
}
