using Application.DTOs.Users;
using Application.Queries.Users;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Queries.Users;

/// <summary>
/// Handler responsable únicamente de la consulta de todos los usuarios
/// </summary>
public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
{
    private readonly Domain.Interfaces.UserRepository _userRepository;
    private readonly ILogger<GetAllUsersQueryHandler> _logger;

    public GetAllUsersQueryHandler(
        Domain.Interfaces.UserRepository userRepository, 
        ILogger<GetAllUsersQueryHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving all users from database");

            var users = await _userRepository.GetAllAsync();
            
            var userDtos = users.Select(user => new UserDto
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

            _logger.LogInformation("Successfully retrieved {Count} users", userDtos.Count);

            return userDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users from database");
            throw;
        }
    }
}
