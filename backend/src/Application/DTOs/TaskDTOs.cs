using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using TaskStatus = Domain.Enums.TaskStatus; // Alias para evitar conflicto

namespace Application.DTOs;

// DTOs para tareas
public class TaskDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskStatus Status { get; set; } // ⭐ CRÍTICO: Campo Status para columna Kanban
    public string? ListId { get; set; } // Opcional: para checklists
    public TaskPriority Priority { get; set; }
    public string BoardId { get; set; } = string.Empty;
    public string? AssignedToId { get; set; }
    public string CreatedById { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateTaskDto
{
    [Required(ErrorMessage = "El título es requerido")]
    [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "El ID del tablero es requerido")]
    public string BoardId { get; set; } = string.Empty;

    // Status para determinar la columna Kanban (Por Hacer, En Progreso, Hecho)
    public TaskStatus Status { get; set; } = TaskStatus.Todo;
    
    // ListId es opcional - solo si la tarea pertenece a una lista de checklist
    public string? ListId { get; set; }

    public TaskPriority? Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }
    public string? AssignedToId { get; set; }
    public List<string> Tags { get; set; } = new();
}

public class UpdateTaskDto
{
    [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
    public string? Title { get; set; }

    [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
    public string? Description { get; set; }

    public TaskPriority? Priority { get; set; }
    public TaskStatus? Status { get; set; } // ⭐ AGREGADO: Para actualizar el estado Kanban
    public string? ListId { get; set; }
    public DateTime? DueDate { get; set; }
    public string? AssignedToId { get; set; }
    public List<string>? Tags { get; set; }
}

public class UpdateTaskListDto
{
    [Required]
    public string ListId { get; set; } = string.Empty;
    public int? Position { get; set; }
}

public class AssignTaskDto
{
    public string? AssignedToId { get; set; }
}
