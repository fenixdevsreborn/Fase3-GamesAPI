using System.Security.Claims;
using Fcg.Games.Api.Authentication;

namespace Fcg.Games.Api.Authorization;

/// <summary>Owner-based authorization: only the resource owner or an admin may access. Use for library entries.</summary>
public static class OwnerAuthorization
{
    /// <summary>Returns true if the current user can access the resource (owner or admin). Use: if (!User.CanAccessResource(ownerId)) return NotFound();</summary>
    public static bool CanAccessResource(this ClaimsPrincipal user, Guid resourceOwnerId)
    {
        if (user.IsAdmin()) return true;
        var userId = user.GetUserId();
        return userId.HasValue && userId.Value == resourceOwnerId;
    }
}
