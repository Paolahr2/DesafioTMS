using Application.DTOs;
using Application.Queries.Boards;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Application.Tests.Handlers.Boards;

public class GetBoardMembersQueryHandlerTests
{
    private readonly Mock<BoardRepository> _mockBoardRepository;
    private readonly Mock<UserRepository> _mockUserRepository;
    private readonly Application.Queries.Boards.GetBoardMembersQueryHandler _handler;

    public GetBoardMembersQueryHandlerTests()
    {
        _mockBoardRepository = new Mock<BoardRepository>();
        _mockUserRepository = new Mock<UserRepository>();
        _handler = new Application.Queries.Boards.GetBoardMembersQueryHandler(
            _mockBoardRepository.Object,
            _mockUserRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOwnerAndMembers_WhenBoardExists()
    {
        // Arrange
        var ownerId = "owner123";
        var member1Id = "member1";
        var member2Id = "member2";
        var boardId = "board123";

        var board = new Board
        {
            Id = boardId,
            Title = "Test Board",
            OwnerId = ownerId,
            MemberIds = new List<string> { ownerId, member1Id, member2Id },
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        var owner = new Domain.Entities.User
        {
            Id = ownerId,
            Username = "owneruser",
            Email = "owner@test.com",
            FirstName = "Owner",
            LastName = "User"
        };

        var member1 = new Domain.Entities.User
        {
            Id = member1Id,
            Username = "member1user",
            Email = "member1@test.com",
            FirstName = "Member",
            LastName = "One"
        };

        var member2 = new Domain.Entities.User
        {
            Id = member2Id,
            Username = "member2user",
            Email = "member2@test.com",
            FirstName = "Member",
            LastName = "Two"
        };

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        _mockUserRepository
            .Setup(repo => repo.GetByIdAsync(ownerId))
            .ReturnsAsync(owner);

        _mockUserRepository
            .Setup(repo => repo.GetByIdAsync(member1Id))
            .ReturnsAsync(member1);

        _mockUserRepository
            .Setup(repo => repo.GetByIdAsync(member2Id))
            .ReturnsAsync(member2);

        var query = new GetBoardMembersQuery(boardId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        
        var ownerMember = result.First(m => m.UserId == ownerId);
        ownerMember.Role.Should().Be("Owner");
        ownerMember.UserName.Should().Be("owneruser");
        ownerMember.UserEmail.Should().Be("owner@test.com");
        
        var member1Result = result.First(m => m.UserId == member1Id);
        member1Result.Role.Should().Be("Member");
        
        var member2Result = result.First(m => m.UserId == member2Id);
        member2Result.Role.Should().Be("Member");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenBoardNotFound()
    {
        // Arrange
        var boardId = "nonexistent";

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync((Board?)null);

        var query = new GetBoardMembersQuery(boardId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldNotDuplicateOwner_WhenOwnerIsInMembersList()
    {
        // Arrange
        var ownerId = "owner123";
        var boardId = "board123";

        var board = new Board
        {
            Id = boardId,
            Title = "Test Board",
            OwnerId = ownerId,
            MemberIds = new List<string> { ownerId }, // Owner también está en la lista de miembros
            CreatedAt = DateTime.UtcNow
        };

        var owner = new Domain.Entities.User
        {
            Id = ownerId,
            Username = "owneruser",
            Email = "owner@test.com"
        };

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        _mockUserRepository
            .Setup(repo => repo.GetByIdAsync(ownerId))
            .ReturnsAsync(owner);

        var query = new GetBoardMembersQuery(boardId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1); // Solo debe aparecer una vez
        result.First().Role.Should().Be("Owner");
    }

    [Fact]
    public async Task Handle_ShouldSkipNullUsers()
    {
        // Arrange
        var ownerId = "owner123";
        var validMemberId = "member1";
        var invalidMemberId = "invalid";
        var boardId = "board123";

        var board = new Board
        {
            Id = boardId,
            Title = "Test Board",
            OwnerId = ownerId,
            MemberIds = new List<string> { ownerId, validMemberId, invalidMemberId },
            CreatedAt = DateTime.UtcNow
        };

        var owner = new Domain.Entities.User
        {
            Id = ownerId,
            Username = "owneruser",
            Email = "owner@test.com"
        };

        var validMember = new Domain.Entities.User
        {
            Id = validMemberId,
            Username = "validmember",
            Email = "valid@test.com"
        };

        _mockBoardRepository
            .Setup(repo => repo.GetByIdAsync(boardId))
            .ReturnsAsync(board);

        _mockUserRepository
            .Setup(repo => repo.GetByIdAsync(ownerId))
            .ReturnsAsync(owner);

        _mockUserRepository
            .Setup(repo => repo.GetByIdAsync(validMemberId))
            .ReturnsAsync(validMember);

        _mockUserRepository
            .Setup(repo => repo.GetByIdAsync(invalidMemberId))
            .ReturnsAsync((Domain.Entities.User?)null);

        var query = new GetBoardMembersQuery(boardId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2); // Solo owner y validMember
        result.Should().NotContain(m => m.UserId == invalidMemberId);
    }
}
