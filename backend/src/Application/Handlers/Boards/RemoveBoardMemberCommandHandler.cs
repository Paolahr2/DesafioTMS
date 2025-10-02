using Application.Commands.Boards;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Handlers.Boards;

public class RemoveBoardMemberCommandHandler : IRequestHandler<RemoveBoardMemberCommand, bool>
{
    private readonly BoardRepository _boardRepository;

    public RemoveBoardMemberCommandHandler(BoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<bool> Handle(RemoveBoardMemberCommand request, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetByIdAsync(request.BoardId);
        if (board == null)
            throw new KeyNotFoundException("Tablero no encontrado");

        // Verificar permisos: solo el owner o el propio usuario pueden remover miembros
        if (board.OwnerId != request.RemovedById && request.MemberId != request.RemovedById)
            throw new UnauthorizedAccessException("No tienes permisos para remover miembros de este tablero");

        // No permitir que el owner se remueva a sí mismo
        if (request.MemberId == board.OwnerId)
            throw new InvalidOperationException("No puedes remover al propietario del tablero");

        // Remover el miembro
        if (!board.MemberIds.Contains(request.MemberId))
            throw new KeyNotFoundException("El usuario no es miembro de este tablero");

        board.MemberIds.Remove(request.MemberId);
        await _boardRepository.UpdateAsync(board);

        return true;
    }
}