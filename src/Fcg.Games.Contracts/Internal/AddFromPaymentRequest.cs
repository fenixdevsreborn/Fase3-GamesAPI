namespace Fcg.Games.Contracts.Internal;

/// <summary>Request for internal endpoint: add game to user library after payment confirmed.</summary>
public class AddFromPaymentRequest
{
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
}
