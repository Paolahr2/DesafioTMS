using Application.DTOs;
using Application.Queries.Boards;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Handlers.Boards;

public class GetPendingInvitationsQueryHandler : IRequestHandler<GetPendingInvitationsQuery, List<BoardInvitationDto>>
{
    private readonly BoardInvitationRepository _invitationRepository;
    private readonly Domain.Interfaces.BoardRepository _boardRepository;
    private readonly Domain.Interfaces.UserRepository _userRepository;

    public GetPendingInvitationsQueryHandler(
        BoardInvitationRepository invitationRepository,
        Domain.Interfaces.BoardRepository boardRepository,
        Domain.Interfaces.UserRepository userRepository)
    {
        _invitationRepository = invitationRepository;
        _boardRepository = boardRepository;
        _userRepository = userRepository;
    }

    public async Task<List<BoardInvitationDto>> Handle(GetPendingInvitationsQuery request, CancellationToken cancellationToken)
    {
        var invitations = await _invitationRepository.GetPendingInvitationsByUserIdAsync(request.UserId);
        Console.WriteLine($"[DEBUG] Found {invitations.Count()} pending invitations for user {request.UserId}");

        var result = new List<BoardInvitationDto>();
        foreach (var invitation in invitations)
        {
            Console.WriteLine($"[DEBUG] Processing invitation {invitation.Id} for board {invitation.BoardId}");
            
            var board = await _boardRepository.GetByIdAsync(invitation.BoardId);
            Console.WriteLine($"[DEBUG] Board lookup result: {board?.Title ?? "NULL"}");
            
            var invitedBy = await _userRepository.GetByIdAsync(invitation.InviterId);
            Console.WriteLine($"[DEBUG] Inviter lookup result: {invitedBy?.Username ?? "NULL"}");
            
            var invitedUser = await _userRepository.GetByIdAsync(invitation.InviteeId);
            Console.WriteLine($"[DEBUG] Invitee lookup result: {invitedUser?.Username ?? "NULL"}");

            result.Add(new BoardInvitationDto
            {
                Id = invitation.Id,
                BoardId = invitation.BoardId,
                BoardTitle = board?.Title ?? "Tablero desconocido",
                InvitedUserId = invitation.InviteeId,
                InvitedUserEmail = invitedUser?.Email ?? "Email desconocido",
                InvitedUserName = invitedUser?.Username ?? "Usuario desconocido",
                InvitedById = invitation.InviterId,
                InvitedByName = invitedBy?.Username ?? "Usuario desconocido",
                Role = invitation.Role,
                Status = invitation.Status.ToString(),
                Message = invitation.Message,
                CreatedAt = invitation.CreatedAt,
                ExpiresAt = invitation.ExpiresAt
            });
        }

        return result;
    }
}
