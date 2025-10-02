using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

// Lista dentro de un tablero (columna Kanban)
public class BoardList : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string BoardId { get; set; } = string.Empty;
    public int Position { get; set; }
    public string Color { get; set; } = "#e3f2fd"; // Color por defecto
}