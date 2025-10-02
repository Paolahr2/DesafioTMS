using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    /// <summary>
    /// Endpoint público para probar que la API funciona
    /// </summary>
    [HttpGet("public")]
    public ActionResult<string> Public()
    {
        return Ok("Este endpoint es público - no requiere autenticación");
    }

    /// <summary>
    /// Endpoint protegido para probar autenticación JWT
    /// </summary>
    [HttpGet("protected")]
    [Authorize]
    public ActionResult<object> Protected()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var username = User.FindFirst(ClaimTypes.Name)?.Value;

        return Ok(new
        {
            Message = "¡Autenticación exitosa! Token JWT válido.",
            UserId = userId,
            Email = email,
            Username = username,
            Claims = User.Claims.Select(c => new { c.Type, c.Value })
        });
    }
}
