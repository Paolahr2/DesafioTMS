using Application.DTOs.Users;
using MediatR;

namespace Application.Commands.Users;

/// <summary>
/// Command para actualizar información de usuario
/// </summary>
public record UpdateUserCommand(
    string UserId,
    string FirstName,
    string LastName,
    string Email
) : IRequest<UserDto>;
