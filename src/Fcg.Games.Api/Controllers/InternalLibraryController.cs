using Fcg.Games.Application.Services;
using Fcg.Games.Contracts.Internal;
using Fcg.Games.Contracts.Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Fcg.Games.Api.Controllers;

[ApiController]
[Route("internal/library")]
[AllowAnonymous]
[Produces("application/json")]
public class InternalLibraryController : ControllerBase
{
    private readonly ILibraryService _libraryService;
    private readonly ILogger<InternalLibraryController> _logger;
    private readonly InternalApiOptions _options;

    public InternalLibraryController(
        ILibraryService libraryService,
        ILogger<InternalLibraryController> logger,
        IOptions<InternalApiOptions> options)
    {
        _libraryService = libraryService;
        _logger = logger;
        _options = options.Value;
    }

    [HttpPost("add-from-payment")]
    [ProducesResponseType(typeof(LibraryEntryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LibraryEntryResponse>> AddFromPayment(
        [FromBody] AddFromPaymentRequest request,
        CancellationToken ct)
    {
        var apiKey = Request.Headers["X-Api-Key"].FirstOrDefault();
        if (string.IsNullOrEmpty(_options.ApiKey) || apiKey != _options.ApiKey)
        {
            _logger.LogWarning("Internal add-from-payment rejected: invalid or missing X-Api-Key");
            return Unauthorized(new { message = "Invalid or missing X-Api-Key." });
        }

        if (request == null || request.UserId == default || request.GameId == default)
            return BadRequest(new { message = "UserId and GameId are required." });

        try
        {
            var addRequest = new AddToLibraryRequest { GameId = request.GameId, Status = "Owned" };
            var entry = await _libraryService.AddToLibraryAsync(request.UserId, addRequest, ct);
            _logger.LogInformation(
                "Internal add-from-payment: added game {GameId} to library for user {UserId}, entry {EntryId}",
                request.GameId, request.UserId, entry.Id);
            return Ok(entry);
        }
        catch (Fcg.Games.Application.Exceptions.NotFoundException)
        {
            return NotFound(new { message = "Game not found." });
        }
    }
}

public class InternalApiOptions
{
    public const string SectionName = "InternalApi";
    public string ApiKey { get; set; } = "";
}
