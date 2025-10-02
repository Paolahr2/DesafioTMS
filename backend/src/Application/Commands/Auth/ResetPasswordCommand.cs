using MediatR;

namespace Application.Commands.Auth;

public record ResetPasswordCommand(string Email, string NewPassword, string Token) : IRequest<object>;
