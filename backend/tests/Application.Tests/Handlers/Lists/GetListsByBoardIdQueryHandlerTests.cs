using Application.DTOs;
using Application.Queries.Lists;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;
using ListsHandler = Application.Handlers.Lists;

namespace Application.Tests.Handlers.Lists;

public class GetListsByBoardIdQueryHandlerTests
{
    private readonly Mock<ListRepository> _mockListRepository;
    private readonly Mock<BoardRepository> _mockBoardRepository;
    private readonly ListsHandler.GetListsByBoardIdQueryHandler _handler;

    public GetListsByBoardIdQueryHandlerTests()
    {
        _mockListRepository = new Mock<ListRepository>();
        _mockBoardRepository = new Mock<BoardRepository>();
        _handler = new ListsHandler.GetListsByBoardIdQueryHandler(
            _mockListRepository.Object,
            _mockBoardRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnLists_WhenUserHasAccess()
    {
        // Arrange
        var userId = "user123";
        var boardId = "board123";

        var lists = new List<List>
        {
            new List
            {
                Id = "list1",
                Title = "Shopping List",
                BoardId = boardId,
                Order = 1,
                Items = new List<ListItem>
                {
                    new() { Id = "item1", Text = "Milk", Completed = false },
                    new() { Id = "item2", Text = "Bread", Completed = true }
                },
                Notes = "Weekly groceries",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new List
            {
                Id = "list2",
                Title = "Todo List",
                BoardId = boardId,
                Order = 2,
                Items = new List<ListItem>(),
                Notes = null,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };

        _mockBoardRepository
            .Setup(repo => repo.UserHasAccessAsync(boardId, userId))
            .ReturnsAsync(true);

        _mockListRepository
            .Setup(repo => repo.GetListsByBoardIdAsync(boardId))
            .ReturnsAsync(lists);

        var query = new GetListsByBoardIdQuery(boardId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var shoppingList = result.First(l => l.Id == "list1");
        shoppingList.Title.Should().Be("Shopping List");
        shoppingList.Order.Should().Be(1);
        shoppingList.Items.Should().HaveCount(2);
        shoppingList.Items.Should().Contain(i => i.Text == "Milk" && !i.Completed);
        shoppingList.Items.Should().Contain(i => i.Text == "Bread" && i.Completed);
        shoppingList.Notes.Should().Be("Weekly groceries");

        var todoList = result.First(l => l.Id == "list2");
        todoList.Title.Should().Be("Todo List");
        todoList.Order.Should().Be(2);
        todoList.Items.Should().BeEmpty();
        todoList.Notes.Should().BeNull();

        _mockBoardRepository.Verify(repo => repo.UserHasAccessAsync(boardId, userId), Times.Once);
        _mockListRepository.Verify(repo => repo.GetListsByBoardIdAsync(boardId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoListsExist()
    {
        // Arrange
        var userId = "user123";
        var boardId = "board123";

        _mockBoardRepository
            .Setup(repo => repo.UserHasAccessAsync(boardId, userId))
            .ReturnsAsync(true);

        _mockListRepository
            .Setup(repo => repo.GetListsByBoardIdAsync(boardId))
            .ReturnsAsync(new List<List>());

        var query = new GetListsByBoardIdQuery(boardId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedException_WhenUserHasNoAccess()
    {
        // Arrange
        var userId = "user123";
        var boardId = "board123";

        _mockBoardRepository
            .Setup(repo => repo.UserHasAccessAsync(boardId, userId))
            .ReturnsAsync(false);

        var query = new GetListsByBoardIdQuery(boardId, userId);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _handler.Handle(query, CancellationToken.None));

        _mockListRepository.Verify(repo => repo.GetListsByBoardIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnListsWithEmptyItems_WhenListsHaveNoItems()
    {
        // Arrange
        var userId = "user123";
        var boardId = "board123";

        var lists = new List<List>
        {
            new List
            {
                Id = "list1",
                Title = "Empty List",
                BoardId = boardId,
                Order = 1,
                Items = new List<ListItem>(), // Empty list instead of null
                Notes = "No items yet",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _mockBoardRepository
            .Setup(repo => repo.UserHasAccessAsync(boardId, userId))
            .ReturnsAsync(true);

        _mockListRepository
            .Setup(repo => repo.GetListsByBoardIdAsync(boardId))
            .ReturnsAsync(lists);

        var query = new GetListsByBoardIdQuery(boardId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        var listDto = result.First();
        listDto.Items.Should().NotBeNull();
        listDto.Items.Should().BeEmpty();
        listDto.Notes.Should().Be("No items yet");
    }

    [Fact]
    public async Task Handle_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        var userId = "user123";
        var boardId = "board123";
        var createdAt = DateTime.UtcNow.AddDays(-5);
        var updatedAt = DateTime.UtcNow.AddDays(-1);

        var list = new List
        {
            Id = "list1",
            Title = "Complete List",
            BoardId = boardId,
            Order = 3,
            Items = new List<ListItem>
            {
                new() { Id = "item1", Text = "Test Item", Completed = true, Notes = "Test notes" }
            },
            Notes = "Complete notes",
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        _mockBoardRepository
            .Setup(repo => repo.UserHasAccessAsync(boardId, userId))
            .ReturnsAsync(true);

        _mockListRepository
            .Setup(repo => repo.GetListsByBoardIdAsync(boardId))
            .ReturnsAsync(new List<List> { list });

        var query = new GetListsByBoardIdQuery(boardId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        var listDto = result.First();
        listDto.Id.Should().Be("list1");
        listDto.Title.Should().Be("Complete List");
        listDto.BoardId.Should().Be(boardId);
        listDto.Order.Should().Be(3);
        listDto.Notes.Should().Be("Complete notes");
        listDto.CreatedAt.Should().Be(createdAt);
        listDto.UpdatedAt.Should().Be(updatedAt);

        listDto.Items.Should().HaveCount(1);
        var item = listDto.Items.First();
        item.Id.Should().Be("item1");
        item.Text.Should().Be("Test Item");
        item.Completed.Should().BeTrue();
        item.Notes.Should().Be("Test notes");
    }

    [Fact]
    public async Task Handle_ShouldReturnMultipleLists_WhenBoardHasManyLists()
    {
        // Arrange
        var userId = "user123";
        var boardId = "board123";

        var lists = new List<List>();
        for (int i = 1; i <= 5; i++)
        {
            lists.Add(new List
            {
                Id = $"list{i}",
                Title = $"List {i}",
                BoardId = boardId,
                Order = i,
                Items = new List<ListItem>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        _mockBoardRepository
            .Setup(repo => repo.UserHasAccessAsync(boardId, userId))
            .ReturnsAsync(true);

        _mockListRepository
            .Setup(repo => repo.GetListsByBoardIdAsync(boardId))
            .ReturnsAsync(lists);

        var query = new GetListsByBoardIdQuery(boardId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(5);
        result.Should().OnlyContain(l => l.BoardId == boardId);
        result.Select(l => l.Order).Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5 });
    }
}
