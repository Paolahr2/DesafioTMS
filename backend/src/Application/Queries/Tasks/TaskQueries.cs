using Application.DTOs;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Queries.Tasks;

// Query para obtener una tarea por ID
public record GetTaskByIdQuery(string TaskId, string UserId) : IRequest<TaskDto?>;

// Query para obtener tareas de un tablero
public record GetBoardTasksQuery(string BoardId, string UserId) : IRequest<IEnumerable<TaskDto>>;

// Query para obtener tareas asignadas a un usuario
public record GetUserTasksQuery(string UserId, string? ListId = null) : IRequest<IEnumerable<TaskDto>>;

// Query para obtener tareas creadas por un usuario
public record GetCreatedTasksQuery(string UserId) : IRequest<IEnumerable<TaskDto>>;

// Query para buscar tareas
public record SearchTasksQuery(string Query, string UserId) : IRequest<IEnumerable<TaskDto>>;

// Handler para queries de tareas
public class TaskQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskDto?>,
                              IRequestHandler<GetBoardTasksQuery, IEnumerable<TaskDto>>,
                              IRequestHandler<GetUserTasksQuery, IEnumerable<TaskDto>>,
                              IRequestHandler<GetCreatedTasksQuery, IEnumerable<TaskDto>>,
                              IRequestHandler<SearchTasksQuery, IEnumerable<TaskDto>>
{
    private readonly Domain.Interfaces.TaskRepository _taskRepository;
    private readonly Domain.Interfaces.BoardRepository _boardRepository;
    private readonly ILogger<TaskQueryHandler> _logger;

    public TaskQueryHandler(
        Domain.Interfaces.TaskRepository taskRepository,
        Domain.Interfaces.BoardRepository boardRepository,
        ILogger<TaskQueryHandler> logger)
    {
        _taskRepository = taskRepository;
        _boardRepository = boardRepository;
        _logger = logger;
    }

    public async Task<TaskDto?> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var task = await _taskRepository.GetByIdAsync(request.TaskId);
            if (task == null) return null;

            // Verificar acceso al tablero
            var board = await _boardRepository.GetByIdAsync(task.BoardId);
            if (board == null || (!board.IsPublic && board.OwnerId != request.UserId && !board.MemberIds.Contains(request.UserId)))
            {
                _logger.LogWarning("User {UserId} attempted to access task {TaskId} without permission", 
                    request.UserId, request.TaskId);
                return null;
            }

            return MapToTaskDto(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting task {TaskId}", request.TaskId);
            throw;
        }
    }

    public async Task<IEnumerable<TaskDto>> Handle(GetBoardTasksQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Verificar acceso al tablero
            var board = await _boardRepository.GetByIdAsync(request.BoardId);
            if (board == null || (!board.IsPublic && board.OwnerId != request.UserId && !board.MemberIds.Contains(request.UserId)))
            {
                _logger.LogWarning("User {UserId} attempted to access tasks from board {BoardId} without permission", 
                    request.UserId, request.BoardId);
                return new List<TaskDto>();
            }

            var tasks = await _taskRepository.GetByBoardIdAsync(request.BoardId);
            return tasks.Select(MapToTaskDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tasks for board {BoardId}", request.BoardId);
            throw;
        }
    }

    public async Task<IEnumerable<TaskDto>> Handle(GetUserTasksQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tasks = await _taskRepository.GetByAssignedUserAsync(request.UserId);
            
            if (!string.IsNullOrEmpty(request.ListId))
            {
                tasks = tasks.Where(t => t.ListId == request.ListId);
            }

            return tasks.Select(MapToTaskDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tasks for user {UserId}", request.UserId);
            throw;
        }
    }

    public async Task<IEnumerable<TaskDto>> Handle(GetCreatedTasksQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tasks = await _taskRepository.GetByCreatorAsync(request.UserId);
            return tasks.Select(MapToTaskDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting created tasks for user {UserId}", request.UserId);
            throw;
        }
    }

    public async Task<IEnumerable<TaskDto>> Handle(SearchTasksQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tasks = await _taskRepository.SearchTasksAsync(request.Query, request.UserId);
            return tasks.Select(MapToTaskDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching tasks with query: {Query}", request.Query);
            throw;
        }
    }

    private static TaskDto MapToTaskDto(Domain.Entities.TaskItem task)
    {
        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status, // ⭐ CRÍTICO: Mapear el Status
            ListId = task.ListId,
            Priority = task.Priority,
            BoardId = task.BoardId,
            AssignedToId = task.AssignedToId,
            CreatedById = task.CreatedById,
            DueDate = task.DueDate,
            CompletedAt = task.CompletedAt,
            Tags = task.Tags,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }
}
