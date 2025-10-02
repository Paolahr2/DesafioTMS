using Application.DTOs;
using Application.Queries.Boards;
using Domain.Interfaces;
using MediatR;

namespace Application.Handlers.Boards;

/// <summary>
/// Handler para GetBoardByIdQuery
/// </summary>
public class GetBoardByIdQueryHandler : IRequestHandler<GetBoardByIdQuery, BoardDto>
{
    private readonly BoardRepository _boardRepository;

    public GetBoardByIdQueryHandler(BoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<BoardDto> Handle(GetBoardByIdQuery request, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetByIdAsync(request.BoardId);

        if (board == null)
        {
            throw new ArgumentException($"Tablero con ID {request.BoardId} no encontrado");
        }

        // Verificar que el usuario tenga acceso al tablero
        if (board.OwnerId != request.UserId && !board.MemberIds.Contains(request.UserId))
        {
            throw new UnauthorizedAccessException("No tienes acceso a este tablero");
        }

        return new BoardDto
        {
            Id = board.Id,
            Title = board.Title,
            Description = board.Description,
            OwnerId = board.OwnerId,
            MemberIds = board.MemberIds,
            Color = board.Color,
            IsArchived = board.IsArchived,
            IsPublic = board.IsPublic,
            CreatedAt = board.CreatedAt,
            UpdatedAt = board.UpdatedAt
        };
    }
}