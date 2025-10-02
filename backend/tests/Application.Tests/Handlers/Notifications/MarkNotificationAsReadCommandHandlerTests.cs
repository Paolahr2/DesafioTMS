using Application.Commands.Notifications;
using Application.Handlers.Notifications;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Application.Tests.Handlers.Notifications;

public class MarkNotificationAsReadCommandHandlerTests
{
    private readonly Domain.Interfaces.NotificationRepository _notificationRepository = Substitute.For<Domain.Interfaces.NotificationRepository>();
    private readonly MarkNotificationAsReadCommandHandler _handler;

    public MarkNotificationAsReadCommandHandlerTests()
    {
        _handler = new MarkNotificationAsReadCommandHandler(_notificationRepository);
    }

    [Fact]
    public async Task Handle_ShouldMarkNotificationAsRead_WhenValidRequest()
    {
        // Arrange
        var notificationId = "notif123";
        var userId = "user456";
        var notification = new Notification
        {
            Id = notificationId,
            UserId = userId,
            Type = NotificationType.InvitationAccepted,
            Title = "Test Notification",
            Message = "Test Message",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _notificationRepository.GetByIdAsync(Arg.Any<string>()).Returns(Task.FromResult<Notification?>(notification));
        _notificationRepository.UpdateAsync(Arg.Any<Notification>()).Returns(Task.FromResult(notification));

        var command = new MarkNotificationAsReadCommand
        {
            NotificationId = notificationId,
            UserId = userId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        notification.IsRead.Should().BeTrue();
        notification.ReadAt.Should().NotBeNull();
        notification.ReadAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _notificationRepository.Received().GetByIdAsync(notificationId);
        
        _notificationRepository.Received().UpdateAsync(Arg.Is<Notification>(n => 
            n.Id == notificationId && 
            n.IsRead == true && 
            n.ReadAt != null
        ));
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenNotificationDoesNotExist()
    {
        // Arrange
        var notificationId = "nonexistent";
        var userId = "user456";

        _notificationRepository.GetByIdAsync(Arg.Any<string>()).Returns(Task.FromResult<Notification?>(null));

        var command = new MarkNotificationAsReadCommand
        {
            NotificationId = notificationId,
            UserId = userId
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _handler.Handle(command, CancellationToken.None)
        );

        _notificationRepository.Received().GetByIdAsync(notificationId);
        
        _notificationRepository.DidNotReceive().UpdateAsync(Arg.Any<Notification>());
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserDoesNotOwnNotification()
    {
        // Arrange
        var notificationId = "notif123";
        var ownerId = "owner123";
        var unauthorizedUserId = "hacker456";
        
        var notification = new Notification
        {
            Id = notificationId,
            UserId = ownerId,
            Type = NotificationType.InvitationAccepted,
            Title = "Test Notification",
            Message = "Test Message",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _notificationRepository.GetByIdAsync(notificationId).Returns(Task.FromResult(notification));

        var command = new MarkNotificationAsReadCommand
        {
            NotificationId = notificationId,
            UserId = unauthorizedUserId
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _handler.Handle(command, CancellationToken.None)
        );

        _notificationRepository.Received().GetByIdAsync(notificationId);
        
        _notificationRepository.DidNotReceive().UpdateAsync(Arg.Any<Notification>());
    }

    [Fact]
    public async Task Handle_ShouldReturnTrue_WhenNotificationAlreadyRead()
    {
        // Arrange
        var notificationId = "notif123";
        var userId = "user456";
        var previousReadAt = DateTime.UtcNow.AddHours(-1);
        
        var notification = new Notification
        {
            Id = notificationId,
            UserId = userId,
            Type = NotificationType.InvitationAccepted,
            Title = "Test Notification",
            Message = "Test Message",
            IsRead = true,
            ReadAt = previousReadAt,
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };

        _notificationRepository.GetByIdAsync(notificationId).Returns(Task.FromResult(notification));

        _notificationRepository.UpdateAsync(Arg.Any<Notification>()).Returns(Task.FromResult(notification));

        var command = new MarkNotificationAsReadCommand
        {
            NotificationId = notificationId,
            UserId = userId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        notification.IsRead.Should().BeTrue();
        // La fecha de lectura debería actualizarse
        notification.ReadAt.Should().BeAfter(previousReadAt);

        _notificationRepository.Received().UpdateAsync(Arg.Any<Notification>());
    }

    [Fact]
    public async Task Handle_ShouldNotModifyOtherProperties_WhenMarkingAsRead()
    {
        // Arrange
        var notificationId = "notif123";
        var userId = "user456";
        var originalTitle = "Original Title";
        var originalMessage = "Original Message";
        var originalCreatedAt = DateTime.UtcNow.AddDays(-1);
        var originalData = new Dictionary<string, object> { { "key", "value" } };

        var notification = new Notification
        {
            Id = notificationId,
            UserId = userId,
            Type = NotificationType.InvitationAccepted,
            Title = originalTitle,
            Message = originalMessage,
            IsRead = false,
            Data = originalData,
            CreatedAt = originalCreatedAt
        };

        _notificationRepository.GetByIdAsync(notificationId).Returns(Task.FromResult(notification));

        _notificationRepository.UpdateAsync(Arg.Any<Notification>()).Returns(Task.FromResult(notification));

        var command = new MarkNotificationAsReadCommand
        {
            NotificationId = notificationId,
            UserId = userId
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        notification.Title.Should().Be(originalTitle);
        notification.Message.Should().Be(originalMessage);
        notification.CreatedAt.Should().Be(originalCreatedAt);
        notification.Data.Should().BeSameAs(originalData);
    }
}
