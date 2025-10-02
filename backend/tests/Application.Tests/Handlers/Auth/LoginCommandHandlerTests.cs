using Application.Commands.Auth;
using Application.DTOs;
using Application.Handlers.Auth;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Application.Tests.Handlers.Auth;

public class LoginCommandHandlerTests
{
    private readonly Domain.Interfaces.UserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userRepository = Substitute.For<Domain.Interfaces.UserRepository>();
        _jwtService = Substitute.For<IJwtService>();
        _handler = new LoginCommandHandler(_userRepository, _jwtService);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCredentialsAreValidWithEmail()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Password123!";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var userId = "user123";
        var token = "jwt-token-123";

        var user = new User
        {
            Id = userId,
            Email = email,
            Username = "testuser",
            PasswordHash = hashedPassword,
            FirstName = "Test",
            LastName = "User",
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _userRepository.GetByEmailAsync(email).Returns(Task.FromResult<User?>(user));
        _jwtService.GenerateToken(userId, email, "testuser").Returns(token);

        var command = new LoginCommand(email, password);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Token.Should().Be(token);
        result.User.Should().NotBeNull();
        result.User!.Email.Should().Be(email);
        result.User.Username.Should().Be("testuser");

        await _userRepository.Received(1).GetByEmailAsync(email);
        _jwtService.Received(1).GenerateToken(userId, email, "testuser");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCredentialsAreValidWithUsername()
    {
        // Arrange
        var username = "testuser";
        var password = "Password123!";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var userId = "user123";
        var token = "jwt-token-123";

        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            Username = username,
            PasswordHash = hashedPassword,
            FirstName = "Test",
            LastName = "User",
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _userRepository.GetByEmailAsync(username).Returns(Task.FromResult<User?>(null));
        _userRepository.GetByUsernameAsync(username).Returns(Task.FromResult<User?>(user));
        _jwtService.GenerateToken(userId, "test@example.com", username).Returns(token);

        var command = new LoginCommand(username, password);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Token.Should().Be(token);
        result.User.Should().NotBeNull();
        result.User!.Username.Should().Be(username);

        await _userRepository.Received(1).GetByEmailAsync(username);
        await _userRepository.Received(1).GetByUsernameAsync(username);
        _jwtService.Received(1).GenerateToken(userId, "test@example.com", username);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var emailOrUsername = "nonexistent@example.com";
        var password = "Password123!";

        _userRepository.GetByEmailAsync(emailOrUsername).Returns(Task.FromResult<User?>(null));
        _userRepository.GetByUsernameAsync(emailOrUsername).Returns(Task.FromResult<User?>(null));
        _userRepository.GetAllAsync().Returns(Task.FromResult<IEnumerable<User>>(new List<User>()));

        var command = new LoginCommand(emailOrUsername, password);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Usuario no encontrado");
        result.Token.Should().BeNullOrEmpty();
        result.User.Should().BeNull();

        await _userRepository.Received(1).GetByEmailAsync(emailOrUsername);
        // El handler llama GetByUsernameAsync dos veces en caso de no encontrar usuario
        await _userRepository.Received().GetByUsernameAsync(emailOrUsername);
        _jwtService.DidNotReceive().GenerateToken(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPasswordIsIncorrect()
    {
        // Arrange
        var email = "test@example.com";
        var correctPassword = "CorrectPassword123!";
        var incorrectPassword = "WrongPassword456!";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(correctPassword);

        var user = new User
        {
            Id = "user123",
            Email = email,
            Username = "testuser",
            PasswordHash = hashedPassword,
            FirstName = "Test",
            LastName = "User",
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _userRepository.GetByEmailAsync(email).Returns(Task.FromResult<User?>(user));

        var command = new LoginCommand(email, incorrectPassword);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Credenciales inválidas");
        result.Token.Should().BeNullOrEmpty();
        result.User.Should().BeNull();

        await _userRepository.Received(1).GetByEmailAsync(email);
        _jwtService.DidNotReceive().GenerateToken(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPasswordHashIsNull()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Password123!";

        var user = new User
        {
            Id = "user123",
            Email = email,
            Username = "testuser",
            PasswordHash = null!, // Password hash is null
            FirstName = "Test",
            LastName = "User",
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _userRepository.GetByEmailAsync(email).Returns(Task.FromResult<User?>(user));

        var command = new LoginCommand(email, password);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Credenciales inválidas");
        result.Token.Should().BeNullOrEmpty();
        result.User.Should().BeNull();

        _jwtService.DidNotReceive().GenerateToken(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ShouldReturnUserDto_WithCorrectProperties()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Password123!";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var userId = "user123";
        var username = "testuser";
        var fullName = "Test User";
        var createdAt = DateTime.UtcNow;
        var token = "jwt-token-123";

        var user = new User
        {
            Id = userId,
            Email = email,
            Username = username,
            PasswordHash = hashedPassword,
            FirstName = "Test",
            LastName = "User",
            Role = UserRole.User,
            CreatedAt = createdAt,
            IsActive = true
        };

        _userRepository.GetByEmailAsync(email).Returns(Task.FromResult<User?>(user));
        _jwtService.GenerateToken(userId, email, username).Returns(token);

        var command = new LoginCommand(email, password);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.User.Should().NotBeNull();
        result.User!.Id.Should().Be(userId);
        result.User.Email.Should().Be(email);
        result.User.Username.Should().Be(username);
        result.User.FullName.Should().Be(fullName);
        result.User.CreatedAt.Should().Be(createdAt);
    }
}
