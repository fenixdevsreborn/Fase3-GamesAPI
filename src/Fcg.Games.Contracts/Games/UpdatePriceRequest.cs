using System.ComponentModel.DataAnnotations;

namespace Fcg.Games.Contracts.Games;

public class UpdatePriceRequest
{
    [Required, Range(0, double.MaxValue)]
    public decimal Price { get; set; }
}
