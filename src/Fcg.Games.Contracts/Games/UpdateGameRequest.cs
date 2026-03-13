using System.ComponentModel.DataAnnotations;

namespace Fcg.Games.Contracts.Games;

public class UpdateGameRequest
{
    [MinLength(1), MaxLength(300)]
    public string? Title { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Genre { get; set; }

    [MaxLength(200)]
    public string? Studio { get; set; }

    [MaxLength(2000)]
    public string? CoverUrl { get; set; }

    public IReadOnlyList<string>? Tags { get; set; }

    public bool? IsPublished { get; set; }
    public bool? IsActive { get; set; }
}
