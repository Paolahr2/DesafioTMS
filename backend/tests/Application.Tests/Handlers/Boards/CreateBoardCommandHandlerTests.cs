using Application.Commands.Boards;
using Application.DTOs;
using Application.Handlers.Boards;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Application.Tests.Handlers.Boards;

public class CreateBoardCommandHandlerTests
{
    private readonly Mock<BoardRepository> _mockBoardRepository;
    private readonly Mock<ListRepository> _mockListRepository;
    private readonly CreateBoardCommandHandler _handler;

    public CreateBoardCommandHandlerTests()
    {
        _mockBoardRepository = new Mock<BoardRepository>();
        _mockListRepository = new Mock<ListRepository>();
        _handler = new CreateBoardCommandHandler(
            _mockBoardRepository.Object,
            _mockListRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateBoard_WhenValidRequest()
    {
        // Arrange
        var userId = "user123";
        var boardDto = new CreateBoardDto
        {
            Title = "New Board",
            Description = "Board Description",
            Color = "#FF5733",
            IsPublic = false
        };

        _mockBoardRepository
            .Setup(repo => repo.CreateAsync(It.IsAny<Board>()))
            .ReturnsAsync((Board b) => b);

        var command = new CreateBoardCommand(boardDto, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(boardDto.Title);
        result.Description.Should().Be(boardDto.Description);
        result.OwnerId.Should().Be(userId);
        result.Color.Should().Be(boardDto.Color);
        result.IsPublic.Should().Be(boardDto.IsPublic);
        result.IsArchived.Should().BeFalse();
        result.MemberIds.Should().Contain(userId);

        _mockBoardRepository.Verify(
            repo => repo.CreateAsync(It.Is<Board>(b =>
                b.Title == boardDto.Title &&
                b.Description == boardDto.Description &&
                b.OwnerId == userId &&
                b.MemberIds.Contains(userId) &&
                b.IsArchived == false
            )),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSetOwnerAsMember_WhenCreatingBoard()
    {
        // Arrange
        var userId = "user123";
        var boardDto = new CreateBoardDto
        {
            Title = "New Board",
            Description = "Board Description"
        };

        _mockBoardRepository
            .Setup(repo => repo.CreateAsync(It.IsAny<Board>()))
            .ReturnsAsync((Board b) => b);

        var command = new CreateBoardCommand(boardDto, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.MemberIds.Should().ContainSingle();
        result.MemberIds.Should().Contain(userId);
        result.OwnerId.Should().Be(userId);
    }

    [Fact]
    public async Task Handle_ShouldSetIsArchivedToFalse_WhenCreatingBoard()
    {
        // Arrange
        var userId = "user123";
        var boardDto = new CreateBoardDto
        {
            Title = "New Board",
            Description = "Board Description"
        };

        _mockBoardRepository
            .Setup(repo => repo.CreateAsync(It.IsAny<Board>()))
            .ReturnsAsync((Board b) => b);

        var command = new CreateBoardCommand(boardDto, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsArchived.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldSetTimestamps_WhenCreatingBoard()
    {
        // Arrange
        var userId = "user123";
        var boardDto = new CreateBoardDto
        {
            Title = "New Board",
            Description = "Board Description"
        };

        var beforeCreation = DateTime.UtcNow;

        _mockBoardRepository
            .Setup(repo => repo.CreateAsync(It.IsAny<Board>()))
            .ReturnsAsync((Board b) => b);

        var command = new CreateBoardCommand(boardDto, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var afterCreation = DateTime.UtcNow;

        // Assert
        result.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        result.CreatedAt.Should().BeOnOrBefore(afterCreation);
        result.UpdatedAt.Should().BeOnOrAfter(beforeCreation);
        result.UpdatedAt.Should().BeOnOrBefore(afterCreation);
    }

    [Fact]
    public async Task Handle_ShouldReturnBoardDto_WithAllProperties()
    {
        // Arrange
        var userId = "user123";
        var boardDto = new CreateBoardDto
        {
            Title = "Complete Board",
            Description = "Full Description",
            Color = "#123456",
            IsPublic = true
        };

        _mockBoardRepository
            .Setup(repo => repo.CreateAsync(It.IsAny<Board>()))
            .ReturnsAsync((Board b) =>
            {
                b.Id = "board123";
                return b;
            });

        var command = new CreateBoardCommand(boardDto, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Id.Should().Be("board123");
        result.Title.Should().Be(boardDto.Title);
        result.Description.Should().Be(boardDto.Description);
        result.OwnerId.Should().Be(userId);
        result.Color.Should().Be(boardDto.Color);
        result.IsPublic.Should().BeTrue();
        result.IsArchived.Should().BeFalse();
        result.MemberIds.Should().Contain(userId);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
