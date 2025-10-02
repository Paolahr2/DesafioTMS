using MediatR;

namespace Application.Commands.Tasks;

/// <summary>
/// Comando para eliminar una tarea
/// </summary>
public record DeleteTaskCommand(string TaskId, string UserId) : IRequest<bool>;
