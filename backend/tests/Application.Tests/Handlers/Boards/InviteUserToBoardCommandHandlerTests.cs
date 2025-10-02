using Application.Commands.Boards;
using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;
using BoardsHandler = Application.Handlers.Boards;

namespace Application.Tests.Handlers.Boards;

public class InviteUserToBoardCommandHandlerTests
{
    private readonly Mock<BoardInvitationRepository> _mockInvitationRepository;
    private readonly Mock<BoardRepository> _mockBoardRepository;
    private readonly Mock<UserRepository> _mockUserRepository;
    private readonly BoardsHandler.InviteUserToBoardCommandHandler _handler;

    public InviteUserToBoardCommandHandlerTests()
    {
        _mockInvitationRepository = new Mock<BoardInvitationRepository>();
        _mockBoardRepository = new Mock<BoardRepository>();
        _mockUserRepository = new Mock<UserRepository>();
        _handler = new BoardsHandler.InviteUserToBoardCommandHandler(
            _mockInvitationRepository.Object,
            _mockBoardRepository.Object,
            _mockUserRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateInvitation_WhenValidRequest()
    {
        // Arrange
        var ownerId = "owner123";
        var inviteeEmail = "invitee@test.com";
        var boardId = "board123";

        var board = new Board
        {
            Id = boardId,
            Title = "Test Board",
            OwnerId = ownerId,
            MemberIds = new List<string> { ownerId }
        };

        var invitee = new Domain.Entities.User
        {
            Id = "invitee456",
            Username = "inviteeuser",
            Email = inviteeEmail,
            FirstName = "Invitee",
            LastName = "User"
        };

        var owner = new Domain.Entities.User
        {
            Id = ownerId,
            Username = "owneruser",
            Email = "owner@test.com"
        };

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        _mockUserRepository
            .Setup(repo => repo.GetByEmailAsync(inviteeEmail))
            .ReturnsAsync(invitee);

        _mockUserRepository
            .Setup(repo => repo.GetByIdAsync(ownerId))
            .ReturnsAsync(owner);

        _mockInvitationRepository
            .Setup(repo => repo.GetInvitationsByBoardIdAsync(boardId))
            .ReturnsAsync(new List<BoardInvitation>());

        _mockInvitationRepository
            .Setup(repo => repo.CreateAsync(It.IsAny<BoardInvitation>()))
            .ReturnsAsync((BoardInvitation invitation) => invitation);

        var dto = new InviteUserToBoardDto
        {
            Email = inviteeEmail,
            Role = "Member",
            Message = "Join our board!"
        };

        var command = new InviteUserToBoardCommand(boardId, dto, ownerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.BoardId.Should().Be(boardId);
        result.InvitedUserId.Should().Be(invitee.Id);
        result.InvitedByName.Should().Be(owner.Username);
        result.Role.Should().Be("Member");
        result.Status.Should().Be("Pending");

        _mockInvitationRepository.Verify(repo => repo.CreateAsync(It.Is<BoardInvitation>(inv =>
            inv.BoardId == boardId &&
            inv.InviteeId == invitee.Id &&
            inv.InviterId == ownerId &&
            inv.Status == InvitationStatus.Pending
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenBoardNotFound()
    {
        // Arrange
        var boardId = "nonexistent";
        var dto = new InviteUserToBoardDto { Email = "test@test.com" };

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync((Board?)null);

        var command = new InviteUserToBoardCommand(boardId, dto, "user123");

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedException_WhenUserNotOwnerOrMember()
    {
        // Arrange
        var ownerId = "owner123";
        var unauthorizedUserId = "unauthorized456";
        var boardId = "board123";

        var board = new Board
        {
            Id = boardId,
            Title = "Test Board",
            OwnerId = ownerId,
            MemberIds = new List<string> { ownerId }
        };

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        var dto = new InviteUserToBoardDto { Email = "test@test.com" };
        var command = new InviteUserToBoardCommand(boardId, dto, unauthorizedUserId);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenInviteeNotFound()
    {
        // Arrange
        var ownerId = "owner123";
        var boardId = "board123";
        var nonExistentEmail = "nonexistent@test.com";

        var board = new Board
        {
            Id = boardId,
            Title = "Test Board",
            OwnerId = ownerId,
            MemberIds = new List<string> { ownerId }
        };

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        _mockUserRepository
            .Setup(repo => repo.GetByEmailAsync(nonExistentEmail))
            .ReturnsAsync((Domain.Entities.User?)null);

        var dto = new InviteUserToBoardDto { Email = nonExistentEmail };
        var command = new InviteUserToBoardCommand(boardId, dto, ownerId);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenUserInvitesThemselves()
    {
        // Arrange
        var userId = "user123";
        var userEmail = "user@test.com";
        var boardId = "board123";

        var board = new Board
        {
            Id = boardId,
            Title = "Test Board",
            OwnerId = userId,
            MemberIds = new List<string> { userId }
        };

        var user = new Domain.Entities.User
        {
            Id = userId,
            Username = "user",
            Email = userEmail
        };

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        _mockUserRepository
            .Setup(repo => repo.GetByEmailAsync(userEmail))
            .ReturnsAsync(user);

        var dto = new InviteUserToBoardDto { Email = userEmail };
        var command = new InviteUserToBoardCommand(boardId, dto, userId);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenUserAlreadyMember()
    {
        // Arrange
        var ownerId = "owner123";
        var memberId = "member456";
        var memberEmail = "member@test.com";
        var boardId = "board123";

        var board = new Board
        {
            Id = boardId,
            Title = "Test Board",
            OwnerId = ownerId,
            MemberIds = new List<string> { ownerId, memberId }
        };

        var member = new Domain.Entities.User
        {
            Id = memberId,
            Username = "member",
            Email = memberEmail
        };

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        _mockUserRepository
            .Setup(repo => repo.GetByEmailAsync(memberEmail))
            .ReturnsAsync(member);

        var dto = new InviteUserToBoardDto { Email = memberEmail };
        var command = new InviteUserToBoardCommand(boardId, dto, ownerId);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenPendingInvitationExists()
    {
        // Arrange
        var ownerId = "owner123";
        var inviteeId = "invitee456";
        var inviteeEmail = "invitee@test.com";
        var boardId = "board123";

        var board = new Board
        {
            Id = boardId,
            Title = "Test Board",
            OwnerId = ownerId,
            MemberIds = new List<string> { ownerId }
        };

        var invitee = new Domain.Entities.User
        {
            Id = inviteeId,
            Username = "invitee",
            Email = inviteeEmail
        };

        var existingInvitation = new BoardInvitation
        {
            Id = "inv1",
            BoardId = boardId,
            InviteeId = inviteeId,
            InviterId = ownerId,
            Status = InvitationStatus.Pending
        };

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        _mockUserRepository
            .Setup(repo => repo.GetByEmailAsync(inviteeEmail))
            .ReturnsAsync(invitee);

        _mockInvitationRepository
            .Setup(repo => repo.GetInvitationsByBoardIdAsync(boardId))
            .ReturnsAsync(new List<BoardInvitation> { existingInvitation });

        var dto = new InviteUserToBoardDto { Email = inviteeEmail };
        var command = new InviteUserToBoardCommand(boardId, dto, ownerId);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldAllowMemberToInvite_WhenMemberHasPermissions()
    {
        // Arrange
        var ownerId = "owner123";
        var memberId = "member456";
        var inviteeEmail = "invitee@test.com";
        var boardId = "board123";

        var board = new Board
        {
            Id = boardId,
            Title = "Test Board",
            OwnerId = ownerId,
            MemberIds = new List<string> { ownerId, memberId }
        };

        var invitee = new Domain.Entities.User
        {
            Id = "invitee789",
            Username = "invitee",
            Email = inviteeEmail
        };

        var member = new Domain.Entities.User
        {
            Id = memberId,
            Username = "member",
            Email = "member@test.com"
        };

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        _mockUserRepository
            .Setup(repo => repo.GetByEmailAsync(inviteeEmail))
            .ReturnsAsync(invitee);

        _mockUserRepository
            .Setup(repo => repo.GetByIdAsync(memberId))
            .ReturnsAsync(member);

        _mockInvitationRepository
            .Setup(repo => repo.GetInvitationsByBoardIdAsync(boardId))
            .ReturnsAsync(new List<BoardInvitation>());

        _mockInvitationRepository
            .Setup(repo => repo.CreateAsync(It.IsAny<BoardInvitation>()))
            .ReturnsAsync((BoardInvitation invitation) => invitation);

        var dto = new InviteUserToBoardDto { Email = inviteeEmail, Role = "Member" };
        var command = new InviteUserToBoardCommand(boardId, dto, memberId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.InvitedById.Should().Be(memberId);
        _mockInvitationRepository.Verify(repo => repo.CreateAsync(It.IsAny<BoardInvitation>()), Times.Once);
    }
}
