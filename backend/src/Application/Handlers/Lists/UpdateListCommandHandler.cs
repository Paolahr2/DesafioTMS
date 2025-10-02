using Application.Commands.Lists;
using Application.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Handlers.Lists;

/// <summary>
/// Handler para UpdateListCommand
/// </summary>
public class UpdateListCommandHandler : IRequestHandler<UpdateListCommand, ListDto>
{
    private readonly ListRepository _listRepository;
    private readonly BoardRepository _boardRepository;

    public UpdateListCommandHandler(ListRepository listRepository, BoardRepository boardRepository)
    {
        _listRepository = listRepository;
        _boardRepository = boardRepository;
    }

    public async Task<ListDto> Handle(UpdateListCommand request, CancellationToken cancellationToken)
    {
        var list = await _listRepository.GetByIdAsync(request.Id);
        if (list == null)
        {
            throw new KeyNotFoundException("Lista no encontrada");
        }

        // Verificar que el usuario tenga acceso al board
        var hasAccess = await _boardRepository.UserHasAccessAsync(list.BoardId, request.UserId);
        if (!hasAccess)
        {
            throw new UnauthorizedAccessException("No tienes acceso a este tablero");
        }

        list.Title = request.ListDto.Title;
        list.Order = request.ListDto.Order ?? list.Order;
        if (request.ListDto.Items != null)
        {
            list.Items = request.ListDto.Items.Select(item => new Domain.Entities.ListItem
            {
                Id = item.Id ?? Guid.NewGuid().ToString(),
                Text = item.Text,
                Completed = item.Completed,
                Notes = item.Notes
            }).ToList();
        }
        list.UpdatedAt = DateTime.UtcNow;

        await _listRepository.UpdateAsync(list);

        return new ListDto
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
        };
    }
}
