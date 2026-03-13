using Fcg.Games.Contracts.Payments;

namespace Fcg.Games.Application.Services;

public interface IPurchaseService
{
    /// <summary>Creates a purchase intent via Payments API and returns checkout info. UserId comes from JWT.</summary>
    Task<PurchaseIntentResponse> PurchaseAsync(Guid userId, Guid gameId, CancellationToken cancellationToken = default);
}
