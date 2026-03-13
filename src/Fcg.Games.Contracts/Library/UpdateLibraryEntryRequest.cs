using System.ComponentModel.DataAnnotations;

namespace Fcg.Games.Contracts.Library;

public class UpdateLibraryEntryRequest
{
    [MaxLength(50)]
    public string? Status { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}
