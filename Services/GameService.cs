using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using ms_games.Events;
using ms_games.Models;
using ms_games.Publisher;

namespace ms_games.Services
{
  public class GameService
  {
    private readonly DynamoDBContext _context;
    private readonly EventPublisher _publisher;
    private readonly string _table;

    public GameService(IAmazonDynamoDB dynamo, EventPublisher publisher)
    {
      _context = new DynamoDBContext(dynamo);
      _publisher = publisher;
      _table = Environment.GetEnvironmentVariable("GAMES_TABLE");
    }

    public async Task<List<Game>> GetAll()
    {
      return await _context.ScanAsync<Game>(new List<ScanCondition>()).GetRemainingAsync();
    }

    public async Task<Game> GetById(string id)
    {
      return await _context.LoadAsync<Game>(id);
    }

    public async Task<List<Game>> GetRecommendation(string gameId, int limit = 5)
    {
      var baseGame = await _context.LoadAsync<Game>(gameId);

      if (baseGame == null)
        throw new Exception("Game not found");

      var conditions = new List<ScanCondition>
      {
        new ScanCondition("Category", ScanOperator.Equal, baseGame.Category)
      };

      var games = await _context
        .ScanAsync<Game>(conditions)
        .GetRemainingAsync();

      var recommendations = games
        .Where(g => g.Id != gameId)
        .OrderBy(g => Math.Abs(g.Price - baseGame.Price))
        .ThenBy(g => g.Name)
        .Take(limit)
        .ToList();

      return recommendations;
    }

    public async Task Create(Game game)
    {
      game.Id = Guid.NewGuid().ToString();
      await _context.SaveAsync(game);
    }

    public async Task Update(string id, Game game)
    {
      game.Id = id;
      await _context.SaveAsync(game);
    }

    public async Task Delete(string id)
    {
      await _context.DeleteAsync<Game>(id);
    }

    public async Task RequestPurchase(string userId, string email, string gameId, string gameName, decimal gameValue, decimal amount)
    {
      var evt = new PurchaseRequestedEvent
      {
        UserId = userId,
        Email = email,
        GameId = gameId,
        GameName = gameName,
        GameValue = gameValue,
        Amount = amount,
        RequestedAt = DateTime.UtcNow
      };

      var queueUrl = Environment.GetEnvironmentVariable("PAYMENT_QUEUE_URL");

      await _publisher.PublishAsync(queueUrl, evt);
    }
  }
}