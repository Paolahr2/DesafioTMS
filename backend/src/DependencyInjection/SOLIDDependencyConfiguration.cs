using Application.Commands.Auth;
using Application.Factories;
using Application.Services.Token;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Users;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Base;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace DependencyInjection;

/// <summary>
/// DIP: Configuración que invierte todas las dependencias
/// Los módulos de alto nivel no dependen de módulos de bajo nivel
/// Ambos dependen de abstracciones
/// </summary>
public static class SOLIDDependencyConfiguration
{
    /// <summary>
    /// DIP: Registra todas las abstracciones y sus implementaciones
    /// Permite cambiar implementaciones sin modificar código cliente
    /// </summary>
    public static IServiceCollection AddSOLIDCompliantServices(this IServiceCollection services, IConfiguration configuration)
    {
        // DIP: Registrar la misma instancia para ambas interfaces del factory
        services.AddSingleton<DependencyInvertedFactory>();
        services.AddSingleton<IRepositoryFactory>(provider => provider.GetRequiredService<DependencyInvertedFactory>());
        services.AddSingleton<IApplicationServiceFactory>(provider => provider.GetRequiredService<DependencyInvertedFactory>());

        // OCP + DIP: Token service extensible e invertido
        services.AddScoped<ITokenService, JwtTokenService>();

        // LSP + DIP: Repositorios que respetan contratos y son intercambiables
        services.AddScoped<IBaseRepository<Domain.Entities.User>, LSPCompliantUserRepository>();
        services.AddScoped<Domain.Interfaces.Repository.IWriteRepository<Domain.Entities.User>, LSPCompliantUserRepository>();
        
        // ISP: Interfaces segregadas registradas independientemente
        services.AddScoped<IUserEmailQueries>(provider => 
            provider.GetRequiredService<LSPCompliantUserRepository>());
        services.AddScoped<IUserUsernameQueries>(provider => 
            provider.GetRequiredService<LSPCompliantUserRepository>());
        services.AddScoped<IUserSearchQueries>(provider => 
            provider.GetRequiredService<LSPCompliantUserRepository>());

        // DIP: Application layer handlers
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(typeof(Application.Commands.Auth.LoginCommand).Assembly);
        });

        return services;
    }

    /// <summary>
    /// DIP: Factory method pattern para crear configuraciones específicas
    /// </summary>
    public static IServiceCollection AddRepositoryConfiguration<TEntity, TRepository>(
        this IServiceCollection services)
        where TEntity : Domain.Entities.BaseEntity
        where TRepository : class, IBaseRepository<TEntity>, Domain.Interfaces.Repository.IWriteRepository<TEntity>
    {
        services.AddScoped<IBaseRepository<TEntity>, TRepository>();
        services.AddScoped<Domain.Interfaces.Repository.IWriteRepository<TEntity>, TRepository>();
        
        return services;
    }

    /// <summary>
    /// OCP: Método extensible para agregar nuevos tipos de servicios sin modificar código existente
    /// </summary>
    public static IServiceCollection AddExtensibleService<TInterface, TImplementation>(
        this IServiceCollection services, 
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.Add(new ServiceDescriptor(typeof(TInterface), typeof(TImplementation), lifetime));
        return services;
    }
}
