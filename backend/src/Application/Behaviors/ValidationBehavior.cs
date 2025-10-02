using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Application.Behaviors;

/// <summary>
/// Pipeline behavior que ejecuta validaciones de FluentValidation automáticamente
/// para todos los comandos antes de ejecutar los handlers
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Any())
        {
            // Log de debug con detalles de las fallas para ayudar a depurar
            try
            {
                var details = failures.Select(f => $"Property={f.PropertyName}; Error={f.ErrorMessage}");
                Console.WriteLine("[DEBUG] Validation failures: " + string.Join(" | ", details));
            }
            catch { }

            throw new ValidationException(failures);
        }

        return await next();
    }
}
