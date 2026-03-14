namespace Fcg.Games.Api.Authorization;

/// <summary>Provides the current authenticated user id from JWT (sub). Use in services that do not have HttpContext.</summary>
public interface ICurrentUserAccessor
{
    /// <summary>Current user id from JWT sub claim, or null if not authenticated.</summary>
    Guid? UserId { get; }

    /// <summary>True if the current user has role admin.</summary>
    bool IsAdmin { get; }
}
