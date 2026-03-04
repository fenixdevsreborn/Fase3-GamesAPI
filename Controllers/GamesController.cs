using Microsoft.AspNetCore.Mvc;
using ms_games.Events;
using ms_games.Models;
using ms_games.Publisher;

namespace ms_games.Controllers
{
  [ApiController]
  [Route("games")]
  public class GamesController : ControllerBase
  {
    private readonly SqsPublisher _publisher;

    public GamesController(SqsPublisher publisher)
    {
      _publisher = publisher;
    }

    [HttpGet]
    public IActionResult Get()
    {
      return Ok(new[]
      {
                new { Id = "1", Name = "FIFA", Price = 199 },
                new { Id = "2", Name = "COD", Price = 299 }
            });
    }

    [HttpPost("{gameId}/buy")]
    public async Task<IActionResult> BuyGame(string gameId, [FromBody] BuyRequest request)
    {
      var evt = new PurchaseRequestedEvent
      {
        UserId = request.UserId,
        GameId = gameId,
        Amount = request.Amount,
        RequestedAt = DateTime.UtcNow
      };

      await _publisher.Publish(evt);

      return Accepted(new
      {
        Message = "Purchase request sent",
        GameId = gameId,
        Amount = request.Amount
      });
    }
  }
}