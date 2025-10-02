using Application.Commands.Notifications;
using Domain.Interfaces;
using MediatR;

namespace Application.Handlers.Notifications;

public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, bool>
{
    private readonly Domain.Interfaces.NotificationRepository _notificationRepository;

    public MarkNotificationAsReadCommandHandler(Domain.Interfaces.NotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<bool> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _notificationRepository.GetByIdAsync(request.NotificationId);
        
        if (notification == null)
            throw new KeyNotFoundException("Notificación no encontrada");
            
        if (notification.UserId != request.UserId)
            throw new UnauthorizedAccessException("No tienes permisos para modificar esta notificación");

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        
        await _notificationRepository.UpdateAsync(notification);
        
        return true;
    }
}
