using Application.DTOs.Users;
using Application.Queries.Users;
using Application.Commands.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// OBTENER todos los usuarios del sistema
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAllUsers()
    {
        try
        {
            var query = new GetAllUsersQuery();
            var users = await _mediator.Send(query);
            return Ok(users);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// OBTENER usuario por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(string id)
    {
        try
        {
            var query = new GetUserByIdQuery(id);
            var user = await _mediator.Send(query);
            
            if (user == null)
                return NotFound(new { message = "Usuario no encontrado" });
                
            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// ACTUALIZAR información del usuario
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(string id, [FromBody] UpdateUserDto updateDto)
    {
        try
        {
            // Verificar que el usuario solo pueda actualizar su propio perfil
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId != id)
                return Forbid("No puedes actualizar el perfil de otro usuario");

            var command = new UpdateUserCommand(id, updateDto.FirstName, updateDto.LastName, updateDto.Email);
            var updatedUser = await _mediator.Send(command);
            
            return Ok(updatedUser);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// ELIMINAR usuario
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(string id)
    {
        try
        {
            // Solo el propio usuario o admin puede eliminar la cuenta
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId != id)
                return Forbid("No puedes eliminar la cuenta de otro usuario");

            var command = new DeleteUserCommand(id);
            var success = await _mediator.Send(command);
            
            if (!success)
                return NotFound(new { message = "Usuario no encontrado" });
                
            return Ok(new { message = "Usuario eliminado exitosamente" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// OBTENER perfil del usuario actual (token JWT)
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetMyProfile()
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            var query = new GetUserByIdQuery(currentUserId);
            var user = await _mediator.Send(query);
            
            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
