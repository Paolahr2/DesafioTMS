using Domain.Entities;
using Domain.Interfaces;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class BoardInvitationRepository : GenericRepository<BoardInvitation>, Domain.Interfaces.BoardInvitationRepository
{
    private new readonly IMongoCollection<BoardInvitation> _collection;

    public BoardInvitationRepository(IMongoDatabase database)
        : base(database, "BoardInvitations")
    {
        _collection = database.GetCollection<BoardInvitation>("BoardInvitations");
    }

    public async Task<IEnumerable<BoardInvitation>> GetPendingInvitationsByUserIdAsync(string userId)
    {
        return await _collection.Find(i => i.InviteeId == userId && i.Status == Domain.Enums.InvitationStatus.Pending).ToListAsync();
    }

    public async Task<IEnumerable<BoardInvitation>> GetInvitationsByBoardIdAsync(string boardId)
    {
        var result = await _collection.Find(i => i.BoardId == boardId).ToListAsync();
        return result ?? new List<BoardInvitation>();
    }

    public async Task<bool> UpdateStatusAsync(string invitationId, Domain.Enums.InvitationStatus status)
    {
        var update = Builders<BoardInvitation>.Update
            .Set(i => i.Status, status)
            .Set(i => i.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(i => i.Id == invitationId, update);
        return result.ModifiedCount > 0;
    }
}