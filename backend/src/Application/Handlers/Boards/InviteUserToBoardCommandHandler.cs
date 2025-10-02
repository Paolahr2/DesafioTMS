using Application.Commands.Boards;
using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;

namespace Application.Handlers.Boards;

public class InviteUserToBoardCommandHandler : IRequestHandler<InviteUserToBoardCommand, BoardInvitationDto>
{
    private readonly BoardInvitationRepository _invitationRepository;
    private readonly BoardRepository _boardRepository;
    private readonly UserRepository _userRepository;

    public InviteUserToBoardCommandHandler(
        BoardInvitationRepository invitationRepository,
        BoardRepository boardRepository,
        UserRepository userRepository)
    {
        _invitationRepository = invitationRepository;
        _boardRepository = boardRepository;
        _userRepository = userRepository;
    }

    public async Task<BoardInvitationDto> Handle(InviteUserToBoardCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Console.WriteLine($"[DEBUG] Procesando invitación para board {request.BoardId}, email {request.InvitedUserEmail}, username {request.InvitedUsername}");

            // Verificar que el tablero existe y el usuario tiene permisos
            var board = await _boardRepository.GetByIdAsync(request.BoardId);
            if (board == null)
                throw new KeyNotFoundException("Tablero no encontrado");

            Console.WriteLine($"[DEBUG] Tablero encontrado: {board.Title}");

            if (board.OwnerId != request.InvitedById && !board.MemberIds.Contains(request.InvitedById))
                throw new UnauthorizedAccessException("No tienes permisos para invitar usuarios a este tablero");

            Console.WriteLine($"[DEBUG] Permisos verificados para usuario {request.InvitedById}");

            // Verificar que el usuario invitado existe (por email o username)
            Domain.Entities.User? invitee = null;
            if (!string.IsNullOrEmpty(request.InvitedUserEmail))
            {
                invitee = await _userRepository.GetByEmailAsync(request.InvitedUserEmail);
            }
            else if (!string.IsNullOrEmpty(request.InvitedUsername))
            {
                invitee = await _userRepository.GetByUsernameAsync(request.InvitedUsername);
            }

            if (invitee == null)
                throw new KeyNotFoundException("Usuario invitado no encontrado");

            Console.WriteLine($"[DEBUG] Usuario invitado encontrado: {invitee.Email} (ID: {invitee.Id})");

            // Verificar que el usuario no se esté invitando a sí mismo
            if (invitee.Id == request.InvitedById)
                throw new InvalidOperationException("No puedes invitarte a ti mismo");

            // Verificar que el usuario no esté ya invitado o sea miembro
            if (board.MemberIds.Contains(invitee.Id))
                throw new InvalidOperationException("El usuario ya es miembro de este tablero");

            Console.WriteLine($"[DEBUG] Verificando invitaciones existentes...");
            var existingInvitations = await _invitationRepository.GetInvitationsByBoardIdAsync(request.BoardId);
            if (existingInvitations == null)
            {
                Console.WriteLine($"[DEBUG] existingInvitations es null, inicializando lista vacía");
                existingInvitations = new List<BoardInvitation>();
            }
            Console.WriteLine($"[DEBUG] Encontradas {existingInvitations.Count()} invitaciones para el tablero");

            if (existingInvitations.Any(i => i.InviteeId == invitee.Id && i.Status == InvitationStatus.Pending))
                throw new InvalidOperationException("Ya existe una invitación pendiente para este usuario");

            Console.WriteLine($"[DEBUG] Creando invitación...");

            // Crear la invitación
            var invitation = new BoardInvitation
            {
                BoardId = request.BoardId,
                InviterId = request.InvitedById,
                InviteeId = invitee.Id,
                Role = request.Role,
                Message = request.Message ?? string.Empty,
                Status = InvitationStatus.Pending,
                ExpiresAt = DateTime.UtcNow.AddDays(7) // Expira en 7 días por defecto
            };

            await _invitationRepository.CreateAsync(invitation);
            Console.WriteLine($"[DEBUG] Invitación creada con ID: {invitation.Id}");

            // Obtener información adicional para el DTO
            var inviter = await _userRepository.GetByIdAsync(request.InvitedById);

            Console.WriteLine($"[DEBUG] Retornando DTO de invitación");

            // Retornar el DTO
            return new BoardInvitationDto
            {
                Id = invitation.Id,
                BoardId = invitation.BoardId,
                BoardTitle = board.Title,
                InvitedUserId = invitation.InviteeId,
                InvitedUserEmail = invitee.Email,
                InvitedUserName = invitee.Username,
                InvitedById = invitation.InviterId,
                InvitedByName = inviter?.Username ?? "Usuario desconocido",
                Role = invitation.Role,
                Status = invitation.Status.ToString(),
                Message = invitation.Message,
                CreatedAt = invitation.CreatedAt,
                ExpiresAt = invitation.ExpiresAt
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Error procesando invitación: {ex.Message}");
            Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}