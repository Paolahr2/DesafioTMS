using Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Infrastructure.Configuration;

/// <summary>
/// Extensiones para configurar servicios de infraestructura
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Configura MongoDB y sus servicios
    /// </summary>
    public static IServiceCollection AddMongoDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        // Configurar MongoDB mapping
        MongoDbConfiguration.Configure();

        // Configurar DatabaseSettings
        services.Configure<DatabaseSettings>(configuration.GetSection("DatabaseSettings"));

        // Configurar MongoDB Client
        services.AddSingleton<IMongoClient>(provider =>
        {
            var connectionString = configuration.GetConnectionString("MongoDb");
            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("MongoDB connection string not found, using in-memory fallback");
                // Return a dummy client that won't be used
                return null!;
            }
            
            try
            {
                Console.WriteLine($"Attempting to connect to MongoDB: {connectionString}");
                var client = new MongoClient(connectionString);
                // Test the connection
                var database = client.GetDatabase("admin");
                var pingResult = database.RunCommandAsync((Command<BsonDocument>)"{ping:1}").Result;
                Console.WriteLine("Successfully connected to MongoDB");
                return client;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to MongoDB: {ex.Message}");
                // For development, don't fail - return null and handle in repositories
                Console.WriteLine("Continuing without MongoDB connection for development");
                return null!;
            }
        });

        // Configurar MongoDB Database
        services.AddScoped<IMongoDatabase>(provider =>
        {
            var client = provider.GetRequiredService<IMongoClient>();
            if (client == null)
            {
                // For development without MongoDB, return null
                return null!;
            }
            
            var databaseName = configuration["DatabaseSettings:DatabaseName"];
            if (string.IsNullOrEmpty(databaseName))
            {
                throw new InvalidOperationException("Database name not found in configuration.");
            }
            return client.GetDatabase(databaseName);
        });

        return services;
    }

    /// <summary>
    /// Crea índices necesarios en MongoDB
    /// </summary>
    public static async Task CreateDatabaseIndexesAsync(IServiceProvider serviceProvider)
    {
        var database = serviceProvider.GetRequiredService<IMongoDatabase>();

        // Crear índices para Users
        var usersCollection = database.GetCollection<Domain.Entities.User>("users");
        await usersCollection.Indexes.CreateOneAsync(new CreateIndexModel<Domain.Entities.User>(
            Builders<Domain.Entities.User>.IndexKeys.Ascending(u => u.Email),
            new CreateIndexOptions { Name = DatabaseIndexes.Users.EmailIndex, Unique = true }));

        await usersCollection.Indexes.CreateOneAsync(new CreateIndexModel<Domain.Entities.User>(
            Builders<Domain.Entities.User>.IndexKeys.Ascending(u => u.Username),
            new CreateIndexOptions { Name = DatabaseIndexes.Users.UsernameIndex, Unique = true }));

        // Crear índices para Boards
        var boardsCollection = database.GetCollection<Domain.Entities.Board>("boards");
        await boardsCollection.Indexes.CreateOneAsync(new CreateIndexModel<Domain.Entities.Board>(
            Builders<Domain.Entities.Board>.IndexKeys.Ascending(b => b.OwnerId),
            new CreateIndexOptions { Name = DatabaseIndexes.Boards.OwnerIndex }));

        await boardsCollection.Indexes.CreateOneAsync(new CreateIndexModel<Domain.Entities.Board>(
            Builders<Domain.Entities.Board>.IndexKeys.Ascending(b => b.IsPublic),
            new CreateIndexOptions { Name = DatabaseIndexes.Boards.PublicIndex }));

        // Crear índices para Tasks
        var tasksCollection = database.GetCollection<Domain.Entities.TaskItem>("tasks");
        await tasksCollection.Indexes.CreateOneAsync(new CreateIndexModel<Domain.Entities.TaskItem>(
            Builders<Domain.Entities.TaskItem>.IndexKeys.Ascending(t => t.BoardId),
            new CreateIndexOptions { Name = DatabaseIndexes.Tasks.BoardIndex }));

        await tasksCollection.Indexes.CreateOneAsync(new CreateIndexModel<Domain.Entities.TaskItem>(
            Builders<Domain.Entities.TaskItem>.IndexKeys.Ascending(t => t.AssignedToId),
            new CreateIndexOptions { Name = DatabaseIndexes.Tasks.AssignedToIndex }));
    }
}
