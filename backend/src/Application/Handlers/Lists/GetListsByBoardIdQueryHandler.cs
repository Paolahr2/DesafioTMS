using Application.DTOs;
using Application.Queries.Lists;
using Domain.Interfaces;
using MediatR;

namespace Application.Handlers.Lists;

/// <summary>
/// Handler para GetListsByBoardIdQuery
/// </summary>
public class GetListsByBoardIdQueryHandler : IRequestHandler<GetListsByBoardIdQuery, IEnumerable<ListDto>>
{
    private readonly ListRepository _listRepository;
    private readonly BoardRepository _boardRepository;

    public GetListsByBoardIdQueryHandler(ListRepository listRepository, BoardRepository boardRepository)
    {
        _listRepository = listRepository;
        _boardRepository = boardRepository;
    }

    public async Task<IEnumerable<ListDto>> Handle(GetListsByBoardIdQuery request, CancellationToken cancellationToken)
    {
        // Verificar que el usuario tenga acceso al board
        var hasAccess = await _boardRepository.UserHasAccessAsync(request.BoardId, request.UserId);
        if (!hasAccess)
        {
            throw new UnauthorizedAccessException("No tienes acceso a este tablero");
        }

        var lists = await _listRepository.GetListsByBoardIdAsync(request.BoardId);

        return lists.Select(list => new ListDto
        {
            Id = list.Id,
            Title = list.Title,
            BoardId = list.BoardId,
            Order = list.Order,
            Items = list.Items?.Select(item => new ListItemDto
            {
                Id = item.Id,
                Text = item.Text,
                Completed = item.Completed,
                Notes = item.Notes
            }).ToList() ?? new List<ListItemDto>(),
            Notes = list.Notes,
            CreatedAt = list.CreatedAt,
            UpdatedAt = list.UpdatedAt
        });
    }
}