using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class ListItemDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Text { get; set; } = string.Empty;
    public bool Completed { get; set; } = false;
    public string? Notes { get; set; }
}

public class ListDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string BoardId { get; set; } = string.Empty;
    public int Order { get; set; }
    public List<ListItemDto> Items { get; set; } = new();
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateListDto
{
    [Required]
    public string Title { get; set; } = string.Empty;
    [Required]
    public string BoardId { get; set; } = string.Empty;
    public int Order { get; set; } = 0;
    public List<ListItemDto>? Items { get; set; }
    public string? Notes { get; set; }
}

public class UpdateListDto
{
    [Required]
    public string Title { get; set; } = string.Empty;
    public int? Order { get; set; }
    public List<ListItemDto>? Items { get; set; }
}

public class ReorderListDto
{
    [Required]
    public string Id { get; set; } = string.Empty;
    public int Order { get; set; }
}