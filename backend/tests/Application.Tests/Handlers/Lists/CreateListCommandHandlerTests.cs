using Application.Commands.Lists;
using Application.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;
using ListsHandler = Application.Handlers.Lists;

namespace Application.Tests.Handlers.Lists;

public class CreateListCommandHandlerTests
{
    private readonly Mock<ListRepository> _mockListRepository;
    private readonly Mock<BoardRepository> _mockBoardRepository;
    private readonly ListsHandler.CreateListCommandHandler _handler;

    public CreateListCommandHandlerTests()
    {
        _mockListRepository = new Mock<ListRepository>();
        _mockBoardRepository = new Mock<BoardRepository>();
        _handler = new ListsHandler.CreateListCommandHandler(
            _mockListRepository.Object,
            _mockBoardRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateList_WhenUserHasAccess()
    {
        // Arrange
        var userId = "user123";
        var boardId = "board123";

        var createDto = new CreateListDto
        {
            Title = "Shopping List",
            BoardId = boardId,
            Order = 1,
            Notes = "Weekly groceries"
        };

        _mockBoardRepository
            .Setup(repo => repo.UserHasAccessAsync(boardId, userId))
            .ReturnsAsync(true);

        _mockListRepository
            .Setup(repo => repo.CreateAsync(It.IsAny<List>()))
            .ReturnsAsync((List list) => { list.Id = "generated-id"; return list; });

        var command = new CreateListCommand(createDto, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Shopping List");
        result.BoardId.Should().Be(boardId);
        result.Order.Should().Be(1);
        result.Notes.Should().Be("Weekly groceries");
        result.Id.Should().NotBeNullOrEmpty();

        _mockListRepository.Verify(repo => repo.CreateAsync(It.Is<List>(l =>
            l.Title == "Shopping List" &&
            l.BoardId == boardId &&
            l.Order == 1 &&
            l.Notes == "Weekly groceries"
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateListWithItems_WhenItemsProvided()
    {
        // Arrange
        var userId = "user123";
        var boardId = "board123";

        var createDto = new CreateListDto
        {
            Title = "Todo List",
            BoardId = boardId,
            Order = 0,
            Items = new List<ListItemDto>
            {
                new() { Text = "Buy milk", Completed = false },
                new() { Text = "Buy bread", Completed = true, Notes = "Whole wheat" }
            }
        };

        _mockBoardRepository
            .Setup(repo => repo.UserHasAccessAsync(boardId, userId))
            .ReturnsAsync(true);

        _mockListRepository
            .Setup(repo => repo.CreateAsync(It.IsAny<List>()))
            .ReturnsAsync((List list) => { list.Id = "generated-id-2"; return list; });

        var command = new CreateListCommand(createDto, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(i => i.Text == "Buy milk" && !i.Completed);
        result.Items.Should().Contain(i => i.Text == "Buy bread" && i.Completed && i.Notes == "Whole wheat");

        _mockListRepository.Verify(repo => repo.CreateAsync(It.Is<List>(l =>
            l.Items.Count == 2
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateListWithEmptyItems_WhenNoItemsProvided()
    {
        // Arrange
        var userId = "user123";
        var boardId = "board123";

        var createDto = new CreateListDto
        {
            Title = "Empty List",
            BoardId = boardId,
            Order = 0
        };

        _mockBoardRepository
            .Setup(repo => repo.UserHasAccessAsync(boardId, userId))
            .ReturnsAsync(true);

        _mockListRepository
            .Setup(repo => repo.CreateAsync(It.IsAny<List>()))
            .ReturnsAsync((List list) => { list.Id = "generated-id-3"; return list; });

        var command = new CreateListCommand(createDto, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Items.Should().NotBeNull();
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldSetTimestamps_WhenCreatingList()
    {
        // Arrange
        var userId = "user123";
        var boardId = "board123";

        var createDto = new CreateListDto
        {
            Title = "Test List",
            BoardId = boardId,
            Order = 0
        };

        _mockBoardRepository
            .Setup(repo => repo.UserHasAccessAsync(boardId, userId))
            .ReturnsAsync(true);

        _mockListRepository
            .Setup(repo => repo.CreateAsync(It.IsAny<List>()))
            .ReturnsAsync((List list) => { list.Id = "generated-id-4"; return list; });

        var command = new CreateListCommand(createDto, userId);
        var beforeCreate = DateTime.UtcNow;

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.CreatedAt.Should().BeAfter(beforeCreate.AddSeconds(-1));
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedException_WhenUserHasNoAccess()
    {
        // Arrange
        var userId = "user123";
        var boardId = "board123";

        var createDto = new CreateListDto
        {
            Title = "Test List",
            BoardId = boardId,
            Order = 0
        };

        _mockBoardRepository
            .Setup(repo => repo.UserHasAccessAsync(boardId, userId))
            .ReturnsAsync(false);

        var command = new CreateListCommand(createDto, userId);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _handler.Handle(command, CancellationToken.None));

        _mockListRepository.Verify(repo => repo.CreateAsync(It.IsAny<List>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldGenerateIdsForItems_WhenItemsHaveNoId()
    {
        // Arrange
        var userId = "user123";
        var boardId = "board123";

        var createDto = new CreateListDto
        {
            Title = "List with items",
            BoardId = boardId,
            Order = 0,
            Items = new List<ListItemDto>
            {
                new() { Text = "Item 1" },
                new() { Text = "Item 2" }
            }
        };

        _mockBoardRepository
            .Setup(repo => repo.UserHasAccessAsync(boardId, userId))
            .ReturnsAsync(true);

        _mockListRepository
            .Setup(repo => repo.CreateAsync(It.IsAny<List>()))
            .ReturnsAsync((List list) => { list.Id = "generated-id-5"; return list; });

        var command = new CreateListCommand(createDto, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(i => !string.IsNullOrEmpty(i.Id));
        result.Items.Select(i => i.Id).Should().OnlyHaveUniqueItems();
    }
}
