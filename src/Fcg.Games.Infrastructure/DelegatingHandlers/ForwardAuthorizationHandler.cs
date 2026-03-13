using Microsoft.AspNetCore.Http;

namespace Fcg.Games.Infrastructure.DelegatingHandlers;

/// <summary>Forwards the current request's Authorization header to the outgoing HttpClient request (e.g. when calling Payments API so the user is identified).</summary>
public sealed class ForwardAuthorizationHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ForwardAuthorizationHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var context = _httpContextAccessor.HttpContext;
        var authHeader = context?.Request.Headers.Authorization.FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader))
            request.Headers.TryAddWithoutValidation("Authorization", authHeader);

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
