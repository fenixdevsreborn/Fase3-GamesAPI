using Microsoft.AspNetCore.Mvc;
using ms_games.Models;
using ms_games.Services;

namespace ms_games.Controllers
{
  [ApiController]
  [Route("games")]
  public class GamesController : ControllerBase
  {
    private readonly GameService _service;

    public GamesController(GameService service)
    {
      _service = service;
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
      await _service.RequestPurchase(
        request.UserId,
        gameId,
        request.Amount
      );

      return Accepted(new
      {
        Message = "Purchase request sent",
        GameId = gameId,
        Amount = request.Amount
      });
    }
  }
}