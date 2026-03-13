using Fcg.Games.Contracts.Library;
using Fcg.Games.Contracts.Paging;
using Fcg.Games.Domain.Entities;
using Fcg.Games.Domain.Enums;
using Fcg.Games.Domain.Repositories;
using Fcg.Games.Application.Exceptions;

namespace Fcg.Games.Application.Services;

public class LibraryService : ILibraryService
{
    private readonly IUserGameLibraryRepository _libraryRepo;
    private readonly IGameRepository _gameRepo;

    public LibraryService(IUserGameLibraryRepository libraryRepo, IGameRepository gameRepo)
    {
        _libraryRepo = libraryRepo;
        _gameRepo = gameRepo;
    }

    public async Task<PagedResponse<LibraryEntryResponse>> GetUserLibraryAsync(Guid userId, LibraryListQuery query, CancellationToken cancellationToken = default)
    {
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var paged = await _libraryRepo.GetByUserIdAsync(userId, pageNumber, pageSize, query.Status, cancellationToken);
        return new PagedResponse<LibraryEntryResponse>
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

    public async Task<LibraryEntryResponse?> GetLibraryEntryAsync(Guid userId, Guid entryId, CancellationToken cancellationToken = default)
    {
        var entry = await _libraryRepo.GetByIdAsync(entryId, cancellationToken);
        if (entry == null || entry.UserId != userId) return null;
        return MapToResponse(entry);
    }

    public async Task<LibraryEntryResponse> AddToLibraryAsync(Guid userId, AddToLibraryRequest request, CancellationToken cancellationToken = default)
    {
        var game = await _gameRepo.GetByIdAsync(request.GameId, includeDeleted: false, cancellationToken);
        if (game == null) throw new NotFoundException("Game", request.GameId);

        var status = ParseStatus(request.Status);
        var existing = await _libraryRepo.GetByUserAndGameAsync(userId, request.GameId, cancellationToken);
        if (existing != null)
        {
            existing.Status = status;
            existing.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
            existing.UpdatedAt = DateTime.UtcNow;
            await _libraryRepo.UpdateAsync(existing, cancellationToken);
            return MapToResponse(existing);
        }

        var entry = new UserGameLibrary
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            GameId = request.GameId,
            Status = status,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _libraryRepo.AddAsync(entry, cancellationToken);
        entry.Game = game;
        return MapToResponse(entry);
    }

    public async Task<LibraryEntryResponse?> UpdateLibraryEntryAsync(Guid userId, Guid entryId, UpdateLibraryEntryRequest request, CancellationToken cancellationToken = default)
    {
        var entry = await _libraryRepo.GetByIdAsync(entryId, cancellationToken);
        if (entry == null || entry.UserId != userId) return null;

        if (request.Status != null && Enum.TryParse<LibraryEntryStatus>(request.Status, true, out var s))
            entry.Status = s;
        if (request.Notes != null) entry.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        entry.UpdatedAt = DateTime.UtcNow;
        await _libraryRepo.UpdateAsync(entry, cancellationToken);
        return MapToResponse(entry);
    }

    public async Task<bool> RemoveFromLibraryAsync(Guid userId, Guid entryId, CancellationToken cancellationToken = default)
    {
        var entry = await _libraryRepo.GetByIdAsync(entryId, cancellationToken);
        if (entry == null || entry.UserId != userId) return false;
        await _libraryRepo.DeleteAsync(entry, cancellationToken);
        return true;
    }

    private static LibraryEntryResponse MapToResponse(UserGameLibrary e)
    {
        var resp = new LibraryEntryResponse
        {
            Id = e.Id,
            UserId = e.UserId,
            GameId = e.GameId,
            Status = e.Status.ToString(),
            SourcePaymentId = e.SourcePaymentId,
            Notes = e.Notes,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt
        };
        if (e.Game != null)
            resp.Game = new GameSummaryDto
            {
                Id = e.Game.Id,
                Title = e.Game.Title,
                Slug = e.Game.Slug,
                CoverUrl = e.Game.CoverUrl,
                Price = e.Game.Price,
                Genre = e.Game.Genre,
                IsPublished = e.Game.IsPublished
            };
        return resp;
    }

    private static LibraryEntryStatus ParseStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status)) return LibraryEntryStatus.Wishlist;
        return Enum.TryParse<LibraryEntryStatus>(status.Trim(), true, out var s) ? s : LibraryEntryStatus.Wishlist;
    }
}
