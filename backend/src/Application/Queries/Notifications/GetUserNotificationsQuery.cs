using MediatR;
using Application.DTOs;

namespace Application.Queries.Notifications;

public class GetUserNotificationsQuery : IRequest<List<NotificationDTO>>
{
    public string UserId { get; set; } = string.Empty;
    public bool? UnreadOnly { get; set; }
    public int? Limit { get; set; }
}
