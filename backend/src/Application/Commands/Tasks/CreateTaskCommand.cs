using Application.DTOs;
using MediatR;

namespace Application.Commands.Tasks;

/// <summary>
/// Comando para crear una nueva tarea en un tablero
/// </summary>
public record CreateTaskCommand(CreateTaskDto Task, string UserId) : IRequest<TaskDto>;
