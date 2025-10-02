using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using TaskStatus = Domain.Enums.TaskStatus; // Alias para evitar conflicto

namespace Application.Commands.Tasks;

/// <summary>
/// Handler responsable únicamente de crear nuevas tareas
/// </summary>
public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskDto>
{
    private readonly Domain.Interfaces.TaskRepository _taskRepository;
    private readonly Domain.Interfaces.BoardRepository _boardRepository;
    private readonly ILogger<CreateTaskCommandHandler> _logger;

    public CreateTaskCommandHandler(
        Domain.Interfaces.TaskRepository taskRepository,
        Domain.Interfaces.BoardRepository boardRepository,
        ILogger<CreateTaskCommandHandler> logger)
    {
        _taskRepository = taskRepository;
        _boardRepository = boardRepository;
        _logger = logger;
    }

    public async Task<TaskDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating task for user: {UserId} in board: {BoardId}", 
                request.UserId, request.Task.BoardId);
            _logger.LogInformation("Task data: Title={Title}, Status={Status}, BoardId={BoardId}", 
                request.Task.Title, request.Task.Status, request.Task.BoardId);

            await ValidateUserCanCreateTaskAsync(request);

            var task = CreateTaskFromRequest(request);
            var createdTask = await _taskRepository.CreateAsync(task);

            _logger.LogInformation("Task created successfully: {TaskId} with status: {Status}", 
                createdTask.Id, createdTask.Status);

            return MapToTaskDto(createdTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task for user: {UserId}. Details: {Message}", request.UserId, ex.Message);
            throw;
        }
    }

    private async Task ValidateUserCanCreateTaskAsync(CreateTaskCommand request)
    {
        var board = await _boardRepository.GetByIdAsync(request.Task.BoardId);
        if (board == null)
        {
            throw new ArgumentException("El tablero especificado no existe");
        }

        if (!board.IsPublic && board.OwnerId != request.UserId && !board.MemberIds.Contains(request.UserId))
        {
            throw new UnauthorizedAccessException("No tiene permisos para crear tareas en este tablero");
        }
    }

    private static TaskItem CreateTaskFromRequest(CreateTaskCommand request)
    {
        return new TaskItem
        {
            Title = request.Task.Title,
            Description = request.Task.Description,
            Status = request.Task.Status,
            ListId = request.Task.ListId, // Opcional - solo si pertenece a una lista de checklist
            Priority = request.Task.Priority ?? TaskPriority.Medium,
            BoardId = request.Task.BoardId,
            AssignedToId = request.Task.AssignedToId,
            CreatedById = request.UserId,
            DueDate = request.Task.DueDate,
            Tags = request.Task.Tags ?? new List<string>()
        };
    }

    private static TaskDto MapToTaskDto(TaskItem task)
    {
        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            ListId = task.ListId,
            Priority = task.Priority,
            BoardId = task.BoardId,
            AssignedToId = task.AssignedToId,
            CreatedById = task.CreatedById,
            DueDate = task.DueDate,
            Tags = task.Tags,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }
}
