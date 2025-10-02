using Application.DTOs;
using MediatR;

namespace Application.Commands.Lists;

public record CreateListCommand(CreateListDto ListDto, string UserId) : IRequest<ListDto>;
public record UpdateListCommand(string Id, UpdateListDto ListDto, string UserId) : IRequest<ListDto>;
public record DeleteListCommand(string Id, string UserId) : IRequest<bool>;
public record ReorderListsCommand(string BoardId, List<ReorderListDto> Lists, string UserId) : IRequest<bool>;