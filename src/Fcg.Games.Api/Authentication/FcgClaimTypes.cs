namespace Fcg.Games.Api.Authentication;

/// <summary>Claim names used in FCG JWT (same as Users API). Must match for token validation.</summary>
public static class FcgClaimTypes
{
    public const string Sub = "sub";
    public const string Email = "email";
    public const string Name = "name";
    public const string Role = "role";
    public const string Scope = "scope";
    public const string Jti = "jti";
}
