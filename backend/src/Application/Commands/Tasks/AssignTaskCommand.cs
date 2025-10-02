using Application.DTOs;
using MediatR;

namespace Application.Commands.Tasks;

/// <summary>
/// Comando para asignar una tarea a un usuario
/// </summary>
public record AssignTaskCommand(string TaskId, string? AssignedToId, string UserId) : IRequest<TaskDto?>;