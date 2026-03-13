using Fcg.Games.Contracts.Games;
using Fcg.Games.Contracts.Paging;
using Fcg.Games.Domain.Entities;
using Fcg.Games.Domain.Repositories;
using Fcg.Games.Application.Exceptions;
using System.Text.RegularExpressions;

namespace Fcg.Games.Application.Services;

public class GameService : IGameService
{
    private readonly IGameRepository _gameRepo;

    public GameService(IGameRepository gameRepo) => _gameRepo = gameRepo;

    public async Task<GameResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var game = await _gameRepo.GetByIdAsync(id, includeDeleted: false, cancellationToken);
        return game == null ? null : MapToResponse(game);
    }

    public async Task<PagedResponse<GameResponse>> GetPagedAsync(GameListQuery query, CancellationToken cancellationToken = default)
    {
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var paged = await _gameRepo.GetPagedAsync(
            pageNumber, pageSize,
            genre: string.IsNullOrWhiteSpace(query.Genre) ? null : query.Genre.Trim(),
            minPrice: query.MinPrice,
            maxPrice: query.MaxPrice,
            studio: string.IsNullOrWhiteSpace(query.Studio) ? null : query.Studio.Trim(),
            isPublished: query.IsPublished,
            sortBy: string.IsNullOrWhiteSpace(query.SortBy) ? null : query.SortBy.Trim(),
            sortDesc: query.SortDesc,
            includeDeleted: false,
            cancellationToken);

        return new PagedResponse<GameResponse>
        {
            Items = paged.Items.Select(MapToResponse).ToList(),
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize,
            TotalPages = paged.TotalPages,
            HasPreviousPage = paged.HasPreviousPage,
            HasNextPage = paged.HasNextPage
        };
    }

    public async Task<PagedResponse<GameResponse>> SearchAsync(string? q, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var paged = await _gameRepo.SearchAsync(q ?? "", pageNumber, pageSize, cancellationToken);
        return new PagedResponse<GameResponse>
        {
            Items = paged.Items.Select(MapToResponse).ToList(),
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize,
            TotalPages = paged.TotalPages,
            HasPreviousPage = paged.HasPreviousPage,
            HasNextPage = paged.HasNextPage
        };
    }

    public async Task<IReadOnlyList<GameResponse>> GetRecommendationsAsync(Guid? userId, int count, CancellationToken cancellationToken = default)
    {
        count = Math.Clamp(count, 1, 50);
        var games = await _gameRepo.GetRecommendationsAsync(userId, count, cancellationToken);
        return games.Select(MapToResponse).ToList();
    }

    public async Task<GameResponse> CreateGameAsync(CreateGameRequest request, CancellationToken cancellationToken = default)
    {
        var slug = Slugify(request.Title);
        if (await _gameRepo.ExistsBySlugAsync(slug, null, cancellationToken))
            throw new ConflictException($"A game with slug '{slug}' already exists.");

        var game = new Game
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Slug = slug,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Genre = string.IsNullOrWhiteSpace(request.Genre) ? null : request.Genre.Trim(),
            Studio = string.IsNullOrWhiteSpace(request.Studio) ? null : request.Studio.Trim(),
            Price = request.Price,
            CoverUrl = string.IsNullOrWhiteSpace(request.CoverUrl) ? null : request.CoverUrl.Trim(),
Tags = request.Tags?.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t!.Trim()).ToList() ?? new List<string>(),
            IsPublished = request.IsPublished,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _gameRepo.AddAsync(game, cancellationToken);
        return MapToResponse(game);
    }

    public async Task<GameResponse?> UpdateGameAsync(Guid id, UpdateGameRequest request, CancellationToken cancellationToken = default)
    {
        var game = await _gameRepo.GetByIdForUpdateAsync(id, cancellationToken);
        if (game == null) return null;

        if (request.Title != null)
        {
            game.Title = request.Title.Trim();
            game.Slug = Slugify(game.Title);
            if (await _gameRepo.ExistsBySlugAsync(game.Slug, id, cancellationToken))
                throw new ConflictException($"A game with slug '{game.Slug}' already exists.");
        }
        if (request.Description != null) game.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        if (request.Genre != null) game.Genre = string.IsNullOrWhiteSpace(request.Genre) ? null : request.Genre.Trim();
        if (request.Studio != null) game.Studio = string.IsNullOrWhiteSpace(request.Studio) ? null : request.Studio.Trim();
        if (request.CoverUrl != null) game.CoverUrl = string.IsNullOrWhiteSpace(request.CoverUrl) ? null : request.CoverUrl.Trim();
        if (request.Tags != null) game.Tags = request.Tags.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t!.Trim()).ToList();
        if (request.IsPublished.HasValue) game.IsPublished = request.IsPublished.Value;
        if (request.IsActive.HasValue) game.IsActive = request.IsActive.Value;

        game.UpdatedAt = DateTime.UtcNow;
        await _gameRepo.UpdateAsync(game, cancellationToken);
        return MapToResponse(game);
    }

    public async Task<GameResponse?> UpdatePriceAsync(Guid id, decimal price, CancellationToken cancellationToken = default)
    {
        var game = await _gameRepo.GetByIdForUpdateAsync(id, cancellationToken);
        if (game == null) return null;
        game.Price = price;
        game.UpdatedAt = DateTime.UtcNow;
        await _gameRepo.UpdateAsync(game, cancellationToken);
        return MapToResponse(game);
    }

    public async Task<GameResponse?> UpdatePublicationAsync(Guid id, bool isPublished, CancellationToken cancellationToken = default)
    {
        var game = await _gameRepo.GetByIdForUpdateAsync(id, cancellationToken);
        if (game == null) return null;
        game.IsPublished = isPublished;
        game.UpdatedAt = DateTime.UtcNow;
        await _gameRepo.UpdateAsync(game, cancellationToken);
        return MapToResponse(game);
    }

    public async Task<bool> DeleteGameAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var game = await _gameRepo.GetByIdForUpdateAsync(id, cancellationToken);
        if (game == null) return false;
        game.DeletedAt = DateTime.UtcNow;
        game.UpdatedAt = DateTime.UtcNow;
        await _gameRepo.UpdateAsync(game, cancellationToken);
        return true;
    }

    private static GameResponse MapToResponse(Game g) => new()
    {
        Id = g.Id,
        Title = g.Title,
        Slug = g.Slug,
        Description = g.Description,
        Genre = g.Genre,
        Studio = g.Studio,
        Price = g.Price,
        CoverUrl = g.CoverUrl,
        Tags = g.Tags?.ToList() ?? new List<string>(),
        IsPublished = g.IsPublished,
        IsActive = g.IsActive,
        CreatedAt = g.CreatedAt,
        UpdatedAt = g.UpdatedAt
    };

    private static string Slugify(string title)
    {
        var slug = title.Trim().ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-").Trim('-');
        return string.IsNullOrEmpty(slug) ? Guid.NewGuid().ToString("N")[..8] : slug;
    }
}
