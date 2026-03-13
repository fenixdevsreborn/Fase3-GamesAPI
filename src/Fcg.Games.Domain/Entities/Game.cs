namespace Fcg.Games.Domain.Entities;

public class Game
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Genre { get; set; }
    public string? Studio { get; set; }
    public decimal Price { get; set; }
    public string? CoverUrl { get; set; }
    /// <summary>Tags stored as JSON array in DB.</summary>
    public List<string> Tags { get; set; } = new();
    public bool IsPublished { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public bool IsDeleted => DeletedAt.HasValue;
}
