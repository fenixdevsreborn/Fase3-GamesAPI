using Fcg.Games.Contracts.Games;
using Fcg.Games.Contracts.Paging;

namespace Fcg.Games.Application.Services;

public interface IGameService
{
    Task<GameResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResponse<GameResponse>> GetPagedAsync(GameListQuery query, CancellationToken cancellationToken = default);
    Task<PagedResponse<GameResponse>> SearchAsync(string? q, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GameResponse>> GetRecommendationsAsync(Guid? userId, int count, CancellationToken cancellationToken = default);
    Task<GameResponse> CreateGameAsync(CreateGameRequest request, CancellationToken cancellationToken = default);
    Task<GameResponse?> UpdateGameAsync(Guid id, UpdateGameRequest request, CancellationToken cancellationToken = default);
    Task<GameResponse?> UpdatePriceAsync(Guid id, decimal price, CancellationToken cancellationToken = default);
    Task<GameResponse?> UpdatePublicationAsync(Guid id, bool isPublished, CancellationToken cancellationToken = default);
    Task<bool> DeleteGameAsync(Guid id, CancellationToken cancellationToken = default);
}
