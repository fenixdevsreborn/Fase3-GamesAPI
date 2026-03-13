using Fcg.Games.Contracts.Games;
using Fcg.Games.Contracts.Paging;
using Fcg.Games.Contracts.Payments;
using Fcg.Games.Application.Services;
using Fcg.Games.Application.Exceptions;
using Fcg.Shared.Auth;
using Fcg.Shared.Observability;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fcg.Games.Api.Controllers;

[ApiController]
[Route("games")]
[Produces("application/json")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly IPurchaseService _purchaseService;
    private readonly ILogger<GamesController> _logger;
    private readonly FcgMeters _meters;

    public GamesController(IGameService gameService, IPurchaseService purchaseService, ILogger<GamesController> logger, FcgMeters meters)
    {
        _gameService = gameService;
        _purchaseService = purchaseService;
        _logger = logger;
        _meters = meters;
    }

    // ----- Catalog (authenticated) -----

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(PagedResponse<GameResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<GameResponse>>> GetGames([FromQuery] GameListQuery query, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return Ok(await _gameService.GetPagedAsync(query, ct));
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GameResponse>> GetById(Guid id, CancellationToken ct)
    {
        var game = await _gameService.GetByIdAsync(id, ct);
        if (game == null) return NotFound(new { message = "Game not found." });
        return Ok(game);
    }

    [HttpGet("search")]
    [Authorize]
    [ProducesResponseType(typeof(PagedResponse<GameResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<GameResponse>>> Search([FromQuery] string? q, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        return Ok(await _gameService.SearchAsync(q, pageNumber, pageSize, ct));
    }

    [HttpGet("recommendations")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<GameResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<GameResponse>>> GetRecommendations([FromQuery] int count = 10, CancellationToken ct = default)
    {
        count = Math.Clamp(count, 1, 50);
        var userId = User.GetUserId();
        var list = await _gameService.GetRecommendationsAsync(userId, count, ct);
        _meters.RecordRecommendationsGenerated(list.Count);
        return Ok(list);
    }

    // ----- Purchase (authenticated, userId from JWT) -----

    [HttpPost("{id:guid}/purchase")]
    [Authorize]
    [ProducesResponseType(typeof(PurchaseIntentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PurchaseIntentResponse>> Purchase(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();
        try
        {
            var result = await _purchaseService.PurchaseAsync(userId.Value, id, ct);
            _logger.LogInformation("Purchase intent created for user {UserId}, game {GameId}", userId, id);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // ----- Admin -----

    [HttpPost]
    [Authorize(Roles = FcgRoles.Admin)]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GameResponse>> Create([FromBody] CreateGameRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var game = await _gameService.CreateGameAsync(request, ct);
            _meters.RecordGameCreated();
            _logger.LogInformation("Game created: {Slug} by admin", game.Slug);
            return CreatedAtAction(nameof(GetById), new { id = game.Id }, game);
        }
        catch (ConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = FcgRoles.Admin)]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GameResponse>> Update(Guid id, [FromBody] UpdateGameRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var game = await _gameService.UpdateGameAsync(id, request, ct);
            if (game == null) return NotFound(new { message = "Game not found." });
            _meters.RecordGameUpdated();
            return Ok(game);
        }
        catch (ConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/price")]
    [Authorize(Roles = FcgRoles.Admin)]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GameResponse>> UpdatePrice(Guid id, [FromBody] UpdatePriceRequest request, CancellationToken ct)
    {
        var game = await _gameService.UpdatePriceAsync(id, request.Price, ct);
        if (game == null) return NotFound(new { message = "Game not found." });
        return Ok(game);
    }

    [HttpPatch("{id:guid}/publication")]
    [Authorize(Roles = FcgRoles.Admin)]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GameResponse>> UpdatePublication(Guid id, [FromBody] UpdatePublicationRequest request, CancellationToken ct)
    {
        var game = await _gameService.UpdatePublicationAsync(id, request.IsPublished, ct);
        if (game == null) return NotFound(new { message = "Game not found." });
        return Ok(game);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = FcgRoles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _gameService.DeleteGameAsync(id, ct);
        if (!deleted) return NotFound(new { message = "Game not found." });
        _meters.RecordGameDeleted();
        _logger.LogInformation("Game deleted (soft): {Id} by admin", id);
        return NoContent();
    }
}
