namespace Fcg.Games.Contracts.Payments;

/// <summary>Contract for calling Payments API to create a purchase intent.</summary>
public record PurchaseIntentRequest(
    Guid UserId,
    Guid GameId,
    decimal Amount,
    string Currency = "BRL"
);

/// <summary>Expected response from Payments API (or stub).</summary>
public record PurchaseIntentResponse(
    string PaymentId,
    string Status, // e.g. Pending, Completed, Failed
    string? CheckoutUrl = null
);
