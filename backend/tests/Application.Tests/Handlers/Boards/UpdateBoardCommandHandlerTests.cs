using Application.Commands.Boards;
using Application.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;
using BoardsHandler = Application.Handlers.Boards;

namespace Application.Tests.Handlers.Boards;

public class UpdateBoardCommandHandlerTests
{
    private readonly Mock<BoardRepository> _mockBoardRepository;
    private readonly BoardsHandler.UpdateBoardCommandHandler _handler;

    public UpdateBoardCommandHandlerTests()
    {
        _mockBoardRepository = new Mock<BoardRepository>();
        _handler = new BoardsHandler.UpdateBoardCommandHandler(_mockBoardRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateBoard_WhenUserIsOwner()
    {
        // Arrange
        var userId = "owner123";
        var boardId = "board123";

        var existingBoard = new Board
        {
            Id = boardId,
            Title = "Old Title",
            Description = "Old Description",
            OwnerId = userId,
            MemberIds = new List<string> { userId },
            Color = "#FF5733",
            IsArchived = false,
            IsPublic = false,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow.AddDays(-5)
        };

        var updateDto = new UpdateBoardDto
        {
            Title = "New Title",
            Description = "New Description",
            Color = "#00FF00",
            IsPublic = true
        };

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(existingBoard);

        _mockBoardRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<Board>()))
            .ReturnsAsync((Board board) => board);

        var command = new UpdateBoardCommand(boardId, updateDto, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(boardId);
        result.Title.Should().Be("New Title");
        result.Description.Should().Be("New Description");
        result.Color.Should().Be("#00FF00");
        result.IsPublic.Should().BeTrue();
        result.OwnerId.Should().Be(userId);

        _mockBoardRepository.Verify(repo => repo.GetByIdAsync(boardId), Times.Once);
        _mockBoardRepository.Verify(repo => repo.UpdateAsync(It.Is<Board>(b => 
            b.Title == "New Title" && 
            b.Description == "New Description" &&
            b.Color == "#00FF00" &&
            b.IsPublic == true
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateOnlyProvidedFields_WhenPartialUpdate()
    {
        // Arrange
        var userId = "owner123";
        var boardId = "board123";

        var existingBoard = new Board
        {
            Id = boardId,
            Title = "Original Title",
            Description = "Original Description",
            OwnerId = userId,
            MemberIds = new List<string> { userId },
            Color = "#FF5733",
            IsArchived = false,
            IsPublic = false,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow.AddDays(-5)
        };

        var updateDto = new UpdateBoardDto
        {
            Title = "Updated Title",
            // Otros campos quedan como null, no se actualizan
        };

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(existingBoard);

        _mockBoardRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<Board>()))
            .ReturnsAsync((Board board) => board);

        var command = new UpdateBoardCommand(boardId, updateDto, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Title.Should().Be("Updated Title");
        result.Description.Should().Be("Original Description"); // No cambió
        result.Color.Should().Be("#FF5733"); // No cambió
        result.IsPublic.Should().BeFalse(); // No cambió

        _mockBoardRepository.Verify(repo => repo.UpdateAsync(It.Is<Board>(b =>
            b.Title == "Updated Title" &&
            b.Description == "Original Description" &&
            b.Color == "#FF5733" &&
            b.IsPublic == false
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateTimestamp_WhenBoardIsUpdated()
    {
        // Arrange
        var userId = "owner123";
        var boardId = "board123";
        var oldUpdateTime = DateTime.UtcNow.AddDays(-5);

        var existingBoard = new Board
        {
            Id = boardId,
            Title = "Test Board",
            Description = "Test Description",
            OwnerId = userId,
            MemberIds = new List<string> { userId },
            Color = "#FF5733",
            IsArchived = false,
            IsPublic = false,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = oldUpdateTime
        };

        var updateDto = new UpdateBoardDto
        {
            Title = "Updated Title"
        };

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(existingBoard);

        _mockBoardRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<Board>()))
            .ReturnsAsync((Board board) => board);

        var command = new UpdateBoardCommand(boardId, updateDto, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.UpdatedAt.Should().BeAfter(oldUpdateTime);
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _mockBoardRepository.Verify(repo => repo.UpdateAsync(It.Is<Board>(b =>
            b.UpdatedAt > oldUpdateTime
        )), Times.Once);
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

        var updateDto = new UpdateBoardDto
        {
            Title = "Unauthorized Update"
        };

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        var command = new UpdateBoardCommand(boardId, updateDto, unauthorizedUserId);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _handler.Handle(command, CancellationToken.None));

        _mockBoardRepository.Verify(repo => repo.GetByIdAsync(boardId), Times.Once);
        _mockBoardRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Board>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenBoardNotFound()
    {
        // Arrange
        var userId = "user123";
        var boardId = "nonexistent";

        var updateDto = new UpdateBoardDto
        {
            Title = "New Title"
        };

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync((Board?)null);

        var command = new UpdateBoardCommand(boardId, updateDto, userId);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _handler.Handle(command, CancellationToken.None));

        _mockBoardRepository.Verify(repo => repo.GetByIdAsync(boardId), Times.Once);
        _mockBoardRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Board>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFullBoardDto_WithAllProperties()
    {
        // Arrange
        var userId = "owner123";
        var boardId = "board123";
        var createdAt = DateTime.UtcNow.AddDays(-10);

        var existingBoard = new Board
        {
            Id = boardId,
            Title = "Test Board",
            Description = "Test Description",
            OwnerId = userId,
            MemberIds = new List<string> { userId, "member2" },
            Color = "#FF5733",
            IsArchived = false,
            IsPublic = false,
            Columns = new List<string> { "Column1", "Column2" },
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };

        var updateDto = new UpdateBoardDto
        {
            Title = "Updated Title"
        };

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(existingBoard);

        _mockBoardRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<Board>()))
            .ReturnsAsync((Board board) => board);

        var command = new UpdateBoardCommand(boardId, updateDto, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Id.Should().Be(boardId);
        result.Title.Should().Be("Updated Title");
        result.Description.Should().Be("Test Description");
        result.OwnerId.Should().Be(userId);
        result.MemberIds.Should().HaveCount(2);
        result.MemberIds.Should().Contain(new[] { userId, "member2" });
        result.Color.Should().Be("#FF5733");
        result.IsArchived.Should().BeFalse();
        result.IsPublic.Should().BeFalse();
        result.Columns.Should().HaveCount(2);
        result.Columns.Should().Contain(new[] { "Column1", "Column2" });
        result.CreatedAt.Should().Be(createdAt);
        result.UpdatedAt.Should().BeAfter(createdAt);
    }
}
