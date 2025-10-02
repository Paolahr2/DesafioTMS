using Application.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Tasks;

public class ChangeTaskListCommandHandler : IRequestHandler<ChangeTaskListCommand, TaskDto?>
{
    private readonly Domain.Interfaces.TaskRepository _taskRepository;
    private readonly Domain.Interfaces.BoardRepository _boardRepository;
    private readonly ILogger<ChangeTaskListCommandHandler> _logger;

    public ChangeTaskListCommandHandler(
        Domain.Interfaces.TaskRepository taskRepository,
        Domain.Interfaces.BoardRepository boardRepository,
        ILogger<ChangeTaskListCommandHandler> logger)
    {
        _taskRepository = taskRepository;
    _boardRepository = boardRepository;
        _logger = logger;
    }

    public async Task<TaskDto?> Handle(ChangeTaskListCommand request, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(request.TaskId);
        if (task == null) return null;

        var board = await _boardRepository.GetByIdAsync(task.BoardId);
        if (board == null) return null;

        // Verify access to board
        if (!board.IsPublic && board.OwnerId != request.UserId && !board.MemberIds.Contains(request.UserId))
        {
            _logger.LogWarning("User {UserId} attempted to change status for task {TaskId} without permission", request.UserId, request.TaskId);
            throw new UnauthorizedAccessException("No tiene permisos para modificar esta tarea");
        }

        // Apply list change
        task.ListId = request.ListId;
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
