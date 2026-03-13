namespace Fcg.Games.Contracts.Library;

public class LibraryEntryResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? SourcePaymentId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public GameSummaryDto? Game { get; set; }
}

public class GameSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }
    public decimal Price { get; set; }
    public string? Genre { get; set; }
    public bool IsPublished { get; set; }
}
