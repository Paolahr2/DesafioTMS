using Application.DTOs;
using Application.Queries.Boards;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;
using BoardsHandler = Application.Handlers.Boards;

namespace Application.Tests.Handlers.Boards;

public class GetPendingInvitationsQueryHandlerTests
{
    private readonly Mock<BoardInvitationRepository> _mockInvitationRepository;
    private readonly Mock<BoardRepository> _mockBoardRepository;
    private readonly Mock<UserRepository> _mockUserRepository;
    private readonly BoardsHandler.GetPendingInvitationsQueryHandler _handler;

    public GetPendingInvitationsQueryHandlerTests()
    {
        _mockInvitationRepository = new Mock<BoardInvitationRepository>();
        _mockBoardRepository = new Mock<BoardRepository>();
        _mockUserRepository = new Mock<UserRepository>();
        _handler = new BoardsHandler.GetPendingInvitationsQueryHandler(
            _mockInvitationRepository.Object,
            _mockBoardRepository.Object,
            _mockUserRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPendingInvitations_WhenUserHasInvitations()
    {
        // Arrange
        var userId = "user123";
        var inviterId = "inviter456";
        var boardId = "board789";

        var invitation = new BoardInvitation
        {
            Id = "invitation1",
            BoardId = boardId,
            InviterId = inviterId,
            InviteeId = userId,
            Role = "Member",
            Status = InvitationStatus.Pending,
            Message = "Please join our board",
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            ExpiresAt = DateTime.UtcNow.AddDays(5)
        };

        var board = new Board
        {
            Id = boardId,
            Title = "Project Board",
            OwnerId = inviterId
        };

        var inviter = new Domain.Entities.User
        {
            Id = inviterId,
            Username = "inviteruser",
            Email = "inviter@test.com"
        };

        var invitee = new Domain.Entities.User
        {
            Id = userId,
            Username = "inviteeuser",
            Email = "invitee@test.com"
        };

        _mockInvitationRepository
            .Setup(repo => repo.GetPendingInvitationsByUserIdAsync(userId))
            .ReturnsAsync(new List<BoardInvitation> { invitation });

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        _mockUserRepository
            .Setup(repo => repo.GetByIdAsync(inviterId))
            .ReturnsAsync(inviter);

        _mockUserRepository
            .Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(invitee);

        var query = new GetPendingInvitationsQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        
        var invitationDto = result.First();
        invitationDto.Id.Should().Be("invitation1");
        invitationDto.BoardId.Should().Be(boardId);
        invitationDto.BoardTitle.Should().Be("Project Board");
        invitationDto.InvitedUserId.Should().Be(userId);
        invitationDto.InvitedById.Should().Be(inviterId);
        invitationDto.InvitedByName.Should().Be("inviteruser");
        invitationDto.Role.Should().Be("Member");
        invitationDto.Status.Should().Be("Pending");
        invitationDto.Message.Should().Be("Please join our board");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenUserHasNoInvitations()
    {
        // Arrange
        var userId = "user123";

        _mockInvitationRepository
            .Setup(repo => repo.GetPendingInvitationsByUserIdAsync(userId))
            .ReturnsAsync(new List<BoardInvitation>());

        var query = new GetPendingInvitationsQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldHandleNullBoard_GracefullyWithDefaultTitle()
    {
        // Arrange
        var userId = "user123";
        var inviterId = "inviter456";
        var boardId = "nonexistentboard";

        var invitation = new BoardInvitation
        {
            Id = "invitation1",
            BoardId = boardId,
            InviterId = inviterId,
            InviteeId = userId,
            Role = "Member",
            Status = InvitationStatus.Pending,
            Message = "",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        var inviter = new Domain.Entities.User
        {
            Id = inviterId,
            Username = "inviteruser",
            Email = "inviter@test.com"
        };

        var invitee = new Domain.Entities.User
        {
            Id = userId,
            Username = "inviteeuser",
            Email = "invitee@test.com"
        };

        _mockInvitationRepository
            .Setup(repo => repo.GetPendingInvitationsByUserIdAsync(userId))
            .ReturnsAsync(new List<BoardInvitation> { invitation });

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync((Board?)null);

        _mockUserRepository
            .Setup(repo => repo.GetByIdAsync(inviterId))
            .ReturnsAsync(inviter);

        _mockUserRepository
            .Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(invitee);

        var query = new GetPendingInvitationsQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().BoardTitle.Should().Be("Tablero desconocido");
    }

    [Fact]
    public async Task Handle_ShouldReturnMultipleInvitations_WhenUserHasMany()
    {
        // Arrange
        var userId = "user123";
        
        var invitations = new List<BoardInvitation>
        {
            new() { Id = "inv1", BoardId = "board1", InviterId = "user1", InviteeId = userId, Role = "Member", Status = InvitationStatus.Pending, CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddDays(7) },
            new() { Id = "inv2", BoardId = "board2", InviterId = "user2", InviteeId = userId, Role = "Member", Status = InvitationStatus.Pending, CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddDays(7) },
            new() { Id = "inv3", BoardId = "board3", InviterId = "user3", InviteeId = userId, Role = "Admin", Status = InvitationStatus.Pending, CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddDays(7) }
        };

        _mockInvitationRepository
            .Setup(repo => repo.GetPendingInvitationsByUserIdAsync(userId))
            .ReturnsAsync(invitations);

        // Setup mocks for boards and users
        foreach (var inv in invitations)
        {
            _mockBoardRepository
                .Setup(repo => repo.GetByIdAsync(inv.BoardId))
                .ReturnsAsync(new Board { Id = inv.BoardId, Title = $"Board {inv.BoardId}", OwnerId = inv.InviterId });

            _mockUserRepository
                .Setup(repo => repo.GetByIdAsync(inv.InviterId))
                .ReturnsAsync(new Domain.Entities.User { Id = inv.InviterId, Username = $"user{inv.InviterId}", Email = $"{inv.InviterId}@test.com" });
        }

        _mockUserRepository
            .Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(new Domain.Entities.User { Id = userId, Username = "inviteeuser", Email = "invitee@test.com" });

        var query = new GetPendingInvitationsQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(i => i.Id == "inv1");
        result.Should().Contain(i => i.Id == "inv2");
        result.Should().Contain(i => i.Id == "inv3");
    }
}
