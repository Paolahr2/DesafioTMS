using MediatR;
using Application.Commands.Auth;
using Application.DTOs;
using Domain.Interfaces;
using Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.Auth;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, LoginResultDto>
{
	private readonly Domain.Interfaces.UserRepository _userRepo;
	private readonly Application.Services.IJwtService _jwtService;

	public RegisterCommandHandler(Domain.Interfaces.UserRepository userRepo, Application.Services.IJwtService jwtService)
	{
		_userRepo = userRepo;
		_jwtService = jwtService;
	}

	public async Task<LoginResultDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
	{
		// Verificar unicidad
		// El RegisterCommand expone Username, Email, Password, FullName
		if (await _userRepo.EmailExistsAsync(request.Email))
		{
			return new LoginResultDto { Success = false, Message = "Email ya registrado" };
		}

		if (await _userRepo.UsernameExistsAsync(request.Username))
		{
			return new LoginResultDto { Success = false, Message = "Username ya existe" };
		}

		// Crear usuario
		var nameParts = (request.FullName ?? string.Empty).Split(' ', 2);
		var firstName = nameParts.Length > 0 ? nameParts[0] : string.Empty;
		var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

		var user = new User
		{
			FirstName = firstName,
			LastName = lastName,
			Email = request.Email,
			Username = request.Username,
			PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
			Role = request.Role,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			IsActive = true
		};

		var created = await _userRepo.CreateAsync(user);

		// Generar token y devolver estructura similar a Login
		var token = _jwtService.GenerateToken(user.Id, user.Email, user.Username);

		return new LoginResultDto
		{
			Success = true,
			Message = "Usuario creado y autenticado",
			Token = token,
			User = new Application.DTOs.UserDto
			{
				Id = user.Id,
				Username = user.Username,
				Email = user.Email,
				FullName = user.FullName,
				CreatedAt = user.CreatedAt
			}
		};
	}
}

