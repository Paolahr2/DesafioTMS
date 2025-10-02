using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Tasks;

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, bool>
{
    private readonly Domain.Interfaces.TaskRepository _taskRepository;
    private readonly Domain.Interfaces.BoardRepository _boardRepository;
    private readonly ILogger<DeleteTaskCommandHandler> _logger;

    public DeleteTaskCommandHandler(
        Domain.Interfaces.TaskRepository taskRepository,
        Domain.Interfaces.BoardRepository boardRepository,
        ILogger<DeleteTaskCommandHandler> logger)
    {
        _taskRepository = taskRepository;
        _boardRepository = boardRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(request.TaskId);
        if (task == null) return false;

    var board = await _boardRepository.GetByIdAsync(task.BoardId);
        if (board == null) return false;

        // Only creator or board owner can delete
        if (task.CreatedById != request.UserId && board.OwnerId != request.UserId)
        {
            _logger.LogWarning("User {UserId} attempted to delete task {TaskId} without permission", request.UserId, request.TaskId);
            throw new UnauthorizedAccessException("No tiene permisos para eliminar esta tarea");
        }

        // Business rule: cannot delete completed tasks
        if (task.IsCompleted)
        {
            throw new InvalidOperationException("No se puede eliminar una tarea completada");
        }

        var deleted = await _taskRepository.DeleteAsync(request.TaskId);
        return deleted;
    }
}
