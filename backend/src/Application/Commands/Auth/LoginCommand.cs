// ============================================================================
// LOGIN COMMAND - CLEAN ARCHITECTURE
// ============================================================================

using MediatR;
using Application.DTOs;

namespace Application.Commands.Auth
{
    public record LoginCommand(
        string EmailOrUsername, 
        string Password
    ) : IRequest<LoginResultDto>;
}
