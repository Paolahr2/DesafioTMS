using MediatR;

namespace Application.Commands.Users;

/// <summary>
/// Command para eliminar usuario
/// </summary>
public record DeleteUserCommand(string UserId) : IRequest<bool>;
