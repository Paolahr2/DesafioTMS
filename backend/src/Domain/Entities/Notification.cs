using Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

// Notificación del sistema para usuarios
public class Notification : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    
    // Datos adicionales específicos del tipo de notificación
    public Dictionary<string, object> Data { get; set; } = new();
    
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
}
