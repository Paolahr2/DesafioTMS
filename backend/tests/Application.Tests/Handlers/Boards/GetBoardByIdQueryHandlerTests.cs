using Application.DTOs;
using Application.Queries.Boards;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;
using BoardsHandler = Application.Handlers.Boards;

namespace Application.Tests.Handlers.Boards;

public class GetBoardByIdQueryHandlerTests
{
    private readonly Mock<BoardRepository> _mockBoardRepository;
    private readonly BoardsHandler.GetBoardByIdQueryHandler _handler;

    public GetBoardByIdQueryHandlerTests()
    {
        _mockBoardRepository = new Mock<BoardRepository>();
        _handler = new BoardsHandler.GetBoardByIdQueryHandler(_mockBoardRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnBoard_WhenUserIsOwner()
    {
        // Arrange
        var userId = "user123";
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

        var query = new GetBoardByIdQuery(boardId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(boardId);
        result.Title.Should().Be(board.Title);
        result.Description.Should().Be(board.Description);
        result.OwnerId.Should().Be(userId);
        result.Color.Should().Be(board.Color);
        result.IsArchived.Should().BeFalse();
        result.IsPublic.Should().BeFalse();

        _mockBoardRepository.Verify(repo => repo.GetByIdAsync(boardId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnBoard_WhenUserIsMember()
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

        var query = new GetBoardByIdQuery(boardId, memberId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(boardId);
        result.OwnerId.Should().Be(ownerId);
        result.MemberIds.Should().Contain(memberId);

        _mockBoardRepository.Verify(repo => repo.GetByIdAsync(boardId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentException_WhenBoardNotFound()
    {
        // Arrange
        var userId = "user123";
        var boardId = "nonexistent";

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync((Board?)null);

        var query = new GetBoardByIdQuery(boardId, userId);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _handler.Handle(query, CancellationToken.None));

        _mockBoardRepository.Verify(repo => repo.GetByIdAsync(boardId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserNotOwnerOrMember()
    {
        // Arrange
        var ownerId = "owner123";
        var unauthorizedUserId = "unauthorized456";
        var boardId = "board123";

        var board = new Board
        {
            Id = boardId,
            Title = "Test Board",
            Description = "Test Description",
            OwnerId = ownerId,
            MemberIds = new List<string> { ownerId },
            Color = "#FF5733",
            IsArchived = false,
            IsPublic = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        var query = new GetBoardByIdQuery(boardId, unauthorizedUserId);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _handler.Handle(query, CancellationToken.None));

        _mockBoardRepository.Verify(repo => repo.GetByIdAsync(boardId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnBoardDto_WithAllProperties()
    {
        // Arrange
        var userId = "user123";
        var boardId = "board123";
        var createdAt = DateTime.UtcNow.AddDays(-5);
        var updatedAt = DateTime.UtcNow.AddDays(-1);

        var board = new Board
        {
            Id = boardId,
            Title = "Complete Board",
            Description = "Full Description",
            OwnerId = userId,
            MemberIds = new List<string> { userId, "member2", "member3" },
            Color = "#123456",
            IsArchived = false,
            IsPublic = true,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        var query = new GetBoardByIdQuery(boardId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Id.Should().Be(boardId);
        result.Title.Should().Be(board.Title);
        result.Description.Should().Be(board.Description);
        result.OwnerId.Should().Be(userId);
        result.MemberIds.Should().HaveCount(3);
        result.MemberIds.Should().Contain(new[] { userId, "member2", "member3" });
        result.Color.Should().Be(board.Color);
        result.IsArchived.Should().BeFalse();
        result.IsPublic.Should().BeTrue();
        result.CreatedAt.Should().Be(createdAt);
        result.UpdatedAt.Should().Be(updatedAt);
    }
}
