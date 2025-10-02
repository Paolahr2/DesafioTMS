using Domain.Entities;

namespace Domain.Interfaces;

// Repositorio para listas con métodos específicos de consulta
public interface ListRepository : GenericRepository<List>
{
    Task<IEnumerable<List>> GetListsByBoardIdAsync(string boardId);
    Task<List?> GetListByIdAsync(string id);
    Task<bool> DeleteListsByBoardIdAsync(string boardId);
}