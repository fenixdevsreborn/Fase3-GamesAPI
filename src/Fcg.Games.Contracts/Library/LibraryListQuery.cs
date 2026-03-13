using System.ComponentModel.DataAnnotations;

namespace Fcg.Games.Contracts.Library;

public class LibraryListQuery
{
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;

    [MaxLength(50)]
    public string? Status { get; set; }
}
