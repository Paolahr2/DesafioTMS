using Application.Commands.Boards;
using Domain.Interfaces;
using MediatR;

namespace Application.Handlers.Boards;

/// <summary>
/// Handler para DeleteBoardCommand
/// </summary>
public class DeleteBoardCommandHandler : IRequestHandler<DeleteBoardCommand, bool>
{
    private readonly BoardRepository _boardRepository;

    public DeleteBoardCommandHandler(BoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<bool> Handle(DeleteBoardCommand request, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetByIdAsync(request.Id);
        if (board == null || board.OwnerId != request.UserId)
        {
            throw new UnauthorizedAccessException("No tienes permisos para eliminar este tablero");
        }

        await _boardRepository.DeleteAsync(request.Id);
        return true;
    }
}
