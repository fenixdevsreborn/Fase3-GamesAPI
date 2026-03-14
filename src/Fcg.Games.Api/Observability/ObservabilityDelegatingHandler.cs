using System.Diagnostics;
using System.Net.Http;

namespace Fcg.Games.Api.Observability;

/// <summary>Propagates X-Correlation-ID and traceparent (W3C) on outgoing HttpClient calls. Register with AddHttpMessageHandler.</summary>
public sealed class ObservabilityDelegatingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var correlationId = ObservabilityContext.GetCurrentCorrelationId();
        if (!string.IsNullOrEmpty(correlationId))
            request.Headers.TryAddWithoutValidation(CorrelationIdMiddleware.HeaderName, correlationId);

        var activity = Activity.Current;
        if (activity != null)
        {
            var traceparent = activity.Id ?? activity.RootId;
            if (!string.IsNullOrEmpty(traceparent))
                request.Headers.TryAddWithoutValidation("traceparent", traceparent);
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
