using Domain.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;

namespace Infrastructure.Configuration;

/// <summary>
/// Configuración de mapeo para MongoDB
/// </summary>
public static class MongoDbConfiguration
{
    private static bool _isConfigured = false;

    public static void Configure()
    {
        if (_isConfigured) return;

        // Configurar convenciones globales
        var pack = new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new IgnoreExtraElementsConvention(true),
            new IgnoreIfNullConvention(true)
        };
        ConventionRegistry.Register("TaskManagerConventions", pack, t => true);

        // Configurar mapeo básico sin ID específico
        BsonClassMap.RegisterClassMap<User>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
        });

        BsonClassMap.RegisterClassMap<Board>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
        });

        BsonClassMap.RegisterClassMap<TaskItem>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
        });

        BsonClassMap.RegisterClassMap<Notification>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
        });

        // Configurar mapeo para ApplicationUser (ASP.NET Identity)
        BsonClassMap.RegisterClassMap<ApplicationUser>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
        });

        // Configurar mapeo para ApplicationRole (ASP.NET Identity)
        BsonClassMap.RegisterClassMap<ApplicationRole>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
        });

        _isConfigured = true;
    }
}
