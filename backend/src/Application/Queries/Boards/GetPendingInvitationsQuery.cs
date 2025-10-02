using Application.DTOs;
using MediatR;

namespace Application.Queries.Boards;

// Query para obtener invitaciones pendientes de un usuario
public class GetPendingInvitationsQuery : IRequest<List<BoardInvitationDto>>
{
    public string UserId { get; set; } = string.Empty;

    public GetPendingInvitationsQuery() { }

    public GetPendingInvitationsQuery(string userId)
    {
        UserId = userId;
    }
}
