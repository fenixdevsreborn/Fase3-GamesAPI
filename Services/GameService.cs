using ms_games.Events;
using ms_games.Publisher;

namespace ms_games.Services
{
  public class GameService
  {
    private readonly EventPublisher _publisher;

    public GameService(EventPublisher publisher)
    {
      _publisher = publisher;
    }

    public async Task RequestPurchase(string userId, string gameId, decimal amount)
    {
      var evt = new PurchaseRequestedEvent
      {
        UserId = userId,
        GameId = gameId,
        Amount = amount,
        RequestedAt = DateTime.UtcNow
      };

      var queueUrl = Environment.GetEnvironmentVariable("PAYMENT_QUEUE_URL");

      await _publisher.PublishAsync(queueUrl, evt);
    }
  }
}