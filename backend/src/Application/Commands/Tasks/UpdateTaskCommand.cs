using Application.DTOs;
using MediatR;

namespace Application.Commands.Tasks;

/// <summary>
/// Comando para actualizar una tarea existente
/// </summary>
public record UpdateTaskCommand(string TaskId, UpdateTaskDto Task, string UserId) : IRequest<TaskDto?>;
