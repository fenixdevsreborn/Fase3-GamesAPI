namespace Fcg.Games.Api.Authentication;

/// <summary>Authorization policy names. Same as Users API.</summary>
public static class FcgPolicies
{
    public const string RequireAuthenticatedUser = "RequireAuthenticatedUser";
    public const string RequireAdmin = "RequireAdmin";
    public const string RequireScopePrefix = "RequireScope:";
}
