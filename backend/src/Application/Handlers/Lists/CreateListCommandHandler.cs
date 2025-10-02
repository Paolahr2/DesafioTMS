using Application.Commands.Lists;
using Application.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Handlers.Lists;

/// <summary>
/// Handler para CreateListCommand
/// </summary>
public class CreateListCommandHandler : IRequestHandler<CreateListCommand, ListDto>
{
    private readonly ListRepository _listRepository;
    private readonly BoardRepository _boardRepository;

    public CreateListCommandHandler(ListRepository listRepository, BoardRepository boardRepository)
    {
        _listRepository = listRepository;
        _boardRepository = boardRepository;
    }

    public async Task<ListDto> Handle(CreateListCommand request, CancellationToken cancellationToken)
    {
        // Verificar que el usuario tenga acceso al board
        var hasAccess = await _boardRepository.UserHasAccessAsync(request.ListDto.BoardId, request.UserId);
        if (!hasAccess)
        {
            throw new UnauthorizedAccessException("No tienes acceso a este tablero");
        }

        var list = new List
        {
            Title = request.ListDto.Title,
            BoardId = request.ListDto.BoardId,
            Order = request.ListDto.Order,
            Items = request.ListDto.Items?.Select(item => new ListItem
            {
                Id = item.Id ?? Guid.NewGuid().ToString(),
                Text = item.Text,
                Completed = item.Completed,
                Notes = item.Notes
            }).ToList() ?? new List<ListItem>(),
            Notes = request.ListDto.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _listRepository.CreateAsync(list);

        return new ListDto
        {
            Id = list.Id,
            Title = list.Title,
            BoardId = list.BoardId,
            Order = list.Order,
            Items = list.Items.Select(item => new ListItemDto
            {
                Id = item.Id,
                Text = item.Text,
                Completed = item.Completed,
                Notes = item.Notes
            }).ToList(),
            Notes = list.Notes,
            CreatedAt = list.CreatedAt,
            UpdatedAt = list.UpdatedAt
        };
    }
}