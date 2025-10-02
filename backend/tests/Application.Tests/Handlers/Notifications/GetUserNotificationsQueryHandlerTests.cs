using Application.DTOs;
using Application.Handlers.Notifications;
using Application.Queries.Notifications;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Application.Tests.Handlers.Notifications;

public class GetUserNotificationsQueryHandlerTests
{
    private readonly Mock<NotificationRepository> _mockNotificationRepository;
    private readonly GetUserNotificationsQueryHandler _handler;

    public GetUserNotificationsQueryHandlerTests()
    {
        _mockNotificationRepository = new Mock<NotificationRepository>();
        _handler = new GetUserNotificationsQueryHandler(_mockNotificationRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllNotifications_WhenNoFiltersApplied()
    {
        // Arrange
        var userId = "user123";
        var notifications = new List<Notification>
        {
            new Notification
            {
                Id = "notif1",
                UserId = userId,
                Type = NotificationType.InvitationAccepted,
                Title = "Invitación aceptada",
                Message = "Juan aceptó tu invitación",
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new Notification
            {
                Id = "notif2",
                UserId = userId,
                Type = NotificationType.InvitationRejected,
                Title = "Invitación rechazada",
                Message = "María rechazó tu invitación",
                IsRead = true,
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            }
        };

        _mockNotificationRepository
            .Setup(repo => repo.GetUserNotificationsAsync(userId, It.IsAny<bool>(), It.IsAny<int>()))
            .ReturnsAsync(notifications);

        var query = new GetUserNotificationsQuery
        {
            UserId = userId,
            UnreadOnly = null,
            Limit = null
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Id.Should().Be("notif1");
        result[1].Id.Should().Be("notif2");
        
        _mockNotificationRepository.Verify(
            repo => repo.GetUserNotificationsAsync(userId, It.IsAny<bool>(), It.IsAny<int>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyUnreadNotifications_WhenUnreadOnlyIsTrue()
    {
        // Arrange
        var userId = "user123";
        var unreadNotifications = new List<Notification>
        {
            new Notification
            {
                Id = "notif1",
                UserId = userId,
                Type = NotificationType.InvitationAccepted,
                Title = "Invitación aceptada",
                Message = "Juan aceptó tu invitación",
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            }
        };

        _mockNotificationRepository
            .Setup(repo => repo.GetUserNotificationsAsync(userId, true, It.IsAny<int>()))
            .ReturnsAsync(unreadNotifications);

        var query = new GetUserNotificationsQuery
        {
            UserId = userId,
            UnreadOnly = true,
            Limit = null
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].IsRead.Should().BeFalse();
        
        _mockNotificationRepository.Verify(
            repo => repo.GetUserNotificationsAsync(userId, true, It.IsAny<int>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldRespectLimit_WhenLimitIsSpecified()
    {
        // Arrange
        var userId = "user123";
        var limit = 5;
        var notifications = new List<Notification>
        {
            new Notification
            {
                Id = "notif1",
                UserId = userId,
                Type = NotificationType.InvitationAccepted,
                Title = "Notificación 1",
                Message = "Mensaje 1",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockNotificationRepository
            .Setup(repo => repo.GetUserNotificationsAsync(userId, It.IsAny<bool>(), limit))
            .ReturnsAsync(notifications);

        var query = new GetUserNotificationsQuery
        {
            UserId = userId,
            UnreadOnly = null,
            Limit = limit
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        
        _mockNotificationRepository.Verify(
            repo => repo.GetUserNotificationsAsync(userId, It.IsAny<bool>(), limit),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoNotificationsExist()
    {
        // Arrange
        var userId = "user123";
        var emptyList = new List<Notification>();

        _mockNotificationRepository
            .Setup(repo => repo.GetUserNotificationsAsync(userId, It.IsAny<bool>(), It.IsAny<int>()))
            .ReturnsAsync(emptyList);

        var query = new GetUserNotificationsQuery
        {
            UserId = userId,
            UnreadOnly = null,
            Limit = null
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        
        _mockNotificationRepository.Verify(
            repo => repo.GetUserNotificationsAsync(userId, It.IsAny<bool>(), It.IsAny<int>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldMapNotificationPropertiesCorrectly()
    {
        // Arrange
        var userId = "user123";
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var data = new Dictionary<string, object>
        {
            { "boardId", "board123" },
            { "invitationId", "inv456" },
            { "TaskId", "task789" }
        };

        var notifications = new List<Notification>
        {
            new Notification
            {
                Id = "notif1",
                UserId = userId,
                Type = NotificationType.InvitationAccepted,
                Title = "Test Title",
                Message = "Test Message",
                IsRead = false,
                Data = data,
                CreatedAt = createdAt
            }
        };

        _mockNotificationRepository
            .Setup(repo => repo.GetUserNotificationsAsync(userId, It.IsAny<bool>(), It.IsAny<int>()))
            .ReturnsAsync(notifications);

        var query = new GetUserNotificationsQuery
        {
            UserId = userId
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        
        var dto = result[0];
        dto.Id.Should().Be("notif1");
        dto.UserId.Should().Be(userId);
        dto.Type.Should().Be("InvitationAccepted");
        dto.Title.Should().Be("Test Title");
        dto.Message.Should().Be("Test Message");
        dto.IsRead.Should().BeFalse();
        dto.TaskId.Should().Be("task789");
        dto.Data.Should().NotBeNull();
        dto.Data.Should().ContainKey("boardId");
        dto.CreatedAt.Should().Be(createdAt);
    }

    [Theory]
    [InlineData(true, 10)]
    [InlineData(false, 20)]
    [InlineData(null, 50)]
    public async Task Handle_ShouldHandleDifferentParameterCombinations(bool? unreadOnly, int? limit)
    {
        // Arrange
        var userId = "user123";
        var notifications = new List<Notification>();

        _mockNotificationRepository
            .Setup(repo => repo.GetUserNotificationsAsync(userId, It.IsAny<bool>(), It.IsAny<int>()))
            .ReturnsAsync(notifications);

        var query = new GetUserNotificationsQuery
        {
            UserId = userId,
            UnreadOnly = unreadOnly,
            Limit = limit
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        
        _mockNotificationRepository.Verify(
            repo => repo.GetUserNotificationsAsync(userId, It.IsAny<bool>(), It.IsAny<int>()),
            Times.Once
        );
    }
}
