using Application.Commands.Boards;
using Application.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Handlers.Boards;

/// <summary>
/// Handler para CreateBoardCommand
/// </summary>
public class CreateBoardCommandHandler : IRequestHandler<CreateBoardCommand, BoardDto>
{
    private readonly BoardRepository _boardRepository;
    private readonly ListRepository _listRepository;

    public CreateBoardCommandHandler(BoardRepository boardRepository, ListRepository listRepository)
    {
        _boardRepository = boardRepository;
        _listRepository = listRepository;
    }

    public async Task<BoardDto> Handle(CreateBoardCommand request, CancellationToken cancellationToken)
    {
        var board = new Board
        {
            Title = request.BoardDto.Title,
            Description = request.BoardDto.Description,
            OwnerId = request.UserId,
            MemberIds = new List<string> { request.UserId },
            Color = request.BoardDto.Color,
            IsArchived = false,
            IsPublic = request.BoardDto.IsPublic,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _boardRepository.CreateAsync(board);

        // NOTA: NO crear listas por defecto aquí.
        // Las columnas "Por Hacer", "En Progreso", "Hecho" son estados de las tareas (hardcoded en el frontend),
        // NO son listas de checklist almacenadas en la base de datos.
        // Las listas de checklist son independientes y el usuario las crea manualmente.

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
