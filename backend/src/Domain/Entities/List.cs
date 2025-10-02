using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

// Item del checklist dentro de una lista
public class ListItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Text { get; set; } = string.Empty;
    public bool Completed { get; set; } = false;
    public string? Notes { get; set; }
}

// Lista dentro de un tablero (equivalente a columna en Trello)
public class List : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string BoardId { get; set; } = string.Empty;
    public int Order { get; set; } = 0; // Para ordenar las listas horizontalmente
    public List<ListItem> Items { get; set; } = new(); // Checklist items
    public string? Notes { get; set; } // Notas de la lista
}