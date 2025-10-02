using Application.Commands.Lists;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Handlers.Lists;

/// <summary>
/// Handler para DeleteListCommand
/// </summary>
public class DeleteListCommandHandler : IRequestHandler<DeleteListCommand, bool>
{
    private readonly ListRepository _listRepository;
    private readonly BoardRepository _boardRepository;

    public DeleteListCommandHandler(ListRepository listRepository, BoardRepository boardRepository)
    {
        _listRepository = listRepository;
        _boardRepository = boardRepository;
    }

    public async Task<bool> Handle(DeleteListCommand request, CancellationToken cancellationToken)
    {
        var list = await _listRepository.GetByIdAsync(request.Id);
        if (list == null)
        {
            return false;
        }

        // Verificar que el usuario tenga acceso al board
        var hasAccess = await _boardRepository.UserHasAccessAsync(list.BoardId, request.UserId);
        if (!hasAccess)
        {
            throw new UnauthorizedAccessException("No tienes acceso a este tablero");
        }

        // NOTA: Las listas de checklist NO contienen tareas.
        // Las tareas están asociadas a estados (Por Hacer, En Progreso, Hecho), no a listas.
        // Por lo tanto, eliminar una lista de checklist NO debe eliminar tareas.
        
        await _listRepository.DeleteAsync(request.Id);
        return true;
    }
}