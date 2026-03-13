using System.Net;
using System.Net.Http.Json;
using Fcg.Games.Application.Services;
using Fcg.Games.Contracts.Payments;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fcg.Games.Infrastructure.Services;

public class PaymentsApiClient : IPaymentsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PaymentsApiClient> _logger;
    private readonly PaymentsApiOptions _options;

    public PaymentsApiClient(HttpClient httpClient, IOptions<PaymentsApiOptions> options, ILogger<PaymentsApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<PurchaseIntentResponse> CreatePurchaseIntentAsync(PurchaseIntentRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_options.BaseAddress) || !_options.UseRealApi)
        {
            _logger.LogInformation("Payments API stub: CreatePurchaseIntent for User {UserId}, Game {GameId}, Amount {Amount}",
                request.UserId, request.GameId, request.Amount);
            return new PurchaseIntentResponse(
                PaymentId: $"stub-{Guid.NewGuid():N}",
                Status: "Pending",
                CheckoutUrl: _options.StubCheckoutUrl);
        }

        var body = new CreatePaymentRequestDto { GameId = request.GameId, Currency = request.Currency };
        var response = await _httpClient.PostAsJsonAsync("payments", body, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var error = await response.Content.ReadFromJsonAsync<PaymentErrorDto>(cancellationToken).ConfigureAwait(false);
            throw new Fcg.Games.Application.Exceptions.ConflictException(error?.Message ?? "A pending payment already exists for this game.");
        }

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PaymentResponseDto>(cancellationToken).ConfigureAwait(false);
        return result == null
            ? throw new InvalidOperationException("Payments API returned null.")
            : new PurchaseIntentResponse(result.Id.ToString(), result.Status, CheckoutUrl: null);
    }
}

internal sealed class PaymentErrorDto
{
    public string? Message { get; set; }
}

public class PaymentsApiOptions
{
    public const string SectionName = "PaymentsApi";
    public string BaseAddress { get; set; } = "";
    public bool UseRealApi { get; set; } = false;
    public string? StubCheckoutUrl { get; set; } = "https://payments.example.com/checkout/stub";
}
