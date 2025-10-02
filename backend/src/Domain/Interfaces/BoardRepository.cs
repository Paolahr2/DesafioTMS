using Domain.Entities;

namespace Domain.Interfaces;

// Repositorio para tableros con métodos específicos de consulta
public interface BoardRepository : GenericRepository<Board>
{
    Task<IEnumerable<Board>> GetBoardsByOwnerIdAsync(string ownerId);
    Task<IEnumerable<Board>> GetBoardsByMemberIdAsync(string memberId);
    Task<IEnumerable<Board>> GetUserBoardsAsync(string userId);
    Task<IEnumerable<Board>> GetPublicBoardsAsync(int page = 1, int pageSize = 10);
    Task<bool> UserHasAccessAsync(string boardId, string userId);
    Task<IEnumerable<Board>> SearchBoardsAsync(string searchTerm, string userId);
    Task<bool> AddMemberAsync(string boardId, string userId);
    Task<bool> RemoveMemberAsync(string boardId, string userId);
    Task<bool> IsMemberAsync(string boardId, string userId);
}
