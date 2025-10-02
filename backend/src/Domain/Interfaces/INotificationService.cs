using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces;

public interface INotificationService
{
    Task CreateNotificationAsync(string userId, NotificationType type, string title, string message, Dictionary<string, object>? data = null);
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, bool onlyUnread = true, int limit = 50);
    Task MarkAsReadAsync(string notificationId);
    Task MarkAllAsReadAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
    Task DeleteOldNotificationsAsync(int daysToKeep = 30);
}