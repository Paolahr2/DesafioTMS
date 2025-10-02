using Application.Commands.Auth;
using FluentValidation;

namespace Application.Validators.Auth;

/// <summary>
/// Validador para el comando de registro
/// </summary>
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
	public RegisterCommandValidator()
	{
		RuleFor(x => x.Username)
			.NotEmpty().WithMessage("El nombre de usuario es obligatorio")
			.MinimumLength(3).WithMessage("El nombre de usuario debe tener al menos 3 caracteres")
			.MaximumLength(50).WithMessage("El nombre de usuario no puede exceder 50 caracteres")
			.Matches("^[a-zA-Z0-9_]+$").WithMessage("El nombre de usuario solo puede contener letras, números y guiones bajos");

		RuleFor(x => x.Email)
			.NotEmpty().WithMessage("El email es obligatorio")
			.EmailAddress().WithMessage("El formato del email es inválido")
			.MaximumLength(200).WithMessage("El email no puede exceder 200 caracteres");

		RuleFor(x => x.Password)
			.NotEmpty().WithMessage("La contraseña es obligatoria")
			.MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres")
			.MaximumLength(100).WithMessage("La contraseña no puede exceder 100 caracteres")
			.Matches("[A-Z]").WithMessage("La contraseña debe contener al menos una letra mayúscula")
			.Matches("[a-z]").WithMessage("La contraseña debe contener al menos una letra minúscula")
			.Matches("[0-9]").WithMessage("La contraseña debe contener al menos un número");

		RuleFor(x => x.FullName)
			.NotEmpty().WithMessage("El nombre completo es obligatorio")
			.MinimumLength(2).WithMessage("El nombre completo debe tener al menos 2 caracteres")
			.MaximumLength(200).WithMessage("El nombre completo no puede exceder 200 caracteres");
	}
}
