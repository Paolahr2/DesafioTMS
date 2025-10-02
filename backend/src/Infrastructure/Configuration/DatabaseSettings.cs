namespace Infrastructure.Configuration;

/// <summary>
/// Configuración de la base de datos
/// </summary>
public class DatabaseSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    
    // Nombres de colecciones
    public string UsersCollectionName { get; set; } = "users";
    public string BoardsCollectionName { get; set; } = "boards";
    public string TasksCollectionName { get; set; } = "tasks";
    public string NotificationsCollectionName { get; set; } = "notifications";
}

/// <summary>
/// Configuración de índices para MongoDB
/// </summary>
public static class DatabaseIndexes
{
    /// <summary>
    /// Nombres de índices para optimizar consultas
    /// </summary>
    public static class Users
    {
        public const string EmailIndex = "email_unique";
        public const string UsernameIndex = "username_unique";
    }

    public static class Boards
    {
        public const string OwnerIndex = "owner_id";
        public const string PublicIndex = "is_public";
    }

    public static class Tasks
    {
        public const string BoardIndex = "board_id";
        public const string AssignedToIndex = "assigned_to";
        public const string StatusIndex = "status";
        public const string DueDateIndex = "due_date";
    }

    public static class Notifications
    {
        public const string UserIndex = "user_id";
        public const string ReadIndex = "is_read";
        public const string CreatedAtIndex = "created_at";
    }
}
