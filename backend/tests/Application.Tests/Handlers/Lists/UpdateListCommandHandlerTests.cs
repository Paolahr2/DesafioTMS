using Application.Commands.Lists;
using Application.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;
using ListsHandler = Application.Handlers.Lists;

namespace Application.Tests.Handlers.Lists;

public class UpdateListCommandHandlerTests
{
    private readonly Mock<ListRepository> _mockListRepository;
    private readonly Mock<BoardRepository> _mockBoardRepository;
    private readonly ListsHandler.UpdateListCommandHandler _handler;

    public UpdateListCommandHandlerTests()
    {
        _mockListRepository = new Mock<ListRepository>();
        _mockBoardRepository = new Mock<BoardRepository>();
        _handler = new ListsHandler.UpdateListCommandHandler(
            _mockListRepository.Object,
            _mockBoardRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateList_WhenUserHasAccess()
    {
        // Arrange
        var userId = "user123";
        var boardId = "board123";
        var listId = "list123";

        var existingList = new List
        {
            Id = listId,
            Title = "Old Title",
            BoardId = boardId,
            Order = 1,
            Items = new List<ListItem>
            {
                new() { Id = "item1", Text = "Old Item", Completed = false }
            },
            Notes = "Old notes",
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow.AddDays(-5)
        };

        var updateDto = new UpdateListDto
        {
            Title = "Updated Title",
            Order = 2,
            Items = new List<ListItemDto>
            {
                new() { Id = "item1", Text = "Updated Item", Completed = true, Notes = "Updated notes" },
                new() { Text = "New Item", Completed = false }
            }
        };

        _mockListRepository
            .Setup(repo => repo.GetByIdAsync(listId))
            .ReturnsAsync(existingList);

        _mockBoardRepository
            .Setup(repo => repo.UserHasAccessAsync(boardId, userId))
            .ReturnsAsync(true);

        _mockListRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<List>()))
            .ReturnsAsync((List list) => list);

        var command = new UpdateListCommand(listId, updateDto, userId);

        var originalUpdatedAt = existingList.UpdatedAt;

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(listId);
        result.Title.Should().Be("Updated Title");
        result.Order.Should().Be(2);
        result.BoardId.Should().Be(boardId);
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(i => i.Text == "Updated Item" && i.Completed);
        result.Items.Should().Contain(i => i.Text == "New Item" && !i.Completed);
        result.CreatedAt.Should().Be(existingList.CreatedAt);
        result.UpdatedAt.Should().BeAfter(originalUpdatedAt);

        _mockListRepository.Verify(repo => repo.UpdateAsync(It.Is<List>(l =>
            l.Title == "Updated Title" &&
            l.Order == 2 &&
            l.Items.Count == 2
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateOnlyProvidedFields()
    {
        // Arrange
        var userId = "user123";
        var boardId = "board123";
        var listId = "list123";

        var existingList = new List
        {
            Id = listId,
            Title = "Original Title",
            BoardId = boardId,
            Order = 1,
            Items = new List<ListItem>(),
            Notes = "Original notes",
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow.AddDays(-5)
        };

        var updateDto = new UpdateListDto
        {
            Title = "Updated Title",
            Order = null,
            Items = null
            // Order and Items not provided, should remain unchanged
        };

        _mockListRepository
            .Setup(repo => repo.GetByIdAsync(listId))
            .ReturnsAsync(existingList);

        _mockBoardRepository
            .Setup(repo => repo.UserHasAccessAsync(boardId, userId))
            .ReturnsAsync(true);

        _mockListRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<List>()))
            .ReturnsAsync((List list) => list);

        var command = new UpdateListCommand(listId, updateDto, userId);

        var originalUpdatedAt = existingList.UpdatedAt;

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Title.Should().Be("Updated Title");
        result.Order.Should().Be(1); // Unchanged
        result.Items.Should().BeEmpty(); // Unchanged
        result.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public async Task Handle_ShouldUpdateTimestamp_WhenListIsUpdated()
    {
        // Arrange
        var userId = "user123";
        var boardId = "board123";
        var listId = "list123";

        var existingList = new List
        {
            Id = listId,
            Title = "Test List",
            BoardId = boardId,
            Order = 0,
            Items = new List<ListItem>(),
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-5)
        };

        var updateDto = new UpdateListDto
        {
            Title = "Updated Title"
        };

        _mockListRepository
            .Setup(repo => repo.GetByIdAsync(listId))
            .ReturnsAsync(existingList);

        _mockBoardRepository
            .Setup(repo => repo.UserHasAccessAsync(boardId, userId))
            .ReturnsAsync(true);

        _mockListRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<List>()))
            .ReturnsAsync((List list) => list);

        var command = new UpdateListCommand(listId, updateDto, userId);
        var beforeUpdate = DateTime.UtcNow;

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.UpdatedAt.Should().BeAfter(beforeUpdate.AddSeconds(-1));
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.CreatedAt.Should().Be(existingList.CreatedAt); // Unchanged
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenListNotFound()
    {
        // Arrange
        var userId = "user123";
        var listId = "nonexistent";

        var updateDto = new UpdateListDto
        {
            Title = "Updated Title"
        };

        _mockListRepository
            .Setup(repo => repo.GetByIdAsync(listId))
            .ReturnsAsync((List?)null);

        var command = new UpdateListCommand(listId, updateDto, userId);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _handler.Handle(command, CancellationToken.None));

        _mockBoardRepository.Verify(repo => repo.UserHasAccessAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockListRepository.Verify(repo => repo.UpdateAsync(It.IsAny<List>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedException_WhenUserHasNoAccess()
    {
        // Arrange
        var userId = "user123";
        var boardId = "board123";
        var listId = "list123";

        var existingList = new List
        {
            Id = listId,
            Title = "Test List",
            BoardId = boardId,
            Order = 0,
            Items = new List<ListItem>()
        };

        var updateDto = new UpdateListDto
        {
            Title = "Updated Title"
        };

        _mockListRepository
            .Setup(repo => repo.GetByIdAsync(listId))
            .ReturnsAsync(existingList);

        _mockBoardRepository
            .Setup(repo => repo.UserHasAccessAsync(boardId, userId))
            .ReturnsAsync(false);

        var command = new UpdateListCommand(listId, updateDto, userId);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _handler.Handle(command, CancellationToken.None));

        _mockListRepository.Verify(repo => repo.UpdateAsync(It.IsAny<List>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldGenerateIdsForNewItems_WhenItemsHaveNoId()
    {
        // Arrange
        var userId = "user123";
        var boardId = "board123";
        var listId = "list123";

        var existingList = new List
        {
            Id = listId,
            Title = "Test List",
            BoardId = boardId,
            Order = 0,
            Items = new List<ListItem>()
        };

        var updateDto = new UpdateListDto
        {
            Title = "Updated Title",
            Items = new List<ListItemDto>
            {
                new() { Text = "Item without ID", Completed = false },
                new() { Id = "existing-id", Text = "Item with ID", Completed = true }
            }
        };

        _mockListRepository
            .Setup(repo => repo.GetByIdAsync(listId))
            .ReturnsAsync(existingList);

        _mockBoardRepository
            .Setup(repo => repo.UserHasAccessAsync(boardId, userId))
            .ReturnsAsync(true);

        _mockListRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<List>()))
            .ReturnsAsync((List list) => list);

        var command = new UpdateListCommand(listId, updateDto, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(i => i.Id == "existing-id");
        result.Items.Should().Contain(i => i.Id != "existing-id" && !string.IsNullOrEmpty(i.Id));
    }
}
