using Fcg.Games.Contracts.Library;
using Fcg.Games.Contracts.Paging;

namespace Fcg.Games.Application.Services;

public interface ILibraryService
{
    Task<PagedResponse<LibraryEntryResponse>> GetUserLibraryAsync(Guid userId, LibraryListQuery query, CancellationToken cancellationToken = default);
    Task<LibraryEntryResponse?> GetLibraryEntryAsync(Guid userId, Guid entryId, CancellationToken cancellationToken = default);
    Task<LibraryEntryResponse> AddToLibraryAsync(Guid userId, AddToLibraryRequest request, CancellationToken cancellationToken = default);
    Task<LibraryEntryResponse?> UpdateLibraryEntryAsync(Guid userId, Guid entryId, UpdateLibraryEntryRequest request, CancellationToken cancellationToken = default);
    Task<bool> RemoveFromLibraryAsync(Guid userId, Guid entryId, CancellationToken cancellationToken = default);
}
