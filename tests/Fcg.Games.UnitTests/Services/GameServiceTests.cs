using Fcg.Games.Application.Exceptions;
using Fcg.Games.Application.Services;
using Fcg.Games.Contracts.Games;
using Fcg.Games.Domain.Entities;
using Fcg.Games.Domain.Paging;
using Fcg.Games.Domain.Repositories;
using NSubstitute;
using Xunit;

namespace Fcg.Games.UnitTests.Services;

public class GameServiceTests
{
    private readonly IGameRepository _repo = Substitute.For<IGameRepository>();

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), false, Arg.Any<CancellationToken>()).Returns((Game?)null);
        var sut = new GameService(_repo);
        Assert.Null(await sut.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateGameAsync_WhenSlugExists_ThrowsConflict()
    {
        _repo.ExistsBySlugAsync(Arg.Any<string>(), null, Arg.Any<CancellationToken>()).Returns(true);
        var sut = new GameService(_repo);
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
        _repo.GetRecommendationsAsync(null, 10, Arg.Any<CancellationToken>())
            .Returns(new List<Game> { game });
        var sut = new GameService(_repo);
        var result = await sut.GetRecommendationsAsync(null, 10);
        Assert.Single(result);
        Assert.Equal(game.Id, result[0].Id);
        Assert.Equal("G", result[0].Title);
    }
}
