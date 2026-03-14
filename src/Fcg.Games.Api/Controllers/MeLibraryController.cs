using Fcg.Games.Api.Authorization;
using Fcg.Games.Api.Observability;
using Fcg.Games.Application.Exceptions;
using Fcg.Games.Application.Services;
using Fcg.Games.Contracts.Library;
using Fcg.Games.Contracts.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fcg.Games.Api.Controllers;

[ApiController]
[Route("me/library")]
[Authorize(Policy = Fcg.Games.Api.Authentication.FcgPolicies.RequireAuthenticatedUser)]
[Produces("application/json")]
public class MeLibraryController : ControllerBase
{
    private readonly ILibraryService _libraryService;
    private readonly ILogger<MeLibraryController> _logger;
    private readonly FcgMeters _meters;

    public MeLibraryController(ILibraryService libraryService, ILogger<MeLibraryController> logger, FcgMeters meters)
    {
        _libraryService = libraryService;
        _logger = logger;
        _meters = meters;
    }

    /// <summary>List current user's library (userId from JWT sub only).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<LibraryEntryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<LibraryEntryResponse>>> GetLibrary([FromQuery] LibraryListQuery query, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return Ok(await _libraryService.GetUserLibraryAsync(userId.Value, query, ct));
    }

    /// <summary>Get a library entry by id (owner only).</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LibraryEntryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LibraryEntryResponse>> GetEntry(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();
        var entry = await _libraryService.GetLibraryEntryAsync(userId.Value, id, ct);
        if (entry == null) return NotFound(new { message = "Library entry not found." });
        return Ok(entry);
    }

    /// <summary>Add game to current user's library (userId from JWT only; do not trust body for userId).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(LibraryEntryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LibraryEntryResponse>> Add([FromBody] AddToLibraryRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var entry = await _libraryService.AddToLibraryAsync(userId.Value, request, ct);
            _meters.RecordLibraryItemAdded();
            _logger.LogInformation("User {UserId} added game {GameId} to library", userId, request.GameId);
            return CreatedAtAction(nameof(GetEntry), new { id = entry.Id }, entry);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(LibraryEntryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LibraryEntryResponse>> Update(Guid id, [FromBody] UpdateLibraryEntryRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var entry = await _libraryService.UpdateLibraryEntryAsync(userId.Value, id, request, ct);
        if (entry == null) return NotFound(new { message = "Library entry not found." });
        return Ok(entry);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();
        var removed = await _libraryService.RemoveFromLibraryAsync(userId.Value, id, ct);
        if (!removed) return NotFound(new { message = "Library entry not found." });
        _meters.RecordLibraryItemRemoved();
        _logger.LogInformation("User {UserId} removed library entry {Id}", userId, id);
        return NoContent();
    }
}
