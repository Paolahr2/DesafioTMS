using Application.Commands.Notifications;
using Application.Queries.Notifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtiene las notificaciones del usuario autenticado
    /// </summary>
    /// <param name="unreadOnly">Filtrar solo notificaciones no leídas</param>
    /// <param name="limit">Límite de notificaciones a retornar</param>
    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] bool? unreadOnly, [FromQuery] int? limit)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var query = new GetUserNotificationsQuery
        {
            UserId = userId,
            UnreadOnly = unreadOnly,
            Limit = limit
        };

        var notifications = await _mediator.Send(query);
        return Ok(notifications);
    }

    /// <summary>
    /// Marca una notificación como leída
    /// </summary>
    /// <param name="notificationId">ID de la notificación</param>
    [HttpPut("{notificationId}/mark-as-read")]
    public async Task<IActionResult> MarkAsRead(string notificationId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        try
        {
            var command = new MarkNotificationAsReadCommand
            {
                NotificationId = notificationId,
                UserId = userId
            };

            await _mediator.Send(command);
            return Ok(new { success = true });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Obtiene el conteo de notificaciones no leídas
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var query = new GetUserNotificationsQuery
        {
            UserId = userId,
            UnreadOnly = true
        };

        var notifications = await _mediator.Send(query);
        return Ok(new { count = notifications.Count });
    }
}
