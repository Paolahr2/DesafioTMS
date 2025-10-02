using Application.Services.Token;
using Domain.Interfaces.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Factories;

/// <summary>
/// Factory específico para repositorios - invierte dependencias
/// Los handlers no dependen de implementaciones concretas
/// </summary>
public interface IRepositoryFactory
{
    TRepository GetRepository<TRepository>() where TRepository : class;
    IBaseRepository<TEntity> GetBaseRepository<TEntity>() where TEntity : Domain.Entities.BaseEntity;
}

/// <summary>
/// Factory para servicios de aplicación
/// </summary>
public interface IApplicationServiceFactory
{
    ITokenService GetTokenService();
    TService GetService<TService>() where TService : class;
}

/// <summary>
/// DIP: Implementación que usa DI container para resolver dependencias
/// </summary>
public class DependencyInvertedFactory : IRepositoryFactory, IApplicationServiceFactory
{
    private readonly IServiceProvider _serviceProvider;

    public DependencyInvertedFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public TRepository GetRepository<TRepository>() where TRepository : class
    {
        return _serviceProvider.GetRequiredService<TRepository>();
    }

    public IBaseRepository<TEntity> GetBaseRepository<TEntity>() where TEntity : Domain.Entities.BaseEntity
    {
        return _serviceProvider.GetRequiredService<IBaseRepository<TEntity>>();
    }

    public ITokenService GetTokenService()
    {
        return _serviceProvider.GetRequiredService<ITokenService>();
    }

    public TService GetService<TService>() where TService : class
    {
        return _serviceProvider.GetRequiredService<TService>();
    }
}
