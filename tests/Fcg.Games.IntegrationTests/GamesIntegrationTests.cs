using System.Net;
using System.Net.Http.Json;
using Fcg.Games.Contracts.Internal;
using Xunit;

namespace Fcg.Games.IntegrationTests;

public class GamesIntegrationTests : IClassFixture<WebAppFixture>
{
    private readonly HttpClient _client;

    public GamesIntegrationTests(WebAppFixture factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetGames_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/games");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetGames_WithValidToken_Returns200()
    {
        var token = TestOidcServer.CreateToken();
        var request = new HttpRequestMessage(HttpMethod.Get, "/games");
        request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + token);
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task OpenApiDocument_Returns200()
    {
        var response = await _client.GetAsync("/openapi/v1.json");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task InternalAddFromPayment_WithoutApiKey_Returns401()
    {
        var body = new AddFromPaymentRequest { UserId = Guid.NewGuid(), GameId = Guid.NewGuid() };
        using var content = JsonContent.Create(body);
        var request = new HttpRequestMessage(HttpMethod.Post, "internal/library/add-from-payment") { Content = content };
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task InternalAddFromPayment_WithInvalidApiKey_Returns401()
    {
        var body = new AddFromPaymentRequest { UserId = Guid.NewGuid(), GameId = Guid.NewGuid() };
        using var content = JsonContent.Create(body);
        var request = new HttpRequestMessage(HttpMethod.Post, "internal/library/add-from-payment") { Content = content };
        request.Headers.TryAddWithoutValidation("X-Api-Key", "wrong-key");
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task InternalAddFromPayment_WithValidApiKey_WhenGameNotFound_Returns404()
    {
        var body = new AddFromPaymentRequest { UserId = Guid.NewGuid(), GameId = Guid.NewGuid() };
        using var content = JsonContent.Create(body);
        var request = new HttpRequestMessage(HttpMethod.Post, "internal/library/add-from-payment") { Content = content };
        request.Headers.TryAddWithoutValidation("X-Api-Key", "test-internal-api-key");
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
