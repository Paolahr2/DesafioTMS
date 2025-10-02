using Application.DTOs;
using MediatR;

namespace Application.Commands.Boards;

public record CreateBoardCommand(CreateBoardDto BoardDto, string UserId) : IRequest<BoardDto>;
public record UpdateBoardCommand(string Id, UpdateBoardDto BoardDto, string UserId) : IRequest<BoardDto>;
public record DeleteBoardCommand(string Id, string UserId) : IRequest<bool>;
