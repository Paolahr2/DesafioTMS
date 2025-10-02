using Application.Commands.Tasks;
using Application.DTOs;
using Application.Queries.Tasks;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using TaskStatus = Domain.Enums.TaskStatus;

namespace Application.Tests.Handlers.Tasks;

public class CreateTaskCommandHandlerTests
{
    private readonly Mock<TaskRepository> _mockTaskRepository;
    private readonly Mock<BoardRepository> _mockBoardRepository;
    private readonly Mock<ILogger<CreateTaskCommandHandler>> _mockLogger;
    private readonly CreateTaskCommandHandler _handler;

    public CreateTaskCommandHandlerTests()
    {
        _mockTaskRepository = new Mock<TaskRepository>();
        _mockBoardRepository = new Mock<BoardRepository>();
        _mockLogger = new Mock<ILogger<CreateTaskCommandHandler>>();
        _handler = new CreateTaskCommandHandler(
            _mockTaskRepository.Object,
            _mockBoardRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateTask_WhenUserHasAccess()
    {
        // Arrange
        var userId = "user123";
        var boardId = "board123";

        var createDto = new CreateTaskDto
        {
            Title = "Test Task",
            Description = "Test Description",
            BoardId = boardId,
            Status = TaskStatus.Todo,
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(7),
            Tags = new List<string> { "urgent", "important" }
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
            .Setup(repo => repo.CreateAsync(It.IsAny<TaskItem>()))
            .ReturnsAsync((TaskItem task) => { task.Id = "generated-task-id"; return task; });

        var command = new CreateTaskCommand(createDto, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
        result.Title.Should().Be("Test Task");
        result.Description.Should().Be("Test Description");
        result.BoardId.Should().Be(boardId);
        result.Status.Should().Be(TaskStatus.Todo);
        result.Priority.Should().Be(TaskPriority.High);
        result.CreatedById.Should().Be(userId);
        result.DueDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromMinutes(1));
        result.Tags.Should().BeEquivalentTo(new List<string> { "urgent", "important" });
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _mockTaskRepository.Verify(repo => repo.CreateAsync(It.Is<TaskItem>(t =>
            t.Title == "Test Task" &&
            t.Description == "Test Description" &&
            t.BoardId == boardId &&
            t.Status == TaskStatus.Todo &&
            t.Priority == TaskPriority.High &&
            t.CreatedById == userId
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateTaskWithAssignedUser_WhenAssignedToIdProvided()
    {
        // Arrange
        var userId = "user123";
        var boardId = "board123";
        var assignedUserId = "assigned-user-123";

        var createDto = new CreateTaskDto
        {
            Title = "Assigned Task",
            BoardId = boardId,
            AssignedToId = assignedUserId
        };

        var board = new Board
        {
            Id = boardId,
            OwnerId = userId,
            IsPublic = false,
            MemberIds = new List<string> { assignedUserId }
        };

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        _mockTaskRepository
            .Setup(repo => repo.CreateAsync(It.IsAny<TaskItem>()))
            .ReturnsAsync((TaskItem task) => { task.Id = "generated-task-id-2"; return task; });

        var command = new CreateTaskCommand(createDto, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.AssignedToId.Should().Be(assignedUserId);
        result.CreatedById.Should().Be(userId);

        _mockTaskRepository.Verify(repo => repo.CreateAsync(It.Is<TaskItem>(t =>
            t.AssignedToId == assignedUserId &&
            t.CreatedById == userId
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserHasNoAccess()
    {
        // Arrange
        var userId = "user123";
        var boardId = "board123";

        var createDto = new CreateTaskDto
        {
            Title = "Test Task",
            BoardId = boardId
        };

        var board = new Board
        {
            Id = boardId,
            OwnerId = "other-owner",
            IsPublic = false,
            MemberIds = new List<string> { "member1", "member2" }
        };

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        var command = new CreateTaskCommand(createDto, userId);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentException_WhenBoardDoesNotExist()
    {
        // Arrange
        var userId = "user123";
        var boardId = "non-existent-board";

        var createDto = new CreateTaskDto
        {
            Title = "Test Task",
            BoardId = boardId
        };

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(() => null!);

        var command = new CreateTaskCommand(createDto, userId);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldCreateTaskWithDefaultValues_WhenOptionalFieldsNotProvided()
    {
        // Arrange
        var userId = "user123";
        var boardId = "board123";

        var createDto = new CreateTaskDto
        {
            Title = "Minimal Task",
            BoardId = boardId
            // No other fields provided
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
            .Setup(repo => repo.CreateAsync(It.IsAny<TaskItem>()))
            .ReturnsAsync((TaskItem task) => { task.Id = "generated-task-id-3"; return task; });

        var command = new CreateTaskCommand(createDto, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(TaskStatus.Todo);
        result.Priority.Should().Be(TaskPriority.Medium);
        result.Tags.Should().BeEmpty();
        result.AssignedToId.Should().BeNull();
        result.DueDate.Should().BeNull();
        result.ListId.Should().BeNull();
    }
}

