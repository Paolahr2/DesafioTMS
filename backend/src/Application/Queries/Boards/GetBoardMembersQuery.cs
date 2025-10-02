using Application.DTOs;
using MediatR;

namespace Application.Queries.Boards;

// Query para obtener miembros de un tablero
public class GetBoardMembersQuery : IRequest<List<BoardMemberDto>>
{
    public string BoardId { get; set; } = string.Empty;

    public GetBoardMembersQuery() { }

    public GetBoardMembersQuery(string boardId)
    {
        BoardId = boardId;
    }
}
