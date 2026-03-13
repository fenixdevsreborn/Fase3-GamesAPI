namespace Fcg.Games.Contracts.Payments;

/// <summary>DTO returned from Payments API POST /payments. Used to build PurchaseIntentResponse.</summary>
public class PaymentResponseDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? FailureReason { get; set; }
}
