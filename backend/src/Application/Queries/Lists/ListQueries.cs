using Application.DTOs;
using MediatR;

namespace Application.Queries.Lists;

public record GetListsByBoardIdQuery(string BoardId, string UserId) : IRequest<IEnumerable<ListDto>>;
public record GetListByIdQuery(string Id, string UserId) : IRequest<ListDto>;