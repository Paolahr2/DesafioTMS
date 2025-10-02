using Application.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Tasks;

public class AssignTaskCommandHandler : IRequestHandler<AssignTaskCommand, TaskDto?>
{
    private readonly Domain.Interfaces.TaskRepository _taskRepository;
    private readonly Domain.Interfaces.BoardRepository _boardRepository;
    private readonly ILogger<AssignTaskCommandHandler> _logger;

    public AssignTaskCommandHandler(
        Domain.Interfaces.TaskRepository taskRepository,
        Domain.Interfaces.BoardRepository boardRepository,
        ILogger<AssignTaskCommandHandler> logger)
    {
        _taskRepository = taskRepository;
        _boardRepository = boardRepository;
        _logger = logger;
    }

    public async Task<TaskDto?> Handle(AssignTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(request.TaskId);
        if (task == null) return null;

        var board = await _boardRepository.GetByIdAsync(task.BoardId);
        if (board == null) return null;

        // Verify access to board
        if (!board.IsPublic && board.OwnerId != request.UserId && !board.MemberIds.Contains(request.UserId))
        {
            _logger.LogWarning("User {UserId} attempted to assign task {TaskId} without permission", request.UserId, request.TaskId);
            throw new UnauthorizedAccessException("No tiene permisos para asignar esta tarea");
        }

        // If assigning to someone, verify the assignee is a board member
        if (request.AssignedToId != null)
        {
            if (!board.MemberIds.Contains(request.AssignedToId) && board.OwnerId != request.AssignedToId)
            {
                throw new InvalidOperationException("El usuario asignado debe ser miembro del tablero");
            }
        }

        // Apply assignment change
        task.AssignedToId = request.AssignedToId;
        task.UpdatedAt = DateTime.UtcNow;

        var updated = await _taskRepository.UpdateAsync(task);

        return new TaskDto
        {
            Id = updated.Id,
            Title = updated.Title,
            Description = updated.Description,
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