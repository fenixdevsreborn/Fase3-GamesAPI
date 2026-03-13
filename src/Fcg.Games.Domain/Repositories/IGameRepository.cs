using Fcg.Games.Domain.Entities;
using Fcg.Games.Domain.Paging;

namespace Fcg.Games.Domain.Repositories;

public interface IGameRepository
{
    Task<Game?> GetByIdAsync(Guid id, bool includeDeleted = false, CancellationToken cancellationToken = default);
    Task<Game?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Game?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySlugAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<PagedResult<Game>> GetPagedAsync(
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
        CancellationToken cancellationToken = default);
    Task<PagedResult<Game>> SearchAsync(
        string query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Game>> GetRecommendationsAsync(
        Guid? userId,
        int count,
        CancellationToken cancellationToken = default);
    Task<Game> AddAsync(Game game, CancellationToken cancellationToken = default);
    Task UpdateAsync(Game game, CancellationToken cancellationToken = default);
    Task<int> GetPurchaseCountAsync(Guid gameId, CancellationToken cancellationToken = default);
}
