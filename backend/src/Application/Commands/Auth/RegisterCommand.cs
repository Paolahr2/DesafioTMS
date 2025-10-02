// ============================================================================
// REGISTER COMMAND - CLEAN ARCHITECTURE
// ============================================================================

using MediatR;
using Application.DTOs;
using Domain.Enums;

namespace Application.Commands.Auth
{
    public record RegisterCommand(
        string Username,
        string Email, 
        string Password,
        string FullName,
        UserRole Role = UserRole.User
    ) : IRequest<Application.DTOs.LoginResultDto>;
}
