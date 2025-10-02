using Application.Commands.Boards;
using Application.Handlers.Boards;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Application.Tests.Handlers.Boards;

public class RespondToBoardInvitationCommandHandlerTests
{
    private readonly Mock<BoardInvitationRepository> _mockInvitationRepository;
    private readonly Mock<BoardRepository> _mockBoardRepository;
    private readonly Mock<UserRepository> _mockUserRepository;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<NotificationRepository> _mockNotificationRepository;
    private readonly RespondToInvitationCommandHandler _handler;

    public RespondToBoardInvitationCommandHandlerTests()
    {
        _mockInvitationRepository = new Mock<BoardInvitationRepository>();
        _mockBoardRepository = new Mock<BoardRepository>();
        _mockUserRepository = new Mock<UserRepository>();
        _mockEmailService = new Mock<IEmailService>();
        _mockNotificationRepository = new Mock<NotificationRepository>();

        _handler = new RespondToInvitationCommandHandler(
            _mockInvitationRepository.Object,
            _mockBoardRepository.Object,
            _mockUserRepository.Object,
            _mockEmailService.Object,
            _mockNotificationRepository.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateNotification_WhenInvitationIsAccepted()
    {
        // Arrange
        var invitationId = "inv123";
        var inviterId = "inviter456";
        var inviteeId = "invitee789";
        var boardId = "board999";
        var boardTitle = "Mi Tablero";
        var responderUserName = "Juan Pérez";

        var invitation = new BoardInvitation
        {
            Id = invitationId,
            BoardId = boardId,
            InviterId = inviterId,
            InviteeId = inviteeId,
            Status = InvitationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var board = new Board
        {
            Id = boardId,
            Title = boardTitle,
            OwnerId = inviterId,
            MemberIds = new List<string> { inviterId }
        };

        var responder = new User
        {
            Id = inviteeId,
            Username = responderUserName,
            Email = "juan@example.com"
        };

        var inviter = new User
        {
            Id = inviterId,
            Username = "María",
            Email = "maria@example.com"
        };

        _mockInvitationRepository
            .Setup(repo => repo.GetByIdAsync(invitationId))
            .ReturnsAsync(invitation);

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        _mockUserRepository
            .Setup(repo => repo.GetByIdAsync(inviteeId))
            .ReturnsAsync(responder);

        _mockUserRepository
            .Setup(repo => repo.GetByIdAsync(inviterId))
            .ReturnsAsync(inviter);

        _mockInvitationRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<BoardInvitation>()))
            .ReturnsAsync(It.IsAny<BoardInvitation>());

        _mockBoardRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<Board>()))
            .ReturnsAsync(It.IsAny<Board>());

        _mockEmailService
            .Setup(service => service.SendInvitationAcceptedNotificationAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _mockNotificationRepository
            .Setup(repo => repo.CreateAsync(It.IsAny<Notification>()))
            .ReturnsAsync(It.IsAny<Notification>());

        var command = new RespondToInvitationCommand
        {
            InvitationId = invitationId,
            UserId = inviteeId,
            Accept = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        // Verificar que se creó la notificación con los datos correctos
        _mockNotificationRepository.Verify(
            repo => repo.CreateAsync(It.Is<Notification>(n =>
                n.UserId == inviterId &&
                n.Type == NotificationType.InvitationAccepted &&
                n.Title == "Invitación aceptada" &&
                n.Message.Contains(responderUserName) &&
                n.Message.Contains(boardTitle) &&
                n.IsRead == false &&
                n.Data != null &&
                n.Data.ContainsKey("boardId") &&
                n.Data["boardId"].ToString() == boardId
            )),
            Times.Once,
            "Debe crear una notificación cuando se acepta la invitación"
        );

        // Verificar que se agregó al usuario al tablero
        _mockBoardRepository.Verify(
            repo => repo.UpdateAsync(It.Is<Board>(b => 
                b.MemberIds.Contains(inviteeId)
            )),
            Times.Once
        );

        // Verificar que se envió el email
        _mockEmailService.Verify(
            service => service.SendInvitationAcceptedNotificationAsync(
                inviter.Email, responderUserName, boardTitle),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateNotification_WhenInvitationIsRejected()
    {
        // Arrange
        var invitationId = "inv123";
        var inviterId = "inviter456";
        var inviteeId = "invitee789";
        var boardId = "board999";
        var boardTitle = "Mi Tablero";
        var responderUserName = "Juan Pérez";

        var invitation = new BoardInvitation
        {
            Id = invitationId,
            BoardId = boardId,
            InviterId = inviterId,
            InviteeId = inviteeId,
            Status = InvitationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var board = new Board
        {
            Id = boardId,
            Title = boardTitle,
            OwnerId = inviterId,
            MemberIds = new List<string> { inviterId }
        };

        var responder = new User
        {
            Id = inviteeId,
            Username = responderUserName,
            Email = "juan@example.com"
        };

        var inviter = new User
        {
            Id = inviterId,
            Username = "María",
            Email = "maria@example.com"
        };

        _mockInvitationRepository
            .Setup(repo => repo.GetByIdAsync(invitationId))
            .ReturnsAsync(invitation);

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        _mockUserRepository
            .Setup(repo => repo.GetByIdAsync(inviteeId))
            .ReturnsAsync(responder);

        _mockUserRepository
            .Setup(repo => repo.GetByIdAsync(inviterId))
            .ReturnsAsync(inviter);

        _mockInvitationRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<BoardInvitation>()))
            .ReturnsAsync(It.IsAny<BoardInvitation>());

        _mockEmailService
            .Setup(service => service.SendInvitationRejectedNotificationAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _mockNotificationRepository
            .Setup(repo => repo.CreateAsync(It.IsAny<Notification>()))
            .ReturnsAsync(It.IsAny<Notification>());

        var command = new RespondToInvitationCommand
        {
            InvitationId = invitationId,
            UserId = inviteeId,
            Accept = false
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        // Verificar que se creó la notificación con los datos correctos
        _mockNotificationRepository.Verify(
            repo => repo.CreateAsync(It.Is<Notification>(n =>
                n.UserId == inviterId &&
                n.Type == NotificationType.InvitationRejected &&
                n.Title == "Invitación rechazada" &&
                n.Message.Contains(responderUserName) &&
                n.Message.Contains(boardTitle) &&
                n.IsRead == false
            )),
            Times.Once,
            "Debe crear una notificación cuando se rechaza la invitación"
        );

        // Verificar que NO se agregó al usuario al tablero
        _mockBoardRepository.Verify(
            repo => repo.UpdateAsync(It.IsAny<Board>()),
            Times.Never,
            "No debe agregar al usuario al tablero cuando rechaza"
        );

        // Verificar que se envió el email de rechazo
        _mockEmailService.Verify(
            service => service.SendInvitationRejectedNotificationAsync(
                inviter.Email, responderUserName, boardTitle),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenInvitationDoesNotExist()
    {
        // Arrange
        _mockInvitationRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((BoardInvitation?)null);

        var command = new RespondToInvitationCommand
        {
            InvitationId = "nonexistent",
            UserId = "user123",
            Accept = true
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None)
        );

        _mockNotificationRepository.Verify(
            repo => repo.CreateAsync(It.IsAny<Notification>()),
            Times.Never,
            "No debe crear notificación si la invitación no existe"
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedException_WhenUserIsNotInvitee()
    {
        // Arrange
        var invitation = new BoardInvitation
        {
            Id = "inv123",
            InviteeId = "correctUser",
            Status = InvitationStatus.Pending
        };

        _mockInvitationRepository
            .Setup(repo => repo.GetByIdAsync("inv123"))
            .ReturnsAsync(invitation);

        var command = new RespondToInvitationCommand
        {
            InvitationId = "inv123",
            UserId = "wrongUser",
            Accept = true
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None)
        );

        _mockNotificationRepository.Verify(
            repo => repo.CreateAsync(It.IsAny<Notification>()),
            Times.Never,
            "No debe crear notificación si el usuario no está autorizado"
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenInvitationAlreadyResponded()
    {
        // Arrange
        var invitation = new BoardInvitation
        {
            Id = "inv123",
            InviteeId = "user123",
            Status = InvitationStatus.Accepted
        };

        _mockInvitationRepository
            .Setup(repo => repo.GetByIdAsync("inv123"))
            .ReturnsAsync(invitation);

        var command = new RespondToInvitationCommand
        {
            InvitationId = "inv123",
            UserId = "user123",
            Accept = false
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None)
        );

        _mockNotificationRepository.Verify(
            repo => repo.CreateAsync(It.IsAny<Notification>()),
            Times.Never,
            "No debe crear notificación duplicada si ya fue respondida"
        );
    }
}
