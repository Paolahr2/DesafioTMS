using AspNetCore.Identity.MongoDbCore.Models;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Configuration;

/// <summary>
/// Contexto de base de datos para ASP.NET Identity con MongoDB
/// Nota: Usando configuración directa del paquete AspNetCore.Identity.MongoDbCore
/// </summary>
public class ApplicationDbContext
{
    private readonly IMongoDatabase _database;

    public ApplicationDbContext(IMongoClient mongoClient, IConfiguration configuration)
    {
        var databaseName = configuration.GetSection("DatabaseSettings")["DatabaseName"] ?? "tasksmanagerbd";
        _database = mongoClient.GetDatabase(databaseName);
    }

    public IMongoDatabase Database => _database;
}