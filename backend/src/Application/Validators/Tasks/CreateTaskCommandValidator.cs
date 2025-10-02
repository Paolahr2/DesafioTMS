using Application.Commands.Tasks;
using Domain.Enums;
using FluentValidation;

namespace Application.Validators.Tasks;

/// <summary>
/// Validador para el comando de creación de tareas siguiendo FluentValidation
/// </summary>
public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Task.Title)
            .NotEmpty().WithMessage("El título de la tarea es obligatorio")
            .MaximumLength(100).WithMessage("El título no puede exceder 100 caracteres");

        RuleFor(x => x.Task.Description)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Task.Description));

        RuleFor(x => x.Task.BoardId)
            .NotEmpty().WithMessage("El ID del tablero es obligatorio");

        RuleFor(x => x.Task.Priority)
            .IsInEnum().WithMessage("La prioridad debe ser un valor válido")
            .When(x => x.Task.Priority.HasValue);

        RuleFor(x => x.Task.DueDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("La fecha de vencimiento debe ser futura")
            .When(x => x.Task.DueDate.HasValue);

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El ID del usuario creador es obligatorio");
    }
}
