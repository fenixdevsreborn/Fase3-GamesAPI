using Fcg.Games.Domain.Entities;
using Fcg.Games.Domain.Paging;
using Fcg.Games.Domain.Repositories;
using Fcg.Games.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Games.Infrastructure.Repositories;

public class UserGameLibraryRepository : IUserGameLibraryRepository
{
    private readonly GamesDbContext _db;

    public UserGameLibraryRepository(GamesDbContext db) => _db = db;

    public async Task<UserGameLibrary?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _db.UserGameLibraries.Include(e => e.Game).FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<UserGameLibrary?> GetByUserAndGameAsync(Guid userId, Guid gameId, CancellationToken cancellationToken = default) =>
        await _db.UserGameLibraries.Include(e => e.Game).FirstOrDefaultAsync(e => e.UserId == userId && e.GameId == gameId, cancellationToken);

    public async Task<PagedResult<UserGameLibrary>> GetByUserIdAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.UserGameLibraries.AsNoTracking().Include(e => e.Game).Where(e => e.UserId == userId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Domain.Enums.LibraryEntryStatus>(status, true, out var s))
            query = query.Where(e => e.Status == s);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(e => e.UpdatedAt)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResult<UserGameLibrary> { Items = items, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize };
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid gameId, CancellationToken cancellationToken = default) =>
        await _db.UserGameLibraries.AnyAsync(e => e.UserId == userId && e.GameId == gameId, cancellationToken);

    public async Task<IReadOnlyList<Guid>> GetOwnedGameIdsAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _db.UserGameLibraries
            .Where(e => e.UserId == userId && e.Status == Domain.Enums.LibraryEntryStatus.Owned)
            .Select(e => e.GameId)
            .ToListAsync(cancellationToken);

    public async Task<UserGameLibrary> AddAsync(UserGameLibrary entry, CancellationToken cancellationToken = default)
    {
        _db.UserGameLibraries.Add(entry);
        await _db.SaveChangesAsync(cancellationToken);
        return entry;
    }

    public async Task UpdateAsync(UserGameLibrary entry, CancellationToken cancellationToken = default)
    {
        _db.UserGameLibraries.Update(entry);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(UserGameLibrary entry, CancellationToken cancellationToken = default)
    {
        _db.UserGameLibraries.Remove(entry);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
