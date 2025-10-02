using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace Application.Services;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
    Task SendPasswordChangedConfirmationAsync(string toEmail);
    Task SendInvitationAcceptedNotificationAsync(string toEmail, string accepterName, string boardTitle);
    Task SendInvitationRejectedNotificationAsync(string toEmail, string rejecterName, string boardTitle);
}

public class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings _settings;

    public SmtpEmailService(IOptions<SmtpSettings> settings)
    {
        _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
        
        // Validar configuración básica
        if (string.IsNullOrEmpty(_settings.Host))
            throw new ArgumentException("SMTP Host is required", nameof(settings));
        if (string.IsNullOrEmpty(_settings.Username))
            throw new ArgumentException("SMTP Username is required", nameof(settings));
        if (string.IsNullOrEmpty(_settings.Password))
            throw new ArgumentException("SMTP Password is required", nameof(settings));
        if (string.IsNullOrEmpty(_settings.FromEmail))
            throw new ArgumentException("SMTP FromEmail is required", nameof(settings));
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        var subject = "TaskManager - Recuperación de contraseña";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Recuperación de contraseña</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #f97316, #f59e0b); padding: 30px; border-radius: 10px; text-align: center; margin-bottom: 30px;'>
        <h1 style='color: white; margin: 0; font-size: 28px;'>TaskManager</h1>
        <p style='color: #fed7aa; margin: 10px 0 0 0;'>Recuperación de contraseña</p>
    </div>

    <div style='background: #f8fafc; padding: 30px; border-radius: 10px; border-left: 4px solid #f97316;'>
        <h2 style='color: #1f2937; margin-top: 0;'>¿Olvidaste tu contraseña?</h2>
        <p style='margin-bottom: 20px;'>No te preocupes, te ayudaremos a recuperarla.</p>
        <p>Haz clic en el botón de abajo para establecer una nueva contraseña:</p>

        <div style='text-align: center; margin: 30px 0;'>
            <a href='{resetLink}' style='background: linear-gradient(135deg, #f97316, #f59e0b); color: white; padding: 15px 30px; text-decoration: none; border-radius: 8px; font-weight: bold; display: inline-block; box-shadow: 0 4px 6px rgba(249, 115, 22, 0.2);'>Restablecer contraseña</a>
        </div>

        <p style='color: #6b7280; font-size: 14px; margin-top: 30px;'>
            Si no solicitaste este cambio, puedes ignorar este mensaje.
        </p>
        <p style='color: #6b7280; font-size: 14px;'>
            Este enlace expirará en 1 hora por seguridad.
        </p>
    </div>

    <div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e5e7eb; color: #6b7280; font-size: 12px;'>
        <p>TaskManager - Gestión de tareas eficiente</p>
        <p>Si tienes problemas con el botón, copia y pega esta URL en tu navegador:</p>
        <p style='word-break: break-all; color: #f97316;'>{resetLink}</p>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, body, true);
    }

    public async Task SendPasswordChangedConfirmationAsync(string toEmail)
    {
        var subject = "TaskManager - Contraseña cambiada exitosamente";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Contraseña cambiada</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #f97316, #f59e0b); padding: 30px; border-radius: 10px; text-align: center; margin-bottom: 30px;'>
        <h1 style='color: white; margin: 0; font-size: 28px;'>TaskManager</h1>
        <p style='color: #fed7aa; margin: 10px 0 0 0;'>Contraseña actualizada</p>
    </div>

    <div style='background: #f0fdf4; padding: 30px; border-radius: 10px; border-left: 4px solid #22c55e;'>
        <div style='text-align: center; margin-bottom: 20px;'>
            <div style='width: 60px; height: 60px; background: #22c55e; border-radius: 50%; margin: 0 auto 15px; display: flex; align-items: center; justify-content: center;'>
                <span style='color: white; font-size: 24px;'>✓</span>
            </div>
        </div>

        <h2 style='color: #15803d; margin-top: 0; text-align: center;'>¡Contraseña cambiada exitosamente!</h2>
        <p style='text-align: center; margin-bottom: 20px;'>Tu contraseña ha sido actualizada correctamente.</p>

        <div style='text-align: center; margin: 30px 0;'>
            <a href='http://localhost:4200/login' style='background: linear-gradient(135deg, #f97316, #f59e0b); color: white; padding: 12px 25px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block;'>Iniciar sesión</a>
        </div>

        <p style='color: #6b7280; font-size: 14px; text-align: center; margin-top: 30px;'>
            Si no realizaste este cambio, contacta inmediatamente con soporte.
        </p>
    </div>

    <div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e5e7eb; color: #6b7280; font-size: 12px;'>
        <p>TaskManager - Gestión de tareas eficiente</p>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, body, true);
    }

    public async Task SendInvitationAcceptedNotificationAsync(string toEmail, string accepterName, string boardTitle)
    {
        var subject = "TaskManager - " + accepterName + " aceptó tu invitación";
        var body = @"<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Invitación aceptada</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; border-radius: 10px; text-align: center; margin-bottom: 30px;'>
        <h1 style='color: white; margin: 0; font-size: 28px;'>TaskManager</h1>
        <p style='color: #e0e7ff; margin: 10px 0 0 0;'>¡Buenas noticias!</p>
    </div>

    <div style='background: #f8fafc; padding: 30px; border-radius: 10px; border-left: 4px solid #667eea;'>
        <h2 style='color: #1f2937; margin-top: 0; text-align: center;'>🎉 Invitación aceptada</h2>
        <p style='text-align: center; margin-bottom: 20px; font-size: 16px;'>
            <strong>" + accepterName + @"</strong> ha aceptado tu invitación para colaborar en el tablero <strong>""" + boardTitle + @"""</strong>.
        </p>

        <div style='background: #ffffff; padding: 20px; border-radius: 8px; border: 1px solid #e5e7eb; margin: 20px 0;'>
            <p style='margin: 0; text-align: center; color: #374151;'>
                Ahora pueden trabajar juntos en las tareas del tablero.
            </p>
        </div>

        <div style='text-align: center; margin: 30px 0;'>
            <a href='http://localhost:4200/boards' style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 12px 25px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block;'>Ver tablero</a>
        </div>

        <p style='color: #6b7280; font-size: 14px; text-align: center; margin-top: 30px;'>
            ¡Felicitaciones por expandir tu equipo de trabajo!
        </p>
    </div>

    <div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e5e7eb; color: #6b7280; font-size: 12px;'>
        <p>TaskManager - Gestión de tareas colaborativa</p>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, body, true);
    }

    public async Task SendInvitationRejectedNotificationAsync(string toEmail, string rejecterName, string boardTitle)
    {
        var subject = "TaskManager - " + rejecterName + " rechazó tu invitación";
        var body = @"<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Invitación rechazada</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(to right, #4a5568 0%, #2563eb 100%); padding: 30px; border-radius: 10px; text-align: center; margin-bottom: 30px;'>
        <h1 style='color: white; margin: 0; font-size: 28px;'>TaskManager</h1>
        <p style='color: #dbeafe; margin: 10px 0 0 0;'>Actualización de invitación</p>
    </div>

    <div style='background: #fef2f2; padding: 30px; border-radius: 10px; border-left: 4px solid #ef4444;'>
        <h2 style='color: #991b1b; margin-top: 0; text-align: center;'>Invitación rechazada</h2>
        <p style='text-align: center; margin-bottom: 20px; font-size: 16px;'>
            <strong>" + rejecterName + @"</strong> ha rechazado tu invitación para colaborar en el tablero <strong>""" + boardTitle + @"""</strong>.
        </p>

        <div style='background: #ffffff; padding: 20px; border-radius: 8px; border: 1px solid #fee2e2; margin: 20px 0;'>
            <p style='margin: 0; text-align: center; color: #374151;'>
                En este momento la persona no puede unirse a tu tablero.
            </p>
        </div>

        <div style='text-align: center; margin: 30px 0;'>
            <a href='http://localhost:4200/boards' style='background: linear-gradient(to right, #4a5568 0%, #2563eb 100%); color: white; padding: 12px 25px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block;'>Ver mis tableros</a>
        </div>

        <p style='color: #6b7280; font-size: 14px; text-align: center; margin-top: 30px;'>
            Puedes invitar a otras personas o intentar nuevamente más tarde.
        </p>
    </div>

    <div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e5e7eb; color: #6b7280; font-size: 12px;'>
        <p>TaskManager - Gestión de tareas colaborativa</p>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, body, true);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
    {
        try
        {
            using var smtpClient = new SmtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = _settings.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);

            Console.WriteLine($"[EMAIL] Sent successfully to {toEmail}: {subject}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EMAIL ERROR] Failed to send email to {toEmail}: {ex.Message}");
            throw;
        }
    }
}

public class ConsoleEmailService : IEmailService
{
    public Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        Console.WriteLine($"[EMAIL-DEV] Password reset email would be sent to {toEmail}");
        Console.WriteLine($"[EMAIL-DEV] Reset link: {resetLink}");
        return Task.CompletedTask;
    }

    public Task SendPasswordChangedConfirmationAsync(string toEmail)
    {
        Console.WriteLine($"[EMAIL-DEV] Password changed confirmation would be sent to {toEmail}");
        return Task.CompletedTask;
    }

    public Task SendInvitationAcceptedNotificationAsync(string toEmail, string accepterName, string boardTitle)
    {
        Console.WriteLine($"[EMAIL-DEV] Invitation accepted notification would be sent to {toEmail}");
        Console.WriteLine($"[EMAIL-DEV] {accepterName} accepted invitation for board '{boardTitle}'");
        return Task.CompletedTask;
    }

    public Task SendInvitationRejectedNotificationAsync(string toEmail, string rejecterName, string boardTitle)
    {
        Console.WriteLine($"[EMAIL-DEV] Invitation rejected notification would be sent to {toEmail}");
        Console.WriteLine($"[EMAIL-DEV] {rejecterName} rejected invitation for board '{boardTitle}'");
        return Task.CompletedTask;
    }
}

public class SmtpSettings
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public bool EnableSsl { get; set; } = true;
    public string FromEmail { get; set; } = "";
    public string FromName { get; set; } = "TaskManager";
}
