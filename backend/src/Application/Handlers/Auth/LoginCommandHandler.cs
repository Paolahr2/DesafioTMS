using MediatR;
using Application.Commands.Auth;
using Application.DTOs;
using Domain.Interfaces;
using Application.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.Auth;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResultDto>
{
	private readonly Domain.Interfaces.UserRepository _userRepo;
	private readonly IJwtService _jwtService;

	public LoginCommandHandler(Domain.Interfaces.UserRepository userRepo, IJwtService jwtService)
	{
		_userRepo = userRepo;
		_jwtService = jwtService;
	}

	public async Task<LoginResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
	{
		// Debug: mostrar qué se está buscando
		Console.WriteLine($"[DEBUG] Login attempt for: '{request.EmailOrUsername}'");

		// Buscar por email primero
		var userByEmail = await _userRepo.GetByEmailAsync(request.EmailOrUsername);
		Console.WriteLine($"[DEBUG] Search by email '{request.EmailOrUsername}': {(userByEmail != null ? $"Found user {userByEmail.Username}" : "Not found")}");

		// Si no encontró por email, buscar por username
		var user = userByEmail ?? await _userRepo.GetByUsernameAsync(request.EmailOrUsername);
		if (user == null)
		{
			// Si aún no encontró, buscar por username también (por si acaso)
			var userByUsername = await _userRepo.GetByUsernameAsync(request.EmailOrUsername);
			Console.WriteLine($"[DEBUG] Search by username '{request.EmailOrUsername}': {(userByUsername != null ? $"Found user {userByUsername.Username}" : "Not found")}");

			// Debug: mostrar qué usuarios existen
			var allUsers = await _userRepo.GetAllAsync();
			var userList = string.Join(", ", allUsers.Select(u => $"{u.Username} ({u.Email})"));
			Console.WriteLine($"[DEBUG] User not found. Registered users: {userList}");
			return new LoginResultDto { Success = false, Message = $"Usuario no encontrado. Usuarios registrados: {userList}" };
		}

		// Verificar contraseña
		try
		{
			Console.WriteLine($"[DEBUG] Verifying password for user '{user.Username}'. Incoming password length: {request.Password?.Length ?? 0}, Stored hash length: {user.PasswordHash?.Length ?? 0}");
			var valid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
			if (!valid)
			{
				Console.WriteLine($"[DEBUG] Password verification failed for user '{user.Username}'. Hash prefix: { (string.IsNullOrEmpty(user.PasswordHash) ? "(empty)" : user.PasswordHash.Substring(0, Math.Min(6, user.PasswordHash.Length))) }");
				return new LoginResultDto { Success = false, Message = "Credenciales inválidas" };
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[ERROR] Exception during password verification for '{user?.Username}': {ex.Message}");
			return new LoginResultDto { Success = false, Message = "Credenciales inválidas" };
		}

		// Generar token
	var token = _jwtService.GenerateToken(user.Id, user.Email, user.Username);

		return new LoginResultDto
		{
			Success = true,
			Message = "Login correcto",
			Token = token,
			User = new UserDto {
				Id = user.Id,
				Username = user.Username,
				Email = user.Email,
				FullName = user.FullName,
				CreatedAt = user.CreatedAt
			}
		};
	}
}

