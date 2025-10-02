using MediatR;

namespace Application.Commands.Notifications;

public class MarkNotificationAsReadCommand : IRequest<bool>
{
    public string NotificationId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}
