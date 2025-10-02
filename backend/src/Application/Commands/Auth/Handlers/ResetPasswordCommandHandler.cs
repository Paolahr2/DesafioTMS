using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Commands.Auth.Handlers;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, object>
{
    private readonly Domain.Interfaces.UserRepository _userRepository;

    public ResetPasswordCommandHandler(Domain.Interfaces.UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<object> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        // Buscar usuario por email
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            // No revelar demasiado: lanzar error controlado
            throw new ArgumentException("Usuario no encontrado para el correo proporcionado.");
        }

        // Actualizar hash de contraseña
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        var updated = await _userRepository.UpdateAsync(user);

        // Retornar estructura simple consistente con otros handlers
        return new { success = true, message = "Contraseña actualizada correctamente" };
    }
}
