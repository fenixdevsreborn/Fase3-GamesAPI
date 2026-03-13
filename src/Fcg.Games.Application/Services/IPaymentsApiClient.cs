using Fcg.Games.Contracts.Payments;

namespace Fcg.Games.Application.Services;

/// <summary>Client for Payments API. Games API calls this to create purchase intents.</summary>
public interface IPaymentsApiClient
{
    Task<PurchaseIntentResponse> CreatePurchaseIntentAsync(PurchaseIntentRequest request, CancellationToken cancellationToken = default);
}
