using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

// Tablero Kanban que contiene tareas organizadas por columnas
public class Board : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = "#3498db"; // Color por defecto
    public string OwnerId { get; set; } = string.Empty;
    
    // Lista de IDs de usuarios que tienen acceso al tablero
    public List<string> MemberIds { get; set; } = new();
    
    public bool IsPublic { get; set; } = false;
    public bool IsArchived { get; set; } = false;
    
    // Configuración de columnas personalizada para el tablero
    public List<string> Columns { get; set; } = new() { "Pendiente", "En Proceso", "Completado" };
}
