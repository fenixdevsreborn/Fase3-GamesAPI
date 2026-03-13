using System.ComponentModel.DataAnnotations;

namespace Fcg.Games.Contracts.Games;

public class GameListQuery
{
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;

    [MaxLength(100)]
    public string? Genre { get; set; }

    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    [MaxLength(200)]
    public string? Studio { get; set; }

    public bool? IsPublished { get; set; } = true;

    [MaxLength(50)]
    public string? SortBy { get; set; } = "CreatedAt";

    public bool SortDesc { get; set; } = true;
}
