using Fcg.Games.Application.Exceptions;
using Fcg.Games.Application.Services;
using Fcg.Games.Contracts.Games;
using Fcg.Games.Domain.Entities;
using Fcg.Games.Domain.Paging;
using Fcg.Games.Domain.Repositories;
using Moq;
using Xunit;

namespace Fcg.Games.UnitTests.Services;

public class GameServiceTests
{
    private readonly Mock<IGameRepository> _repo = new();

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), false, It.IsAny<CancellationToken>())).ReturnsAsync((Game?)null);
        var sut = new GameService(_repo.Object);
        Assert.Null(await sut.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateGameAsync_WhenSlugExists_ThrowsConflict()
    {
        _repo.Setup(r => r.ExistsBySlugAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var sut = new GameService(_repo.Object);
        var request = new CreateGameRequest { Title = "Test Game", Price = 10 };
        await Assert.ThrowsAsync<ConflictException>(() => sut.CreateGameAsync(request));
    }

    [Fact]
    public async Task GetRecommendationsAsync_ReturnsMappedResponses()
    {
        var game = new Game
        {
            Id = Guid.NewGuid(),
            Title = "G",
            Slug = "g",
            Price = 5,
            IsPublished = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _repo.Setup(r => r.GetRecommendationsAsync(null, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Game> { game });
        var sut = new GameService(_repo.Object);
        var result = await sut.GetRecommendationsAsync(null, 10);
        Assert.Single(result);
        Assert.Equal(game.Id, result[0].Id);
        Assert.Equal("G", result[0].Title);
    }
}
