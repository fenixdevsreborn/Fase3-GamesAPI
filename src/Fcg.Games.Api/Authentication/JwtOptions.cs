namespace Fcg.Games.Api.Authentication;

/// <summary>JWT configuration for validation (token emitted by Users API). Same section name and defaults.</summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "Fcg.Users.Api";
    public string Audience { get; set; } = "fcg-cloud-platform";
    public string SigningKey { get; set; } = string.Empty;
    public int ExpirationSeconds { get; set; } = 3600;

    public const int MinSigningKeyLength = 32;
}
