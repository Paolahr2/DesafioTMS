using Domain.Enums;

namespace Domain.Entities;

public class BoardInvitation : BaseEntity
{
    public string BoardId { get; set; } = string.Empty;
    public string InviterId { get; set; } = string.Empty;
    public string InviteeId { get; set; } = string.Empty;
    public string Role { get; set; } = "Member";
    public string Message { get; set; } = string.Empty;
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public DateTime? ExpiresAt { get; set; }
    public DateTime? RespondedAt { get; set; }

    // Navigation properties
    public Board? Board { get; set; }
    public User? Inviter { get; set; }
    public User? Invitee { get; set; }
}