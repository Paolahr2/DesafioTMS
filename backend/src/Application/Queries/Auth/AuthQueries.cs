using Application.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Queries.Auth;

// Query para obtener todos los usuarios registrados
public record GetAllUsersQuery() : IRequest<List<UserDto>>;

// Handler para queries de autenticación
public class AuthQueryHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
{
    private readonly Domain.Interfaces.UserRepository _userRepository;
    private readonly ILogger<AuthQueryHandler> _logger;

    public AuthQueryHandler(Domain.Interfaces.UserRepository userRepository, ILogger<AuthQueryHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            
            return users.Select(user => new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            throw;
        }
    }
}
