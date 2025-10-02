using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

// DTOs para tableros
public class BoardDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public List<string> MemberIds { get; set; } = new();
    public string Color { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
    public bool IsPublic { get; set; }
    public List<string> Columns { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateBoardDto
{
    [Required(ErrorMessage = "El título es requerido")]
    [StringLength(100, ErrorMessage = "El título no puede exceder 100 caracteres")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string Description { get; set; } = string.Empty;

    public string Color { get; set; } = "#3B82F6";
    public bool IsPublic { get; set; } = false;
}

public class UpdateBoardDto
{
    [StringLength(100, ErrorMessage = "El título no puede exceder 100 caracteres")]
    public string? Title { get; set; }

    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Description { get; set; }

    public string? Color { get; set; }
    public bool? IsPublic { get; set; }
}

// DTOs para invitaciones y colaboración
public class InviteUserToBoardDto
{
    public string? Email { get; set; }
    public string? Username { get; set; }
    public string Role { get; set; } = "Member";
    public string? Message { get; set; }
    // public DateTime? ExpiresAt { get; set; } // Comentado temporalmente
}

public class BoardInvitationDto
{
    public string Id { get; set; } = string.Empty;
    public string BoardId { get; set; } = string.Empty;
    public string BoardTitle { get; set; } = string.Empty;
    public string InvitedUserId { get; set; } = string.Empty;
    public string InvitedUserEmail { get; set; } = string.Empty;
    public string InvitedUserName { get; set; } = string.Empty;
    public string InvitedById { get; set; } = string.Empty;
    public string InvitedByName { get; set; } = string.Empty;
    public string Role { get; set; } = "Member";
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class RespondToInvitationDto
{
    [Required(ErrorMessage = "Debe especificar si acepta o rechaza la invitación")]
    public bool Accept { get; set; }
}

public class BoardMemberDto
{
    public string UserId { get; set; } = string.Empty;
    public string BoardId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public string? UserAvatar { get; set; }
    public string? FullName { get; set; }
}
