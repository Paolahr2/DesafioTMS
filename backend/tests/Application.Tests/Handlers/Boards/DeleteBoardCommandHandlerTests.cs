using Application.Commands.Boards;
using Domain.Entities;
using Domain.Interfaces;
using Moq;
using Xunit;
using BoardsHandler = Application.Handlers.Boards;

namespace Application.Tests.Handlers.Boards;

public class DeleteBoardCommandHandlerTests
{
    private readonly Mock<BoardRepository> _mockBoardRepository;
    private readonly BoardsHandler.DeleteBoardCommandHandler _handler;

    public DeleteBoardCommandHandlerTests()
    {
        _mockBoardRepository = new Mock<BoardRepository>();
        _handler = new BoardsHandler.DeleteBoardCommandHandler(_mockBoardRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteBoard_WhenUserIsOwner()
    {
        // Arrange
        var userId = "owner123";
        var boardId = "board123";

        var board = new Board
        {
            Id = boardId,
            Title = "Test Board",
            Description = "Test Description",
            OwnerId = userId,
            MemberIds = new List<string> { userId },
            Color = "#FF5733",
            IsArchived = false,
            IsPublic = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        _mockBoardRepository
            .Setup(repo => repo.DeleteAsync(boardId))
            .ReturnsAsync(true);

        var command = new DeleteBoardCommand(boardId, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        _mockBoardRepository.Verify(repo => repo.GetByIdAsync(boardId), Times.Once);
        _mockBoardRepository.Verify(repo => repo.DeleteAsync(boardId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserIsNotOwner()
    {
        // Arrange
        var ownerId = "owner123";
        var unauthorizedUserId = "user456";
        var boardId = "board123";

        var board = new Board
        {
            Id = boardId,
            Title = "Test Board",
            Description = "Test Description",
            OwnerId = ownerId,
            MemberIds = new List<string> { ownerId, unauthorizedUserId },
            Color = "#FF5733",
            IsArchived = false,
            IsPublic = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        var command = new DeleteBoardCommand(boardId, unauthorizedUserId);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _handler.Handle(command, CancellationToken.None));

        _mockBoardRepository.Verify(repo => repo.GetByIdAsync(boardId), Times.Once);
        _mockBoardRepository.Verify(repo => repo.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenBoardNotFound()
    {
        // Arrange
        var userId = "user123";
        var boardId = "nonexistent";

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync((Board?)null);

        var command = new DeleteBoardCommand(boardId, userId);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _handler.Handle(command, CancellationToken.None));

        _mockBoardRepository.Verify(repo => repo.GetByIdAsync(boardId), Times.Once);
        _mockBoardRepository.Verify(repo => repo.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldOnlyAllowOwnerToDelete_EvenIfUserIsMember()
    {
        // Arrange
        var ownerId = "owner123";
        var memberId = "member456";
        var boardId = "board123";

        var board = new Board
        {
            Id = boardId,
            Title = "Test Board",
            Description = "Test Description",
            OwnerId = ownerId,
            MemberIds = new List<string> { ownerId, memberId },
            Color = "#FF5733",
            IsArchived = false,
            IsPublic = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        var command = new DeleteBoardCommand(boardId, memberId);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _handler.Handle(command, CancellationToken.None));

        _mockBoardRepository.Verify(repo => repo.DeleteAsync(It.IsAny<string>()), Times.Never);
    }
}
