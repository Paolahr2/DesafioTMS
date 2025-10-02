using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces;

// Repositorio para tareas con consultas específicas del dominio
public interface TaskRepository : GenericRepository<TaskItem>
{
    Task<IEnumerable<TaskItem>> GetTasksByBoardIdAsync(string boardId);
    Task<IEnumerable<TaskItem>> GetByBoardIdAsync(string boardId);
    Task<IEnumerable<TaskItem>> GetTasksByAssignedUserAsync(string userId);
    Task<IEnumerable<TaskItem>> GetByAssignedUserAsync(string userId);
    Task<IEnumerable<TaskItem>> GetByCreatorAsync(string userId);
    Task<IEnumerable<TaskItem>> GetTasksByStatusAsync(string boardId, Domain.Enums.TaskStatus status);
    Task<IEnumerable<TaskItem>> GetOverdueTasksAsync();
    Task<IEnumerable<TaskItem>> SearchTasksAsync(string searchTerm, string userId);
    Task<int> GetTaskCountByBoardAsync(string boardId);
}
