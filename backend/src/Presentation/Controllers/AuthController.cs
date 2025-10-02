using Application.Commands.Auth;
using Application.DTOs.Auth;
using Application.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using System.Linq;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Identity;
using Domain.Entities;
using Domain.Interfaces;
using BCrypt.Net;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IEmailService _emailService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly Domain.Interfaces.UserRepository _userRepository;

    // PoC: almacenamiento en memoria de tokens de reset (token -> (email, expiry))
    private static readonly ConcurrentDictionary<string, (string Email, DateTime ExpiresAt)> _passwordResetTokens = new();

    public AuthController(
        IMediator mediator,
        IEmailService emailService,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtService jwtService,
        Domain.Interfaces.UserRepository userRepository)
    {
        _mediator = mediator;
        _emailService = emailService;
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _userRepository = userRepository;
    }

    [HttpGet("debug/users")]
    public async Task<ActionResult> GetDebugUsers()
    {
        try
        {
            // Obtener usuarios de ASP.NET Identity
            var identityUsers = _userManager.Users.ToList();
            
            // Obtener usuarios del sistema legacy
            var legacyUsers = await _userRepository.GetAllAsync();
            
            return Ok(new
            {
                identityUsers = identityUsers.Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email,
                    u.FirstName,
                    u.LastName,
                    u.IsActive,
                    u.EmailConfirmed
                }),
                legacyUsers = legacyUsers.Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.FirstName,
                    u.LastName,
                    u.IsActive
                })
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            // Primero intentar con ASP.NET Identity
            var user = await _userManager.FindByEmailAsync(request.EmailOrUsername) ??
                      await _userManager.FindByNameAsync(request.EmailOrUsername);

            if (user != null && user.IsActive)
            {
                Console.WriteLine($"[DEBUG] Found user in Identity: {user.UserName}, Email: {user.Email}, IsActive: {user.IsActive}, EmailConfirmed: {user.EmailConfirmed}");

                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
                Console.WriteLine($"[DEBUG] Identity password check result: Succeeded={result.Succeeded}, IsLockedOut={result.IsLockedOut}, IsNotAllowed={result.IsNotAllowed}");

                if (result.Succeeded)
                {
                    // Login exitoso con Identity
                    await _userManager.UpdateAsync(user);
                    var roles = await _userManager.GetRolesAsync(user);
                    var roleString = roles.FirstOrDefault() ?? "User";
                    var userRole = roleString switch
                    {
                        "Admin" => Domain.Enums.UserRole.Admin,
                        "ProjectManager" => Domain.Enums.UserRole.ProjectManager,
                        _ => Domain.Enums.UserRole.User
                    };
                    var token = _jwtService.GenerateToken(user.Id, user.Email!, user.UserName!, roleString);

                    var response = new AuthResponseDto
                    {
                        Success = true,
                        Message = "Login exitoso",
                        Token = token,
                        User = new Application.DTOs.UserDto
                        {
                            Id = user.Id,
                            Username = user.UserName ?? string.Empty,
                            Email = user.Email ?? string.Empty,
                            FirstName = user.FirstName ?? string.Empty,
                            LastName = user.LastName ?? string.Empty,
                            Role = userRole,
                            IsActive = user.IsActive,
                            Avatar = user.Avatar,
                            LastLoginAt = user.LastLoginAt
                        }
                    };

                    return Ok(response);
                }
            }

            // Si no funcionó con Identity, intentar con el sistema antiguo (UserRepository)
            Console.WriteLine($"[DEBUG] User not found in Identity or password failed, trying legacy system");
            var legacyUser = await _userRepository.GetByEmailAsync(request.EmailOrUsername) ??
                            await _userRepository.GetByUsernameAsync(request.EmailOrUsername);

            if (legacyUser != null)
            {
                Console.WriteLine($"[DEBUG] Found user in legacy system: {legacyUser.Username}");

                // Verificar contraseña con BCrypt
                var valid = BCrypt.Net.BCrypt.Verify(request.Password, legacyUser.PasswordHash);
                Console.WriteLine($"[DEBUG] Legacy password check: {valid}");

                if (valid)
                {
                    // Login exitoso con sistema legacy
                    // Actualizar último login
                    legacyUser.LastLoginAt = DateTime.UtcNow;
                    await _userRepository.UpdateAsync(legacyUser);

                    // Generar token
                    var token = _jwtService.GenerateToken(legacyUser.Id, legacyUser.Email, legacyUser.Username, "User");

                    var response = new AuthResponseDto
                    {
                        Success = true,
                        Message = "Login exitoso",
                        Token = token,
                        User = new Application.DTOs.UserDto
                        {
                            Id = legacyUser.Id,
                            Username = legacyUser.Username,
                            Email = legacyUser.Email,
                            FirstName = legacyUser.FirstName,
                            LastName = legacyUser.LastName,
                            Role = legacyUser.Role,
                            IsActive = legacyUser.IsActive,
                            Avatar = legacyUser.Avatar,
                            LastLoginAt = legacyUser.LastLoginAt
                        }
                    };

                    return Ok(response);
                }
            }

            return Unauthorized(new { message = "Credenciales inválidas" });
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors.Select(e => e.ErrorMessage).ToArray();
            return BadRequest(new { message = "Errores de validación", errors });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Login failed: {ex.Message}");
            Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            // Verificar si el usuario ya existe
            var existingUser = await _userManager.FindByEmailAsync(request.Email) ??
                              await _userManager.FindByNameAsync(request.Username);

            if (existingUser != null)
            {
                return BadRequest(new { message = "El usuario ya existe" });
            }

            // Crear nuevo usuario
            var user = new ApplicationUser(request.Username, request.Email, request.FirstName, request.LastName)
            {
                Role = Domain.Enums.UserRole.User,
                IsActive = true,
                EmailConfirmed = false // En producción, requerir confirmación de email
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                return BadRequest(new { message = "Errores de validación", errors });
            }

            // Asignar rol por defecto
            await _userManager.AddToRoleAsync(user, "User");

            // Generar token JWT para login automático
            var token = await _mediator.Send(new LoginCommand(request.Email, request.Password));

            return Ok(token);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors.Select(e => e.ErrorMessage).ToArray();
            return BadRequest(new { message = "Errores de validación", errors });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        // No revelar si el email existe: siempre responder OK
        try
        {
            // Generar token y guardarlo en memoria con expiración (1 hora)
            var token = Guid.NewGuid().ToString("N");
            var expires = DateTime.UtcNow.AddHours(1);
            _passwordResetTokens[token] = (request.Email.ToLowerInvariant(), expires);

            // Generar enlace de reset
            var resetLink = $"http://localhost:4200/reset-password?token={token}&email={Uri.EscapeDataString(request.Email)}";

            // Enviar email con el enlace de recuperación
            try
            {
                await _emailService.SendPasswordResetEmailAsync(request.Email, resetLink);
                Console.WriteLine($"[EMAIL] Password reset email sent to {request.Email}");
            }
            catch (Exception emailEx)
            {
                Console.WriteLine($"[EMAIL ERROR] Failed to send password reset email to {request.Email}: {emailEx.Message}");
                // No fallar la petición si el email falla, solo loggear
            }

            return Ok(new { message = "Si existe una cuenta asociada al correo, se ha enviado un enlace de recuperación." });
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERROR] ForgotPassword: " + ex.Message);
            return BadRequest(new { message = "No se pudo procesar la solicitud." });
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.NewPassword))
                return BadRequest(new { message = "Token, email y nueva contraseña son requeridos." });

            if (!_passwordResetTokens.TryGetValue(request.Token, out var entry))
                return BadRequest(new { message = "Token inválido o expirado." });

            if (!string.Equals(entry.Email, request.Email, StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "Token inválido para el correo proporcionado." });

            if (entry.ExpiresAt < DateTime.UtcNow)
            {
                _passwordResetTokens.TryRemove(request.Token, out _);
                return BadRequest(new { message = "Token expirado." });
            }

            // Aquí delegamos al mediator un comando para cambiar la contraseña (reutilizar login/register patterns)
            var cmd = new ResetPasswordCommand(request.Email, request.NewPassword, request.Token);
            var result = await _mediator.Send(cmd);

            // Consumir token
            _passwordResetTokens.TryRemove(request.Token, out _);

            // Enviar email de confirmación
            try
            {
                await _emailService.SendPasswordChangedConfirmationAsync(request.Email);
                Console.WriteLine($"[EMAIL] Password changed confirmation sent to {request.Email}");
            }
            catch (Exception emailEx)
            {
                Console.WriteLine($"[EMAIL ERROR] Failed to send confirmation email to {request.Email}: {emailEx.Message}");
                // No fallar la petición si el email falla
            }

            return Ok(result);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors.Select(e => e.ErrorMessage).ToArray();
            return BadRequest(new { message = "Errores de validación", errors });
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERROR] ResetPassword: " + ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("create-admin")]
    public async Task<ActionResult<AuthResponseDto>> CreateAdmin([FromBody] RegisterRequestDto request)
    {
        try
        {
            Console.WriteLine($"[DEBUG] Create admin request received. Username={request.Username}, Email={request.Email}");
            var command = new RegisterCommand(
                request.Username,
                request.Email,
                request.Password,
                $"{request.FirstName} {request.LastName}".Trim(),
                Domain.Enums.UserRole.Admin
            );
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors.Select(e => e.ErrorMessage).ToArray();
            Console.WriteLine("[DEBUG] ValidationException during admin creation: " + string.Join("; ", errors));
            return BadRequest(new { message = "Errores de validación", errors });
        }
        catch (Exception ex)
        {
            Console.WriteLine("[DEBUG] Exception during admin creation: " + ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }
}
