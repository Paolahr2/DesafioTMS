using Application.DTOs.Users;
using Application.Queries.Users;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.Tests.Handlers.Users;

public class GetAllUsersQueryHandlerTests
{
    private readonly Mock<UserRepository> _mockUserRepository;
    private readonly Mock<ILogger<GetAllUsersQueryHandler>> _mockLogger;
    private readonly GetAllUsersQueryHandler _handler;

    public GetAllUsersQueryHandlerTests()
    {
        _mockUserRepository = new Mock<UserRepository>();
        _mockLogger = new Mock<ILogger<GetAllUsersQueryHandler>>();
        _handler = new GetAllUsersQueryHandler(_mockUserRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnListOfUserDtos_WhenUsersExist()
    {
        // Arrange
        var users = new List<User>
        {
            new User
            {
                Id = "user1",
                Username = "user1",
                Email = "user1@example.com",
                FirstName = "User",
                LastName = "One",
                Role = UserRole.User,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new User
            {
                Id = "user2",
                Username = "user2",
                Email = "user2@example.com",
                FirstName = "User",
                LastName = "Two",
                Role = UserRole.Admin,
                IsActive = false,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };

        _mockUserRepository
            .Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(users);

        var query = new GetAllUsersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var firstUser = result.First();
        firstUser.Id.Should().Be("user1");
        firstUser.Username.Should().Be("user1");
        firstUser.Email.Should().Be("user1@example.com");
        firstUser.FirstName.Should().Be("User");
        firstUser.LastName.Should().Be("One");
        firstUser.Role.Should().Be(UserRole.User);
        firstUser.IsActive.Should().BeTrue();

        var secondUser = result.Last();
        secondUser.Id.Should().Be("user2");
        secondUser.Username.Should().Be("user2");
        secondUser.Email.Should().Be("user2@example.com");
        secondUser.FirstName.Should().Be("User");
        secondUser.LastName.Should().Be("Two");
        secondUser.Role.Should().Be(UserRole.Admin);
        secondUser.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoUsersExist()
    {
        // Arrange
        var users = new List<User>();

        _mockUserRepository
            .Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(users);

        var query = new GetAllUsersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}