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

public class RegisterCommandHandlerTests
{
    private readonly Domain.Interfaces.UserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _userRepository = Substitute.For<Domain.Interfaces.UserRepository>();
        _jwtService = Substitute.For<IJwtService>();
        _handler = new RegisterCommandHandler(_userRepository, _jwtService);
    }

    [Fact]
    public async Task Handle_ShouldRegisterUser_WhenValidRequest()
    {
        // Arrange
        var username = "newuser";
        var email = "newuser@example.com";
        var password = "Password123!";
        var fullName = "New User";
        var token = "jwt-token-123";

        _userRepository.EmailExistsAsync(email).Returns(Task.FromResult(false));
        _userRepository.UsernameExistsAsync(username).Returns(Task.FromResult(false));
        _userRepository.CreateAsync(Arg.Any<User>()).Returns(callInfo =>
        {
            var user = callInfo.Arg<User>();
            user.Id = "user123";
            return Task.FromResult(user);
        });
        _jwtService.GenerateToken(Arg.Any<string>(), email, username).Returns(token);

        var command = new RegisterCommand(username, email, password, fullName);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Usuario creado y autenticado");
        result.Token.Should().Be(token);
        result.User.Should().NotBeNull();
        result.User!.Email.Should().Be(email);
        result.User.Username.Should().Be(username);
        result.User.FullName.Should().Be(fullName);

        await _userRepository.Received(1).EmailExistsAsync(email);
        await _userRepository.Received(1).UsernameExistsAsync(username);
        await _userRepository.Received(1).CreateAsync(Arg.Is<User>(u =>
            u.Email == email &&
            u.Username == username &&
            u.FirstName == "New" &&
            u.LastName == "User" &&
            u.IsActive == true
        ));
        _jwtService.Received(1).GenerateToken(Arg.Any<string>(), email, username);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEmailAlreadyExists()
    {
        // Arrange
        var username = "newuser";
        var email = "existing@example.com";
        var password = "Password123!";
        var fullName = "New User";

        _userRepository.EmailExistsAsync(email).Returns(Task.FromResult(true));

        var command = new RegisterCommand(username, email, password, fullName);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Email ya registrado");
        result.Token.Should().BeNullOrEmpty();
        result.User.Should().BeNull();

        await _userRepository.Received(1).EmailExistsAsync(email);
        await _userRepository.DidNotReceive().UsernameExistsAsync(Arg.Any<string>());
        await _userRepository.DidNotReceive().CreateAsync(Arg.Any<User>());
        _jwtService.DidNotReceive().GenerateToken(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUsernameAlreadyExists()
    {
        // Arrange
        var username = "existinguser";
        var email = "newuser@example.com";
        var password = "Password123!";
        var fullName = "New User";

        _userRepository.EmailExistsAsync(email).Returns(Task.FromResult(false));
        _userRepository.UsernameExistsAsync(username).Returns(Task.FromResult(true));

        var command = new RegisterCommand(username, email, password, fullName);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Username ya existe");
        result.Token.Should().BeNullOrEmpty();
        result.User.Should().BeNull();

        await _userRepository.Received(1).EmailExistsAsync(email);
        await _userRepository.Received(1).UsernameExistsAsync(username);
        await _userRepository.DidNotReceive().CreateAsync(Arg.Any<User>());
        _jwtService.DidNotReceive().GenerateToken(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ShouldHashPassword_WhenCreatingUser()
    {
        // Arrange
        var username = "newuser";
        var email = "newuser@example.com";
        var password = "Password123!";
        var fullName = "New User";
        var token = "jwt-token-123";

        User? capturedUser = null;
        
        _userRepository.EmailExistsAsync(email).Returns(Task.FromResult(false));
        _userRepository.UsernameExistsAsync(username).Returns(Task.FromResult(false));
        _userRepository.CreateAsync(Arg.Any<User>()).Returns(callInfo =>
        {
            var user = callInfo.Arg<User>();
            capturedUser = user; // Capture here
            user.Id = "user123";
            return Task.FromResult(user);
        });
        _jwtService.GenerateToken(Arg.Any<string>(), email, username).Returns(token);

        var command = new RegisterCommand(username, email, password, fullName);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();

        // Verify password was hashed correctly
        capturedUser.Should().NotBeNull();
        capturedUser!.PasswordHash.Should().NotBeNull();
        capturedUser.PasswordHash.Should().NotBe(password);
        BCrypt.Net.BCrypt.Verify(password, capturedUser.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldSetDefaultRole_WhenNotSpecified()
    {
        // Arrange
        var username = "newuser";
        var email = "newuser@example.com";
        var password = "Password123!";
        var fullName = "New User";
        var token = "jwt-token-123";

        _userRepository.EmailExistsAsync(email).Returns(Task.FromResult(false));
        _userRepository.UsernameExistsAsync(username).Returns(Task.FromResult(false));
        _userRepository.CreateAsync(Arg.Any<User>()).Returns(callInfo =>
        {
            var user = callInfo.Arg<User>();
            user.Id = "user123";
            return Task.FromResult(user);
        });
        _jwtService.GenerateToken(Arg.Any<string>(), email, username).Returns(token);

        var command = new RegisterCommand(username, email, password, fullName); // No role specified

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();

        await _userRepository.Received(1).CreateAsync(Arg.Is<User>(u =>
            u.Role == UserRole.User
        ));
    }

    [Fact]
    public async Task Handle_ShouldSetCustomRole_WhenSpecified()
    {
        // Arrange
        var username = "adminuser";
        var email = "admin@example.com";
        var password = "Password123!";
        var fullName = "Admin User";
        var token = "jwt-token-123";

        _userRepository.EmailExistsAsync(email).Returns(Task.FromResult(false));
        _userRepository.UsernameExistsAsync(username).Returns(Task.FromResult(false));
        _userRepository.CreateAsync(Arg.Any<User>()).Returns(callInfo =>
        {
            var user = callInfo.Arg<User>();
            user.Id = "user123";
            return Task.FromResult(user);
        });
        _jwtService.GenerateToken(Arg.Any<string>(), email, username).Returns(token);

        var command = new RegisterCommand(username, email, password, fullName, UserRole.Admin);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();

        await _userRepository.Received(1).CreateAsync(Arg.Is<User>(u =>
            u.Role == UserRole.Admin
        ));
    }

    [Fact]
    public async Task Handle_ShouldParseFullName_IntoFirstAndLastName()
    {
        // Arrange
        var username = "newuser";
        var email = "newuser@example.com";
        var password = "Password123!";
        var fullName = "John Doe";
        var token = "jwt-token-123";

        _userRepository.EmailExistsAsync(email).Returns(Task.FromResult(false));
        _userRepository.UsernameExistsAsync(username).Returns(Task.FromResult(false));
        _userRepository.CreateAsync(Arg.Any<User>()).Returns(callInfo =>
        {
            var user = callInfo.Arg<User>();
            user.Id = "user123";
            return Task.FromResult(user);
        });
        _jwtService.GenerateToken(Arg.Any<string>(), email, username).Returns(token);

        var command = new RegisterCommand(username, email, password, fullName);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();

        await _userRepository.Received(1).CreateAsync(Arg.Is<User>(u =>
            u.FirstName == "John" &&
            u.LastName == "Doe"
        ));
    }

    [Fact]
    public async Task Handle_ShouldHandleFullName_WithMultipleSpaces()
    {
        // Arrange
        var username = "newuser";
        var email = "newuser@example.com";
        var password = "Password123!";
        var fullName = "John Michael Doe";
        var token = "jwt-token-123";

        _userRepository.EmailExistsAsync(email).Returns(Task.FromResult(false));
        _userRepository.UsernameExistsAsync(username).Returns(Task.FromResult(false));
        _userRepository.CreateAsync(Arg.Any<User>()).Returns(callInfo =>
        {
            var user = callInfo.Arg<User>();
            user.Id = "user123";
            return Task.FromResult(user);
        });
        _jwtService.GenerateToken(Arg.Any<string>(), email, username).Returns(token);

        var command = new RegisterCommand(username, email, password, fullName);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();

        await _userRepository.Received(1).CreateAsync(Arg.Is<User>(u =>
            u.FirstName == "John" &&
            u.LastName == "Michael Doe"
        ));
    }

    [Fact]
    public async Task Handle_ShouldSetUserAsActive_WhenCreated()
    {
        // Arrange
        var username = "newuser";
        var email = "newuser@example.com";
        var password = "Password123!";
        var fullName = "New User";
        var token = "jwt-token-123";

        _userRepository.EmailExistsAsync(email).Returns(Task.FromResult(false));
        _userRepository.UsernameExistsAsync(username).Returns(Task.FromResult(false));
        _userRepository.CreateAsync(Arg.Any<User>()).Returns(callInfo =>
        {
            var user = callInfo.Arg<User>();
            user.Id = "user123";
            return Task.FromResult(user);
        });
        _jwtService.GenerateToken(Arg.Any<string>(), email, username).Returns(token);

        var command = new RegisterCommand(username, email, password, fullName);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();

        await _userRepository.Received(1).CreateAsync(Arg.Is<User>(u =>
            u.IsActive == true
        ));
    }
}
