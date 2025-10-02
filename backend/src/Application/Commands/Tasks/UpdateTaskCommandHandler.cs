using Application.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;
using Domain.Entities;
using Domain.Enums;

namespace Application.Commands.Tasks;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskDto?>
{
    private readonly Domain.Interfaces.TaskRepository _taskRepository;
    private readonly Domain.Interfaces.BoardRepository _boardRepository;
    private readonly Domain.Interfaces.NotificationRepository _notificationRepository;
    private readonly ILogger<UpdateTaskCommandHandler> _logger;
    private readonly MediatR.IMediator _mediator;

    public UpdateTaskCommandHandler(
        Domain.Interfaces.TaskRepository taskRepository,
        Domain.Interfaces.BoardRepository boardRepository,
        Domain.Interfaces.NotificationRepository notificationRepository,
        ILogger<UpdateTaskCommandHandler> logger,
        MediatR.IMediator mediator)
    {
        _taskRepository = taskRepository;
        _boardRepository = boardRepository;
        _notificationRepository = notificationRepository;
        _logger = logger;
        _mediator = mediator;
    }

    public async Task<TaskDto?> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(request.TaskId);
        if (task == null) return null;

        var board = await _boardRepository.GetByIdAsync(task.BoardId);
        if (board == null) return null;

        if (!board.IsPublic && board.OwnerId != request.UserId && !board.MemberIds.Contains(request.UserId))
        {
            _logger.LogWarning("User {UserId} attempted to update task {TaskId} without permission", request.UserId, request.TaskId);
            throw new UnauthorizedAccessException("No tiene permisos para actualizar esta tarea");
        }

        // Store original assigned user to detect changes
        var originalAssignedToId = task.AssignedToId;

        // Apply allowed updates
        if (!string.IsNullOrEmpty(request.Task.Title)) task.Title = request.Task.Title!;
        if (!string.IsNullOrEmpty(request.Task.Description)) task.Description = request.Task.Description!;
        if (request.Task.Priority.HasValue) task.Priority = request.Task.Priority.Value;
        if (request.Task.Status.HasValue) task.Status = request.Task.Status.Value; // ⭐ AGREGADO: Actualizar estado Kanban
        if (request.Task.DueDate.HasValue) task.DueDate = request.Task.DueDate;
        if (!string.IsNullOrEmpty(request.Task.AssignedToId)) task.AssignedToId = request.Task.AssignedToId;
        if (request.Task.Tags != null) task.Tags = request.Task.Tags;
        if (!string.IsNullOrEmpty(request.Task.ListId)) task.ListId = request.Task.ListId;

        task.UpdatedAt = DateTime.UtcNow;
        var updated = await _taskRepository.UpdateAsync(task);

        // Create notification if task was assigned to a user
        if (!string.IsNullOrEmpty(request.Task.AssignedToId) && 
            request.Task.AssignedToId != originalAssignedToId)
        {
            var notification = new Notification
            {
                UserId = request.Task.AssignedToId,
                Type = NotificationType.TaskAssigned,
                Title = "Nueva tarea asignada",
                Message = $"Se te ha asignado la tarea: {task.Title}",
                Data = new Dictionary<string, object>
                {
                    { "taskId", task.Id },
                    { "boardId", task.BoardId },
                    { "assignedBy", request.UserId }
                },
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.CreateAsync(notification);
            _logger.LogInformation("Notification created for user {UserId} about task assignment {TaskId}", 
                request.Task.AssignedToId, task.Id);
        }

        return new TaskDto
        {
            Id = updated.Id,
            Title = updated.Title,
            Description = updated.Description,
            Status = updated.Status, // ⭐ AGREGADO: Incluir Status en respuesta
            ListId = updated.ListId,
            Priority = updated.Priority,
            BoardId = updated.BoardId,
            AssignedToId = updated.AssignedToId,
            CreatedById = updated.CreatedById,
            DueDate = updated.DueDate,
            CompletedAt = updated.CompletedAt,
            Tags = updated.Tags,
            CreatedAt = updated.CreatedAt,
            UpdatedAt = updated.UpdatedAt
        };
    }
}
