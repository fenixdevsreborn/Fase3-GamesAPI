using Fcg.Games.Domain.Entities;
using Fcg.Games.Domain.Paging;

namespace Fcg.Games.Domain.Repositories;

public interface IUserGameLibraryRepository
{
    Task<UserGameLibrary?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserGameLibrary?> GetByUserAndGameAsync(Guid userId, Guid gameId, CancellationToken cancellationToken = default);
    Task<PagedResult<UserGameLibrary>> GetByUserIdAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        string? status = null,
        CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid userId, Guid gameId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guid>> GetOwnedGameIdsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserGameLibrary> AddAsync(UserGameLibrary entry, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserGameLibrary entry, CancellationToken cancellationToken = default);
    Task DeleteAsync(UserGameLibrary entry, CancellationToken cancellationToken = default);
}
