namespace Fcg.Games.Contracts.Payments;

/// <summary>DTO sent to Payments API POST /payments. Matches Payments.Contracts CreatePaymentRequest shape.</summary>
public class CreatePaymentRequestDto
{
    public Guid GameId { get; set; }
    public string Currency { get; set; } = "BRL";
}
