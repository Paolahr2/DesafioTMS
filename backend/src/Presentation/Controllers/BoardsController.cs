using Application.Commands.Boards;
using Application.DTOs;
using Application.Queries.Boards;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TaskManager.Controllers;

[ApiController]
[Route("api/boards")]
[Authorize]
[Tags("Boards")]
public class BoardsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BoardsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtiene todos los tableros del usuario
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<BoardDto>), 200)]
    public async Task<ActionResult<List<BoardDto>>> GetBoards()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var query = new GetUserBoardsQuery(userId, userRole ?? "User");
        var boards = await _mediator.Send(query);
        return Ok(boards);
    }

    /// <summary>
    /// Obtiene un tablero por su ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BoardDto), 200)]
    public async Task<ActionResult<BoardDto>> GetBoardById(string id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var query = new GetBoardByIdQuery(id, userId);
        var board = await _mediator.Send(query);
        return board != null ? Ok(board) : NotFound();
    }

    /// <summary>
    /// Crea un nuevo tablero
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BoardDto), 201)]
    public async Task<ActionResult<BoardDto>> CreateBoard([FromBody] CreateBoardDto boardDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var command = new CreateBoardCommand(boardDto, userId);
        var result = await _mediator.Send(command) as BoardDto;
        return CreatedAtAction(nameof(GetBoardById), new { id = result?.Id }, result);
    }

    /// <summary>
    /// Actualiza un tablero existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BoardDto), 200)]
    public async Task<ActionResult<BoardDto>> UpdateBoard(string id, [FromBody] UpdateBoardDto boardDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var command = new UpdateBoardCommand(id, boardDto, userId);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Elimina un tablero
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    public async Task<ActionResult> DeleteBoard(string id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var command = new DeleteBoardCommand(id, userId);
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Endpoint de prueba para invitaciones
    /// </summary>
    [HttpPost("{id}/invite-test")]
    public async Task<IActionResult> InviteUserTest(string id, [FromBody] object data)
    {
        Console.WriteLine($"[TEST] InviteUserTest called with boardId: {id}");
        Console.WriteLine($"[TEST] Data received: {System.Text.Json.JsonSerializer.Serialize(data)}");
        return Ok(new { message = "Test successful", boardId = id, data });
    }

    /// <summary>
    /// Invita a un usuario a un tablero
    /// </summary>
    [HttpPost("{id}/invite")]
    [ProducesResponseType(typeof(BoardInvitationDto), 201)]
    public async Task<ActionResult<BoardInvitationDto>> InviteUser(string id, [FromBody] InviteUserToBoardDto inviteDto)
    {
        Console.WriteLine($"[DEBUG] InviteUser called with boardId: {id}");
        Console.WriteLine($"[DEBUG] InviteDto - Email: '{inviteDto?.Email}', Username: '{inviteDto?.Username}', Role: '{inviteDto?.Role}', Message: '{inviteDto?.Message}'");

        // Verificar validación del modelo
        if (!ModelState.IsValid)
        {
            Console.WriteLine($"[DEBUG] ModelState is invalid");
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"[DEBUG] Validation error: {error.ErrorMessage}");
            }
            return BadRequest(ModelState);
        }

        Console.WriteLine($"[DEBUG] ModelState is valid, proceeding with invitation");

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        Console.WriteLine($"[DEBUG] UserId from token: {userId}");

        var command = new InviteUserToBoardCommand(id, inviteDto, userId);
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetBoardById), new { id = result.BoardId }, result);
    }

    /// <summary>
    /// Obtiene las invitaciones pendientes del usuario
    /// </summary>
    [HttpGet("invitations/pending")]
    [ProducesResponseType(typeof(List<BoardInvitationDto>), 200)]
    public async Task<ActionResult<List<BoardInvitationDto>>> GetPendingInvitations()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var query = new GetPendingInvitationsQuery(userId);
        var invitations = await _mediator.Send(query);
        return Ok(invitations);
    }

    /// <summary>
    /// Responde a una invitación (aceptar/rechazar)
    /// </summary>
    [HttpPost("invitations/{invitationId}/respond")]
    [ProducesResponseType(200)]
    public async Task<ActionResult> RespondToInvitation(string invitationId, [FromBody] RespondToInvitationDto responseDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var command = new RespondToInvitationCommand(invitationId, responseDto, userId);
        var result = await _mediator.Send(command);
        return Ok(new { success = result });
    }

    /// <summary>
    /// Obtiene los miembros de un tablero
    /// </summary>
    [HttpGet("{id}/members")]
    [ProducesResponseType(typeof(List<BoardMemberDto>), 200)]
    public async Task<ActionResult<List<BoardMemberDto>>> GetBoardMembers(string id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Verificar que el usuario tenga acceso al tablero
        var boardQuery = new GetBoardByIdQuery(id, userId);
        var board = await _mediator.Send(boardQuery);
        if (board == null)
            return NotFound();

        var query = new GetBoardMembersQuery(id);
        var members = await _mediator.Send(query);
        return Ok(members);
    }

    /// <summary>
    /// Elimina un miembro del tablero
    /// </summary>
    [HttpDelete("{id}/members/{memberId}")]
    [ProducesResponseType(204)]
    public async Task<ActionResult> RemoveBoardMember(string id, string memberId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var command = new RemoveBoardMemberCommand(id, memberId, userId);
        var result = await _mediator.Send(command);
        return result ? NoContent() : BadRequest("No se pudo eliminar el miembro");
    }

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
            throw new UnauthorizedAccessException("Usuario no autenticado");
    }
}
