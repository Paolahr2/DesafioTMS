using Application.Queries.Tasks;
using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using TaskStatus = Domain.Enums.TaskStatus;

namespace Application.Tests.Handlers.Tasks;

public class TaskQueryHandlerTests
{
    private readonly Mock<TaskRepository> _mockTaskRepository;
    private readonly Mock<BoardRepository> _mockBoardRepository;
    private readonly Mock<ILogger<TaskQueryHandler>> _mockLogger;
    private readonly TaskQueryHandler _handler;

    public TaskQueryHandlerTests()
    {
        _mockTaskRepository = new Mock<TaskRepository>();
        _mockBoardRepository = new Mock<BoardRepository>();
        _mockLogger = new Mock<ILogger<TaskQueryHandler>>();
        _handler = new TaskQueryHandler(
            _mockTaskRepository.Object,
            _mockBoardRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_GetTaskById_ShouldReturnTask_WhenUserHasAccess()
    {
        // Arrange
        var userId = "user123";
        var taskId = "task123";
        var boardId = "board123";

        var task = new TaskItem
        {
            Id = taskId,
            Title = "Test Task",
            Description = "Test Description",
            Status = TaskStatus.Todo,
            BoardId = boardId,
            Priority = TaskPriority.High,
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var board = new Board
        {
            Id = boardId,
            OwnerId = userId,
            IsPublic = false,
            MemberIds = new List<string>()
        };

        _mockTaskRepository
            .Setup(repo => repo.GetByIdAsync(taskId))
            .ReturnsAsync(task);

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        var query = new GetTaskByIdQuery(taskId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(taskId);
        result.Title.Should().Be("Test Task");
        result.Description.Should().Be("Test Description");
        result.Status.Should().Be(TaskStatus.Todo);
        result.BoardId.Should().Be(boardId);
        result.Priority.Should().Be(TaskPriority.High);
        result.CreatedById.Should().Be(userId);
    }

    [Fact]
    public async Task Handle_GetTaskById_ShouldReturnNull_WhenTaskDoesNotExist()
    {
        // Arrange
        var userId = "user123";
        var taskId = "non-existent-task";

        _mockTaskRepository
            .Setup(repo => repo.GetByIdAsync(taskId))
            .ReturnsAsync(() => null!);

        var query = new GetTaskByIdQuery(taskId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_GetTaskById_ShouldReturnNull_WhenUserHasNoAccess()
    {
        // Arrange
        var userId = "user123";
        var taskId = "task123";
        var boardId = "board123";

        var task = new TaskItem
        {
            Id = taskId,
            BoardId = boardId,
            CreatedById = "other-user"
        };

        var board = new Board
        {
            Id = boardId,
            OwnerId = "other-owner",
            IsPublic = false,
            MemberIds = new List<string> { "member1", "member2" }
        };

        _mockTaskRepository
            .Setup(repo => repo.GetByIdAsync(taskId))
            .ReturnsAsync(task);

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        var query = new GetTaskByIdQuery(taskId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_GetBoardTasks_ShouldReturnTasks_WhenUserHasAccess()
    {
        // Arrange
        var userId = "user123";
        var boardId = "board123";

        var tasks = new List<TaskItem>
        {
            new TaskItem
            {
                Id = "task1",
                Title = "Task 1",
                BoardId = boardId,
                Status = TaskStatus.Todo,
                CreatedById = userId
            },
            new TaskItem
            {
                Id = "task2",
                Title = "Task 2",
                BoardId = boardId,
                Status = TaskStatus.InProgress,
                CreatedById = userId
            }
        };

        var board = new Board
        {
            Id = boardId,
            OwnerId = userId,
            IsPublic = false,
            MemberIds = new List<string>()
        };

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        _mockTaskRepository
            .Setup(repo => repo.GetByBoardIdAsync(boardId))
            .ReturnsAsync(tasks);

        var query = new GetBoardTasksQuery(boardId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(t => t.Id == "task1" && t.Title == "Task 1");
        result.Should().Contain(t => t.Id == "task2" && t.Title == "Task 2");
    }

    [Fact]
    public async Task Handle_GetBoardTasks_ShouldReturnEmptyList_WhenUserHasNoAccess()
    {
        // Arrange
        var userId = "user123";
        var boardId = "board123";

        var board = new Board
        {
            Id = boardId,
            OwnerId = "other-owner",
            IsPublic = false,
            MemberIds = new List<string> { "member1" }
        };

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        var query = new GetBoardTasksQuery(boardId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
