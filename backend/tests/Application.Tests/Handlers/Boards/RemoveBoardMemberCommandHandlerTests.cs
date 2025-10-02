using Application.Commands.Boards;
using Domain.Entities;
using Domain.Interfaces;
using Moq;
using Xunit;
using BoardsHandler = Application.Handlers.Boards;

namespace Application.Tests.Handlers.Boards;

public class RemoveBoardMemberCommandHandlerTests
{
    private readonly Mock<BoardRepository> _mockBoardRepository;
    private readonly BoardsHandler.RemoveBoardMemberCommandHandler _handler;

    public RemoveBoardMemberCommandHandlerTests()
    {
        _mockBoardRepository = new Mock<BoardRepository>();
        _handler = new BoardsHandler.RemoveBoardMemberCommandHandler(_mockBoardRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldRemoveMember_WhenUserIsOwner()
    {
        // Arrange
        var ownerId = "owner123";
        var memberId = "member456";
        var boardId = "board123";

        var board = new Board
        {
            Id = boardId,
            Title = "Test Board",
            OwnerId = ownerId,
            MemberIds = new List<string> { ownerId, memberId, "member2" }
        };

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        _mockBoardRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<Board>()))
            .ReturnsAsync((Board b) => b);

        var command = new RemoveBoardMemberCommand(boardId, memberId, ownerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.DoesNotContain(memberId, board.MemberIds);
        _mockBoardRepository.Verify(repo => repo.UpdateAsync(It.Is<Board>(b =>
            !b.MemberIds.Contains(memberId)
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldAllowMemberToRemoveThemselves()
    {
        // Arrange
        var ownerId = "owner123";
        var memberId = "member456";
        var boardId = "board123";

        var board = new Board
        {
            Id = boardId,
            Title = "Test Board",
            OwnerId = ownerId,
            MemberIds = new List<string> { ownerId, memberId }
        };

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        _mockBoardRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<Board>()))
            .ReturnsAsync((Board b) => b);

        var command = new RemoveBoardMemberCommand(boardId, memberId, memberId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.DoesNotContain(memberId, board.MemberIds);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenTryingToRemoveOwner()
    {
        // Arrange
        var ownerId = "owner123";
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

        var command = new RemoveBoardMemberCommand(boardId, ownerId, ownerId);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _handler.Handle(command, CancellationToken.None));

        _mockBoardRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Board>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedException_WhenUserHasNoPermission()
    {
        // Arrange
        var ownerId = "owner123";
        var memberId = "member456";
        var unauthorizedUserId = "unauthorized789";
        var boardId = "board123";

        var board = new Board
        {
            Id = boardId,
            Title = "Test Board",
            OwnerId = ownerId,
            MemberIds = new List<string> { ownerId, memberId, unauthorizedUserId }
        };

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        var command = new RemoveBoardMemberCommand(boardId, memberId, unauthorizedUserId);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _handler.Handle(command, CancellationToken.None));

        _mockBoardRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Board>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenBoardNotFound()
    {
        // Arrange
        var boardId = "nonexistent";
        var memberId = "member456";
        var userId = "user123";

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync((Board?)null);

        var command = new RemoveBoardMemberCommand(boardId, memberId, userId);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenUserNotMemberOfBoard()
    {
        // Arrange
        var ownerId = "owner123";
        var nonMemberId = "nonmember456";
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

        var command = new RemoveBoardMemberCommand(boardId, nonMemberId, ownerId);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _handler.Handle(command, CancellationToken.None));

        _mockBoardRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Board>()), Times.Never);
    }
}
