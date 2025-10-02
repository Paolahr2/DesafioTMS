using Application.DTOs;
using MediatR;

namespace Application.Queries.Boards;

/// <summary>
/// Query para obtener un tablero por ID
/// </summary>
public record GetBoardByIdQuery(string BoardId, string UserId) : IRequest<BoardDto>;