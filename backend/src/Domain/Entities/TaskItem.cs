using Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;
using TaskStatus = Domain.Enums.TaskStatus; // Alias para evitar conflicto con System.Threading.Tasks.TaskStatus

namespace Domain.Entities;

// Tarea individual dentro de un tablero 
public class TaskItem : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BoardId { get; set; } = string.Empty;
    
    // Estado de la tarea en el sistema Kanban (Por Hacer, En Progreso, Hecho)
    public TaskStatus Status { get; set; } = TaskStatus.Todo;
    
    // ListId es OPCIONAL y solo se usa si la tarea está asociada a una lista de checklist
    // NO confundir con el estado de la tarea
    public string? ListId { get; set; }
    
    public string CreatedById { get; set; } = string.Empty;
    public string? AssignedToId { get; set; }
    
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    
    public DateTime? DueDate { get; set; }
    public List<string> Tags { get; set; } = new();
    
    // Para ordenamiento dentro de cada columna
    public int Position { get; set; }
    
    // Información de completado
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }
    public string? CompletedBy { get; set; }
    
    // Progreso de la tarea (0-100)
    public int ProgressPercentage { get; set; } = 0;
    
    // Archivos adjuntos (URLs)
    public List<string> Attachments { get; set; } = new();
}
