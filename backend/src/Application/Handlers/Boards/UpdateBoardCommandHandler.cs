using Application.Commands.Boards;
using Application.DTOs;
using Domain.Interfaces;
using MediatR;

namespace Application.Handlers.Boards;

/// <summary>
/// Handler para UpdateBoardCommand
/// </summary>
public class UpdateBoardCommandHandler : IRequestHandler<UpdateBoardCommand, BoardDto>
{
    private readonly BoardRepository _boardRepository;

    public UpdateBoardCommandHandler(BoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<BoardDto> Handle(UpdateBoardCommand request, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetByIdAsync(request.Id);
        if (board == null || board.OwnerId != request.UserId)
        {
            throw new UnauthorizedAccessException("No tienes permisos para actualizar este tablero");
        }

        board.Title = request.BoardDto.Title ?? board.Title;
        board.Description = request.BoardDto.Description ?? board.Description;
        board.Color = request.BoardDto.Color ?? board.Color;
        board.IsPublic = request.BoardDto.IsPublic ?? board.IsPublic;
        board.UpdatedAt = DateTime.UtcNow;

        await _boardRepository.UpdateAsync(board);

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
            Columns = board.Columns,
            CreatedAt = board.CreatedAt,
            UpdatedAt = board.UpdatedAt
        };
    }
}
