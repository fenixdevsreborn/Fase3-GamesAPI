namespace ms_games.Events
{
  public class PurchaseProcessedEvent
  {
    public string EventType => "PURCHASE_PROCESSED";

    public string UserId { get; set; }

    public string GameId { get; set; }

    public decimal Amount { get; set; }

    public bool Success { get; set; }

    public string? Reason { get; set; }

    public DateTime ProcessedAt { get; set; }
  }
}
