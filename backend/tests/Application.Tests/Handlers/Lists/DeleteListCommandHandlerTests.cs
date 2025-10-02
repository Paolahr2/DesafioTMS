using Application.Commands.Lists;
using Domain.Entities;
using Domain.Interfaces;
using Moq;
using Xunit;
using ListsHandler = Application.Handlers.Lists;

namespace Application.Tests.Handlers.Lists;

public class DeleteListCommandHandlerTests
{
    private readonly Mock<ListRepository> _mockListRepository;
    private readonly Mock<BoardRepository> _mockBoardRepository;
    private readonly ListsHandler.DeleteListCommandHandler _handler;

    public DeleteListCommandHandlerTests()
    {
        _mockListRepository = new Mock<ListRepository>();
        _mockBoardRepository = new Mock<BoardRepository>();
        _handler = new ListsHandler.DeleteListCommandHandler(
            _mockListRepository.Object,
            _mockBoardRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteList_WhenUserHasAccess()
    {
        // Arrange
        var userId = "user123";
        var boardId = "board123";
        var listId = "list123";

        var list = new List
        {
            Id = listId,
            Title = "Test List",
            BoardId = boardId,
            Order = 0,
            Items = new List<ListItem>()
        };

        _mockListRepository
            .Setup(repo => repo.GetByIdAsync(listId))
            .ReturnsAsync(list);

        _mockBoardRepository
            .Setup(repo => repo.UserHasAccessAsync(boardId, userId))
            .ReturnsAsync(true);

        _mockListRepository
            .Setup(repo => repo.DeleteAsync(listId))
            .ReturnsAsync(true);

        var command = new DeleteListCommand(listId, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        _mockListRepository.Verify(repo => repo.DeleteAsync(listId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenListNotFound()
    {
        // Arrange
        var userId = "user123";
        var listId = "nonexistent";

        _mockListRepository
            .Setup(repo => repo.GetByIdAsync(listId))
            .ReturnsAsync((List?)null);

        var command = new DeleteListCommand(listId, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
        _mockListRepository.Verify(repo => repo.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedException_WhenUserHasNoAccess()
    {
        // Arrange
        var userId = "user123";
        var boardId = "board123";
        var listId = "list123";

        var list = new List
        {
            Id = listId,
            Title = "Test List",
            BoardId = boardId,
            Order = 0,
            Items = new List<ListItem>()
        };

        _mockListRepository
            .Setup(repo => repo.GetByIdAsync(listId))
            .ReturnsAsync(list);

        _mockBoardRepository
            .Setup(repo => repo.UserHasAccessAsync(boardId, userId))
            .ReturnsAsync(false);

        var command = new DeleteListCommand(listId, userId);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _handler.Handle(command, CancellationToken.None));

        _mockListRepository.Verify(repo => repo.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldDeleteListWithItems_WithoutAffectingTasks()
    {
        // Arrange
        var userId = "user123";
        var boardId = "board123";
        var listId = "list123";

        var list = new List
        {
            Id = listId,
            Title = "Checklist with Items",
            BoardId = boardId,
            Order = 0,
            Items = new List<ListItem>
            {
                new() { Id = "item1", Text = "Item 1", Completed = false },
                new() { Id = "item2", Text = "Item 2", Completed = true }
            }
        };

        _mockListRepository
            .Setup(repo => repo.GetByIdAsync(listId))
            .ReturnsAsync(list);

        _mockBoardRepository
            .Setup(repo => repo.UserHasAccessAsync(boardId, userId))
            .ReturnsAsync(true);

        _mockListRepository
            .Setup(repo => repo.DeleteAsync(listId))
            .ReturnsAsync(true);

        var command = new DeleteListCommand(listId, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        _mockListRepository.Verify(repo => repo.DeleteAsync(listId), Times.Once);
        // Note: Las listas de checklist NO contienen tareas, por lo que eliminar una lista no afecta tareas
    }

    [Fact]
    public async Task Handle_ShouldVerifyAccessBeforeDeleting()
    {
        // Arrange
        var userId = "user123";
        var boardId = "board123";
        var listId = "list123";

        var list = new List
        {
            Id = listId,
            Title = "Test List",
            BoardId = boardId,
            Order = 0,
            Items = new List<ListItem>()
        };

        _mockListRepository
            .Setup(repo => repo.GetByIdAsync(listId))
            .ReturnsAsync(list);

        _mockBoardRepository
            .Setup(repo => repo.UserHasAccessAsync(boardId, userId))
            .ReturnsAsync(true);

        _mockListRepository
            .Setup(repo => repo.DeleteAsync(listId))
            .ReturnsAsync(true);

        var command = new DeleteListCommand(listId, userId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockBoardRepository.Verify(repo => repo.UserHasAccessAsync(boardId, userId), Times.Once);
        _mockListRepository.Verify(repo => repo.GetByIdAsync(listId), Times.Once);
    }
}
