using Application.Commands.Tasks;
using Application.DTOs;
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

public class UpdateTaskCommandHandlerTests
{
    private readonly Mock<TaskRepository> _mockTaskRepository;
    private readonly Mock<BoardRepository> _mockBoardRepository;
    private readonly Mock<NotificationRepository> _mockNotificationRepository;
    private readonly Mock<ILogger<UpdateTaskCommandHandler>> _mockLogger;
    private readonly Mock<IMediator> _mockMediator;
    private readonly UpdateTaskCommandHandler _handler;

    public UpdateTaskCommandHandlerTests()
    {
        _mockTaskRepository = new Mock<TaskRepository>();
        _mockBoardRepository = new Mock<BoardRepository>();
        _mockNotificationRepository = new Mock<NotificationRepository>();
        _mockLogger = new Mock<ILogger<UpdateTaskCommandHandler>>();
        _mockMediator = new Mock<IMediator>();
        _handler = new UpdateTaskCommandHandler(
            _mockTaskRepository.Object,
            _mockBoardRepository.Object,
            _mockNotificationRepository.Object,
            _mockLogger.Object,
            _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateTask_WhenUserHasAccess()
    {
        // Arrange
        var userId = "user123";
        var taskId = "task123";
        var boardId = "board123";

        var existingTask = new TaskItem
        {
            Id = taskId,
            Title = "Original Title",
            Description = "Original Description",
            Status = TaskStatus.Todo,
            BoardId = boardId,
            Priority = TaskPriority.Medium,
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

        var updateDto = new UpdateTaskDto
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Status = TaskStatus.InProgress,
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(3),
            Tags = new List<string> { "updated" }
        };

        _mockTaskRepository
            .Setup(repo => repo.GetByIdAsync(taskId))
            .ReturnsAsync(existingTask);

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        _mockTaskRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<TaskItem>()))
            .ReturnsAsync((TaskItem task) => task);

        var command = new UpdateTaskCommand(taskId, updateDto, userId);

        var originalUpdatedAt = existingTask.UpdatedAt;

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(taskId);
        result.Title.Should().Be("Updated Title");
        result.Description.Should().Be("Updated Description");
        result.Status.Should().Be(TaskStatus.InProgress);
        result.Priority.Should().Be(TaskPriority.High);
        result.DueDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(3), TimeSpan.FromMinutes(1));
        result.Tags.Should().BeEquivalentTo(new List<string> { "updated" });
        result.UpdatedAt.Should().BeAfter(originalUpdatedAt);

        _mockTaskRepository.Verify(repo => repo.UpdateAsync(It.Is<TaskItem>(t =>
            t.Title == "Updated Title" &&
            t.Description == "Updated Description" &&
            t.Status == TaskStatus.InProgress &&
            t.Priority == TaskPriority.High
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateOnlyProvidedFields()
    {
        // Arrange
        var userId = "user123";
        var taskId = "task123";
        var boardId = "board123";

        var existingTask = new TaskItem
        {
            Id = taskId,
            Title = "Original Title",
            Description = "Original Description",
            Status = TaskStatus.Todo,
            Priority = TaskPriority.Medium,
            BoardId = boardId,
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

        var updateDto = new UpdateTaskDto
        {
            Title = "Updated Title"
            // Only Title provided, other fields should remain unchanged
        };

        _mockTaskRepository
            .Setup(repo => repo.GetByIdAsync(taskId))
            .ReturnsAsync(existingTask);

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        _mockTaskRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<TaskItem>()))
            .ReturnsAsync((TaskItem task) => task);

        var command = new UpdateTaskCommand(taskId, updateDto, userId);

        var originalUpdatedAt = existingTask.UpdatedAt;

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated Title");
        result.Description.Should().Be("Original Description"); // Unchanged
        result.Status.Should().Be(TaskStatus.Todo); // Unchanged
        result.Priority.Should().Be(TaskPriority.Medium); // Unchanged
        result.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenTaskDoesNotExist()
    {
        // Arrange
        var userId = "user123";
        var taskId = "non-existent-task";

        var updateDto = new UpdateTaskDto
        {
            Title = "Updated Title"
        };

        _mockTaskRepository
            .Setup(repo => repo.GetByIdAsync(taskId))
            .ReturnsAsync((TaskItem)null);

        var command = new UpdateTaskCommand(taskId, updateDto, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserHasNoAccess()
    {
        // Arrange
        var userId = "user123";
        var taskId = "task123";
        var boardId = "board123";

        var existingTask = new TaskItem
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
            MemberIds = new List<string> { "member1" }
        };

        var updateDto = new UpdateTaskDto
        {
            Title = "Updated Title"
        };

        _mockTaskRepository
            .Setup(repo => repo.GetByIdAsync(taskId))
            .ReturnsAsync(existingTask);

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        var command = new UpdateTaskCommand(taskId, updateDto, userId);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldUpdateAssignedToId_WhenProvided()
    {
        // Arrange
        var userId = "user123";
        var taskId = "task123";
        var boardId = "board123";
        var assignedUserId = "assigned-user-123";

        var existingTask = new TaskItem
        {
            Id = taskId,
            Title = "Original Title",
            BoardId = boardId,
            AssignedToId = null,
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var board = new Board
        {
            Id = boardId,
            OwnerId = userId,
            IsPublic = false,
            MemberIds = new List<string> { assignedUserId }
        };

        var updateDto = new UpdateTaskDto
        {
            AssignedToId = assignedUserId
        };

        _mockTaskRepository
            .Setup(repo => repo.GetByIdAsync(taskId))
            .ReturnsAsync(existingTask);

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        _mockTaskRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<TaskItem>()))
            .ReturnsAsync((TaskItem task) => task);

        _mockNotificationRepository
            .Setup(repo => repo.CreateAsync(It.IsAny<Notification>()))
            .ReturnsAsync((Notification n) => n);

        var command = new UpdateTaskCommand(taskId, updateDto, userId);

        var originalUpdatedAt = existingTask.UpdatedAt;

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.AssignedToId.Should().Be(assignedUserId);

        // Verify notification was created
        _mockNotificationRepository.Verify(repo => repo.CreateAsync(It.Is<Notification>(n =>
            n.UserId == assignedUserId &&
            n.Type == NotificationType.TaskAssigned &&
            n.Title == "Nueva tarea asignada" &&
            n.Message == $"Se te ha asignado la tarea: {existingTask.Title}" &&
            n.Data.ContainsKey("taskId") &&
            n.Data.ContainsKey("boardId") &&
            n.Data.ContainsKey("assignedBy") &&
            !n.IsRead
        )), Times.Once);
        result.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }
}