using System.ComponentModel.DataAnnotations;

namespace Fcg.Games.Contracts.Library;

public class AddToLibraryRequest
{
    public Guid GameId { get; set; }

    [Required]
    public string Status { get; set; } = "Wishlist"; // Owned, Wishlist, Favorite, Hidden, Archived

    [MaxLength(2000)]
    public string? Notes { get; set; }
}
