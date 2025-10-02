using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces;

// Repositorio para invitaciones a tableros
public interface BoardInvitationRepository : GenericRepository<BoardInvitation>
{
    Task<IEnumerable<BoardInvitation>> GetPendingInvitationsByUserIdAsync(string userId);
    Task<IEnumerable<BoardInvitation>> GetInvitationsByBoardIdAsync(string boardId);
    Task<bool> UpdateStatusAsync(string invitationId, InvitationStatus status);
}