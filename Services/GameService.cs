using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using ms_games.Events;
using ms_games.Messaging;
using ms_games.Models;

namespace ms_games.Services
{
  public class GameService
  {
    private readonly DynamoDBContext _context;
    private readonly IMessagePublisher _publisher;
    private readonly string _table;
    private readonly string _paymentQueueName;

    public GameService(IAmazonDynamoDB dynamo, IMessagePublisher publisher, IConfiguration configuration)
    {
      _context = new DynamoDBContextBuilder()
        .WithDynamoDBClient(() => dynamo)
        .Build();
      _publisher = publisher;
      _table = configuration["DynamoDb:GamesTable"] ?? Environment.GetEnvironmentVariable("GAMES_TABLE") ?? "Games";
      _paymentQueueName = configuration["RabbitMq:PaymentQueueName"] ?? "payment-queue";
    }

    public async Task<List<Game>> GetAll()
    {
      return await _context.ScanAsync<Game>(new List<ScanCondition>(), ScanConfig()).GetRemainingAsync();
    }

    public async Task<Game> GetById(string id)
    {
      return await _context.LoadAsync<Game>(id, LoadConfig());
    }

    public async Task<List<Game>> GetRecommendation(string gameId, int limit = 5)
    {
      var baseGame = await _context.LoadAsync<Game>(gameId, LoadConfig());

      if (baseGame == null)
        throw new Exception("Game not found");

      var conditions = new List<ScanCondition>
      {
        new ScanCondition("Category", ScanOperator.Equal, baseGame.Category)
      };

      var games = await _context
        .ScanAsync<Game>(conditions, ScanConfig())
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
      await _context.SaveAsync(game, SaveConfig());
    }

    public async Task Update(string id, Game game)
    {
      game.Id = id;
      await _context.SaveAsync(game, SaveConfig());
    }

    public async Task Delete(string id)
    {
      await _context.DeleteAsync<Game>(id, DeleteConfig());
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

      await _publisher.PublishAsync(_paymentQueueName, evt);
    }

    private LoadConfig LoadConfig() => new()
    {
      OverrideTableName = _table
    };

    private SaveConfig SaveConfig() => new()
    {
      OverrideTableName = _table
    };

    private DeleteConfig DeleteConfig() => new()
    {
      OverrideTableName = _table
    };

    private ScanConfig ScanConfig() => new()
    {
      OverrideTableName = _table
    };
  }
}
