using Application.DTOs;
using MediatR;

namespace Application.Commands.Tasks;

/// <summary>
/// Comando para cambiar la lista de una tarea
/// </summary>
public record ChangeTaskListCommand(string TaskId, string ListId, string UserId) : IRequest<TaskDto?>;
