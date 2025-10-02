using Application.Commands.Lists;
using Application.DTOs;
using Application.Queries.Lists;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TaskManager.Controllers;

[ApiController]
[Route("api/lists")]
[Authorize]
[Tags("Lists")]
public class ListsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ListsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtiene todas las listas de un tablero
    /// </summary>
    [HttpGet("board/{boardId}")]
    [ProducesResponseType(typeof(List<ListDto>), 200)]
    public async Task<ActionResult<List<ListDto>>> GetListsByBoardId(string boardId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var query = new GetListsByBoardIdQuery(boardId, userId);
        var lists = await _mediator.Send(query);
        return Ok(lists);
    }

    /// <summary>
    /// Crea una nueva lista
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ListDto), 201)]
    public async Task<ActionResult<ListDto>> CreateList(CreateListDto listDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var command = new CreateListCommand(listDto, userId);
        var list = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetListsByBoardId), new { boardId = list.BoardId }, list);
    }

    /// <summary>
    /// Actualiza una lista
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ListDto), 200)]
    public async Task<ActionResult<ListDto>> UpdateList(string id, UpdateListDto listDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var command = new UpdateListCommand(id, listDto, userId);
        var list = await _mediator.Send(command);
        return Ok(list);
    }

    /// <summary>
    /// Elimina una lista
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteList(string id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var command = new DeleteListCommand(id, userId);
        var result = await _mediator.Send(command);
        if (!result)
            return NotFound();
        return NoContent();
    }
}