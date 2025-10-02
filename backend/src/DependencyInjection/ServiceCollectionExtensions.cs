using Application.Behaviors;
using Application.Services;
using Domain.Interfaces;
using FluentValidation;
using Infrastructure.Repositories;
using Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MediatR;

namespace DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra todos los servicios de la aplicación
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configurar MongoDB usando Infrastructure
        services.AddMongoDatabase(configuration);

        // Agregar MediatR básico
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(typeof(Application.Commands.Auth.RegisterCommand).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(Application.Handlers.Auth.RegisterCommandHandler).Assembly);
        });

        // Registrar validadores de FluentValidation desde el assembly de Application
        services.AddValidatorsFromAssembly(typeof(Application.Commands.Auth.RegisterCommand).Assembly);

        // Registrar el pipeline de validación para que MediatR ejecute los validadores antes de los handlers
        services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(Application.Behaviors.ValidationBehavior<,>));

        // Configurar servicios SOLID (método local)
        ConfigureSOLIDServices(services, configuration);

        // Registrar repositorios
        services.AddRepositories();

        return services;
    }

    private static void ConfigureSOLIDServices(IServiceCollection services, IConfiguration configuration)
    {
        // Token service
        services.AddScoped<IJwtService, JwtService>();
        
        // Repositorios básicos
        services.AddScoped<Domain.Interfaces.UserRepository, Infrastructure.Repositories.UserRepository>();
        services.AddScoped<Domain.Interfaces.BoardRepository, Infrastructure.Repositories.BoardRepository>();
        services.AddScoped<Domain.Interfaces.TaskRepository, Infrastructure.Repositories.TaskRepository>();
        services.AddScoped<Domain.Interfaces.BoardInvitationRepository, Infrastructure.Repositories.BoardInvitationRepository>();
        services.AddScoped<Domain.Interfaces.NotificationRepository, Infrastructure.Repositories.NotificationRepository>();

        // Email service - se puede forzar el uso de ConsoleEmailService mediante la clave UseConsoleEmail (appsettings o user-secrets)
        var useConsole = configuration.GetValue<bool>("UseConsoleEmail", false);

        var smtpSection = configuration.GetSection("Smtp");
        var smtpUsername = smtpSection.GetValue<string>("Username");
        var smtpPassword = smtpSection.GetValue<string>("Password");

        if (useConsole)
        {
            // Forzar servicio de consola (útil en desarrollo y pruebas locales)
            services.AddScoped<Application.Services.IEmailService, Application.Services.ConsoleEmailService>();
        }
        else if (!string.IsNullOrEmpty(smtpUsername) && !string.IsNullOrEmpty(smtpPassword))
        {
            // Registrar configuracion y servicio SMTP si existen credenciales
            services.Configure<Application.Services.SmtpSettings>(smtpSection);
            services.AddScoped<Application.Services.IEmailService, Application.Services.SmtpEmailService>();
        }
        else
        {
            // Fallback a ConsoleEmailService para desarrollo local sin credenciales
            services.AddScoped<Application.Services.IEmailService, Application.Services.ConsoleEmailService>();
        }
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<Domain.Interfaces.UserRepository, Infrastructure.Repositories.UserRepository>();
        services.AddScoped<Domain.Interfaces.BoardRepository, Infrastructure.Repositories.BoardRepository>();
        services.AddScoped<Domain.Interfaces.TaskRepository, Infrastructure.Repositories.TaskRepository>();
        services.AddScoped<Domain.Interfaces.ListRepository, Infrastructure.Repositories.ListRepository>();
        services.AddScoped<Domain.Interfaces.BoardInvitationRepository, Infrastructure.Repositories.BoardInvitationRepository>();

        return services;
    }

    /// <summary>
    /// Registra los servicios de dominio y aplicación
    /// </summary>
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IJwtService, JwtService>();

        return services;
    }
}
