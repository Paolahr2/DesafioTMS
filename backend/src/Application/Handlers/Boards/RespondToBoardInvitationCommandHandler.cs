using Application.Commands.Boards;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Application.Services;

namespace Application.Handlers.Boards;

public class RespondToInvitationCommandHandler : IRequestHandler<RespondToInvitationCommand, bool>  
{
    private readonly BoardInvitationRepository _invitationRepository;
    private readonly BoardRepository _boardRepository;
    private readonly UserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly NotificationRepository _notificationRepository;

    public RespondToInvitationCommandHandler(
        BoardInvitationRepository invitationRepository,
        BoardRepository boardRepository,
        UserRepository userRepository,
        IEmailService emailService,
        NotificationRepository notificationRepository)
    {
        _invitationRepository = invitationRepository;
        _boardRepository = boardRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _notificationRepository = notificationRepository;
    }

    public async Task<bool> Handle(RespondToInvitationCommand request, CancellationToken cancellationToken)
    {
        var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);
        if (invitation == null)
            throw new KeyNotFoundException("Invitación no encontrada");

        if (invitation.InviteeId != request.UserId)
            throw new UnauthorizedAccessException("No tienes permisos para responder a esta invitación");     

        if (invitation.Status != Domain.Enums.InvitationStatus.Pending)
            throw new InvalidOperationException("Esta invitación ya ha sido respondida");

        // Verificar que la invitación no haya expirado
        if (invitation.ExpiresAt.HasValue && invitation.ExpiresAt.Value < DateTime.UtcNow)
        {
            invitation.Status = Domain.Enums.InvitationStatus.Expired;
            await _invitationRepository.UpdateAsync(invitation);
            throw new InvalidOperationException("Esta invitación ha expirado");
        }

        invitation.Status = request.Accept ? Domain.Enums.InvitationStatus.Accepted : Domain.Enums.InvitationStatus.Rejected;
        invitation.RespondedAt = DateTime.UtcNow;

        await _invitationRepository.UpdateAsync(invitation);

        // Enviar notificación por email al invitador (tanto aceptación como rechazo)
        try
        {
            var responder = await _userRepository.GetByIdAsync(request.UserId);
            var inviter = await _userRepository.GetByIdAsync(invitation.InviterId);
            var board = await _boardRepository.GetByIdAsync(invitation.BoardId);
            
            if (responder != null && inviter != null && board != null)
            {
                Console.WriteLine($"Procesando respuesta de invitación: Accept={request.Accept}, Responder={responder.Username}, Inviter={inviter.Username}, Board={board.Title}");
                
                if (request.Accept)
                {
                    // Agregar al usuario como miembro del tablero
                    if (!board.MemberIds.Contains(request.UserId))
                    {
                        board.MemberIds.Add(request.UserId);
                        await _boardRepository.UpdateAsync(board);
                    }

                    // Enviar email
                    await _emailService.SendInvitationAcceptedNotificationAsync(
                        inviter.Email, 
                        responder.Username, 
                        board.Title);

                    // Crear notificación en la aplicación
                    var notification = new Domain.Entities.Notification
                    {
                        UserId = inviter.Id,
                        Type = Domain.Enums.NotificationType.InvitationAccepted,
                        Title = "Invitación aceptada",
                        Message = $"{responder.Username} aceptó tu invitación para colaborar en el tablero \"{board.Title}\"",
                        Data = new Dictionary<string, object>
                        {
                            { "boardId", board.Id },
                            { "BoardTitle", board.Title },
                            { "AccepterUserId", responder.Id },
                            { "AccepterUsername", responder.Username }
                        }
                    };
                    await _notificationRepository.CreateAsync(notification);
                    Console.WriteLine($"Notificación de aceptación creada para usuario {inviter.Username}");
                }
                else
                {
                    // Enviar email
                    await _emailService.SendInvitationRejectedNotificationAsync(
                        inviter.Email, 
                        responder.Username, 
                        board.Title);

                    // Crear notificación en la aplicación
                    var notification = new Domain.Entities.Notification
                    {
                        UserId = inviter.Id,
                        Type = Domain.Enums.NotificationType.InvitationRejected,
                        Title = "Invitación rechazada",
                        Message = $"{responder.Username} rechazó tu invitación para colaborar en el tablero \"{board.Title}\"",
                        Data = new Dictionary<string, object>
                        {
                            { "BoardId", board.Id },
                            { "BoardTitle", board.Title },
                            { "RejecterUserId", responder.Id },
                            { "RejecterUsername", responder.Username }
                        }
                    };
                    await _notificationRepository.CreateAsync(notification);
                    Console.WriteLine($"Notificación de rechazo creada para usuario {inviter.Username}");
                }
            }
            else
            {
                Console.WriteLine($"Error: No se pudieron obtener los datos necesarios - Responder: {responder?.Username}, Inviter: {inviter?.Username}, Board: {board?.Title}");
            }
        }
        catch (Exception ex)
        {
            // Log the error but don't fail the invitation response
            Console.WriteLine($"Error sending invitation notification: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        return true;
    }
}