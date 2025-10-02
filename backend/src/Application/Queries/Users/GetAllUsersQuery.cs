using Application.DTOs.Users;
using MediatR;

namespace Application.Queries.Users;

/// <summary>
/// Query para obtener todos los usuarios registrados en el sistema
/// </summary>
public record GetAllUsersQuery() : IRequest<List<Application.DTOs.Users.UserDto>>;
