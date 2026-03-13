using Fcg.Games.Domain.Entities;
using Fcg.Games.Domain.Paging;
using Fcg.Games.Domain.Repositories;
using Fcg.Games.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Games.Infrastructure.Repositories;

public class GameRepository : IGameRepository
{
    private readonly GamesDbContext _db;

    public GameRepository(GamesDbContext db) => _db = db;

    public async Task<Game?> GetByIdAsync(Guid id, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = _db.Games.AsNoTracking();
        if (includeDeleted) query = query.IgnoreQueryFilters();
        return await query.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<Game?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _db.Games.FirstOrDefaultAsync(g => g.Id == id && g.DeletedAt == null, cancellationToken);

    public async Task<Game?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        await _db.Games.AsNoTracking().FirstOrDefaultAsync(g => g.Slug == slug, cancellationToken);

    public async Task<bool> ExistsBySlugAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Games.IgnoreQueryFilters().Where(g => g.Slug == slug);
        if (excludeId.HasValue) query = query.Where(g => g.Id != excludeId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<PagedResult<Game>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? genre = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? studio = null,
        bool? isPublished = null,
        string? sortBy = null,
        bool sortDesc = false,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Games.AsNoTracking();
        if (includeDeleted) query = query.IgnoreQueryFilters();

        if (!string.IsNullOrWhiteSpace(genre)) query = query.Where(g => g.Genre == genre);
        if (minPrice.HasValue) query = query.Where(g => g.Price >= minPrice.Value);
        if (maxPrice.HasValue) query = query.Where(g => g.Price <= maxPrice.Value);
        if (!string.IsNullOrWhiteSpace(studio)) query = query.Where(g => g.Studio == studio);
        if (isPublished.HasValue) query = query.Where(g => g.IsPublished == isPublished.Value);

        query = ApplySort(query, sortBy ?? "CreatedAt", sortDesc);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResult<Game> { Items = items, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize };
    }

    public async Task<PagedResult<Game>> SearchAsync(string query, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return await GetPagedAsync(pageNumber, pageSize, isPublished: true, cancellationToken: cancellationToken);

        var term = query.Trim().ToLower();
        var q = _db.Games.AsNoTracking()
            .Where(g => g.DeletedAt == null && g.IsPublished
                && (g.Title.ToLower().Contains(term) || (g.Description != null && g.Description.ToLower().Contains(term)) || (g.Genre != null && g.Genre.ToLower().Contains(term))));

        var totalCount = await q.CountAsync(cancellationToken);
        var items = await q.OrderByDescending(g => g.CreatedAt)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResult<Game> { Items = items, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize };
    }

    public async Task<IReadOnlyList<Game>> GetRecommendationsAsync(Guid? userId, int count, CancellationToken cancellationToken = default)
    {
        IQueryable<Game> baseQuery = _db.Games.AsNoTracking()
            .Where(g => g.DeletedAt == null && g.IsPublished && g.IsActive);

        if (userId.HasValue)
        {
            var ownedIds = await _db.UserGameLibraries
                .Where(l => l.UserId == userId.Value && l.Status == Domain.Enums.LibraryEntryStatus.Owned)
                .Select(l => l.GameId)
                .ToListAsync(cancellationToken);
            if (ownedIds.Count > 0)
                baseQuery = baseQuery.Where(g => !ownedIds.Contains(g.Id));
        }

        var purchaseCounts = await _db.UserGameLibraries
            .Where(l => l.Status == Domain.Enums.LibraryEntryStatus.Owned)
            .GroupBy(l => l.GameId)
            .Select(g => new { GameId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var gameIdsWithCount = purchaseCounts.ToDictionary(x => x.GameId, x => x.Count);

        var candidates = await baseQuery.OrderByDescending(g => g.CreatedAt).Take(count * 3).ToListAsync(cancellationToken);
        var ordered = candidates
            .OrderByDescending(g => gameIdsWithCount.GetValueOrDefault(g.Id, 0))
            .ThenByDescending(g => g.CreatedAt)
            .Take(count)
            .ToList();

        return ordered;
    }

    public async Task<Game> AddAsync(Game game, CancellationToken cancellationToken = default)
    {
        _db.Games.Add(game);
        await _db.SaveChangesAsync(cancellationToken);
        return game;
    }

    public async Task UpdateAsync(Game game, CancellationToken cancellationToken = default)
    {
        _db.Games.Update(game);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetPurchaseCountAsync(Guid gameId, CancellationToken cancellationToken = default) =>
        await _db.UserGameLibraries.CountAsync(l => l.GameId == gameId && l.Status == Domain.Enums.LibraryEntryStatus.Owned, cancellationToken);

    private static IQueryable<Game> ApplySort(IQueryable<Game> query, string sortBy, bool sortDesc)
    {
        return (sortBy?.ToLowerInvariant(), sortDesc) switch
        {
            ("title", false) => query.OrderBy(g => g.Title),
            ("title", true) => query.OrderByDescending(g => g.Title),
            ("price", false) => query.OrderBy(g => g.Price),
            ("price", true) => query.OrderByDescending(g => g.Price),
            ("createdat", false) => query.OrderBy(g => g.CreatedAt),
            (_, _) => query.OrderByDescending(g => g.CreatedAt)
        };
    }
}
