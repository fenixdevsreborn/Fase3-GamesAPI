using Microsoft.AspNetCore.Http;

namespace Fcg.Games.Api.Authorization;

/// <summary>Reads current user from HttpContext.User (JWT claims). Registered as scoped.</summary>
public sealed class CurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId => _httpContextAccessor.HttpContext?.User.GetUserId();

    public bool IsAdmin => _httpContextAccessor.HttpContext?.User.IsAdmin() ?? false;
}
