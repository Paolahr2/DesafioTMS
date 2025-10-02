using Application.DTOs;
using MediatR;

namespace Application.Commands.Boards;

// Comando para invitar a un usuario a un tablero
public class InviteUserToBoardCommand : IRequest<BoardInvitationDto>
{
    public string BoardId { get; set; } = string.Empty;
    public string? InvitedUserEmail { get; set; }
    public string? InvitedUsername { get; set; }
    public string InvitedById { get; set; } = string.Empty;
    public string Role { get; set; } = "Member";
    public string? Message { get; set; }
    // public DateTime? ExpiresAt { get; set; }

    public InviteUserToBoardCommand() { }

    public InviteUserToBoardCommand(string boardId, InviteUserToBoardDto dto, string invitedById)
    {
        BoardId = boardId;
        InvitedUserEmail = dto.Email;
        InvitedUsername = dto.Username;
        InvitedById = invitedById;
        Role = dto.Role;
        Message = dto.Message;
        // ExpiresAt = dto.ExpiresAt;
    }
}

// Comando para responder a una invitación
public class RespondToInvitationCommand : IRequest<bool>
{
    public string InvitationId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public bool Accept { get; set; }

    public RespondToInvitationCommand() { }

    public RespondToInvitationCommand(string invitationId, RespondToInvitationDto dto, string userId)
    {
        InvitationId = invitationId;
        UserId = userId;
        Accept = dto.Accept;
    }
}

// Comando para eliminar un miembro del tablero
public class RemoveBoardMemberCommand : IRequest<bool>
{
    public string BoardId { get; set; } = string.Empty;
    public string MemberId { get; set; } = string.Empty;
    public string RemovedById { get; set; } = string.Empty;

    public RemoveBoardMemberCommand() { }

    public RemoveBoardMemberCommand(string boardId, string memberId, string removedById)
    {
        BoardId = boardId;
        MemberId = memberId;
        RemovedById = removedById;
    }
}
