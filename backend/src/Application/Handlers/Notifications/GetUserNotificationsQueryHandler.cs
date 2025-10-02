using Application.DTOs;
using Application.Queries.Notifications;
using Domain.Interfaces;
using MediatR;

namespace Application.Handlers.Notifications;

public class GetUserNotificationsQueryHandler : IRequestHandler<GetUserNotificationsQuery, List<NotificationDTO>>
{
    private readonly NotificationRepository _notificationRepository;

    public GetUserNotificationsQueryHandler(NotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<List<NotificationDTO>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notifications = await _notificationRepository.GetUserNotificationsAsync(
            request.UserId,
            request.UnreadOnly ?? false,
            request.Limit ?? 50
        );

        return notifications.Select(n => new NotificationDTO
        {
            Id = n.Id,
            UserId = n.UserId,
            Type = n.Type.ToString(),
            Title = n.Title,
            Message = n.Message,
            IsRead = n.IsRead,
            TaskId = n.Data.ContainsKey("TaskId") ? n.Data["TaskId"]?.ToString() : null,
            Data = n.Data,
            CreatedAt = n.CreatedAt
        }).ToList();
    }
}
