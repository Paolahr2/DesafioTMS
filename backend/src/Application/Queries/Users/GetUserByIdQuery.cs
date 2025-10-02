using Application.DTOs.Users;
using MediatR;

namespace Application.Queries.Users;

/// <summary>
/// Query para obtener usuario por ID
/// </summary>
public record GetUserByIdQuery(string UserId) : IRequest<UserDto>;
