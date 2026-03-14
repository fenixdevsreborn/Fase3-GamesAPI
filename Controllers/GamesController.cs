using Microsoft.AspNetCore.Mvc;
using ms_games.Models;
using ms_games.Services;

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
  public async Task<IActionResult> Get()
  {
    return Ok(await _service.GetAll());
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> GetById(string id)
  {
    return Ok(await _service.GetById(id));
  }

  [HttpPost]
  public async Task<IActionResult> Create([FromBody] Game game)
  {
    await _service.Create(game);
    return Ok(game);
  }

  [HttpPut("{id}")]
  public async Task<IActionResult> Update(string id, [FromBody] Game game)
  {
    await _service.Update(id, game);
    return Ok();
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> Delete(string id)
  {
    await _service.Delete(id);
    return Ok();
  }

  [HttpPost("{gameId}/buy")]
  public async Task<IActionResult> BuyGame(string gameId, [FromBody] BuyRequest request)
  {
    var userId = User.FindFirst("sub")?.Value;
    var email = User.FindFirst("email")?.Value;

    if (userId == null || email == null)
      return Unauthorized();

    var game = await _service.GetById(gameId);

    await _service.RequestPurchase(
      userId,
      email,
      gameId,
      game.Name,
      game.Price,
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