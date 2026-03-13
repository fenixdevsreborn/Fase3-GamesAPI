using Fcg.Games.Contracts.Payments;
using Fcg.Games.Domain.Entities;
using Fcg.Games.Domain.Enums;
using Fcg.Games.Domain.Repositories;
using Fcg.Games.Application.Exceptions;

namespace Fcg.Games.Application.Services;

public class PurchaseService : IPurchaseService
{
    private readonly IGameRepository _gameRepo;
    private readonly IUserGameLibraryRepository _libraryRepo;
    private readonly IPaymentsApiClient _paymentsClient;

    public PurchaseService(
        IGameRepository gameRepo,
        IUserGameLibraryRepository libraryRepo,
        IPaymentsApiClient paymentsClient)
    {
        _gameRepo = gameRepo;
        _libraryRepo = libraryRepo;
        _paymentsClient = paymentsClient;
    }

    public async Task<PurchaseIntentResponse> PurchaseAsync(Guid userId, Guid gameId, CancellationToken cancellationToken = default)
    {
        var game = await _gameRepo.GetByIdAsync(gameId, includeDeleted: false, cancellationToken);
        if (game == null) throw new NotFoundException("Game", gameId);
        if (!game.IsPublished) throw new ConflictException("Game is not available for purchase.");

        var existingEntry = await _libraryRepo.GetByUserAndGameAsync(userId, gameId, cancellationToken);
        if (existingEntry?.Status == LibraryEntryStatus.Owned)
            throw new ConflictException("You already own this game.");

        var request = new PurchaseIntentRequest(userId, gameId, game.Price, "BRL");
        return await _paymentsClient.CreatePurchaseIntentAsync(request, cancellationToken);
    }
}
