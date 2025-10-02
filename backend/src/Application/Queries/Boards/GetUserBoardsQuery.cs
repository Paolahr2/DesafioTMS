using Application.DTOs;
using MediatR;

namespace Application.Queries.Boards;

/// <summary>
/// Query para obtener todos los tableros de un usuario
/// </summary>
public record GetUserBoardsQuery(string UserId, string UserRole) : IRequest<List<BoardDto>>;