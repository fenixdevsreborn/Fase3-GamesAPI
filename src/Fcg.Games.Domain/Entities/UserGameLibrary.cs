using Fcg.Games.Domain.Enums;

namespace Fcg.Games.Domain.Entities;

public class UserGameLibrary
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public LibraryEntryStatus Status { get; set; }
    public string? SourcePaymentId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Game? Game { get; set; }
}
