using Domain.Interfaces;
using MediatR;

namespace Application.Commands.Users;

/// <summary>
/// Handler para el comando DeleteUser
/// </summary>
public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
{
    private readonly UserRepository _userRepository;

    public DeleteUserCommandHandler(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            return false;
        }

        return await _userRepository.DeleteAsync(request.UserId);
    }
}
