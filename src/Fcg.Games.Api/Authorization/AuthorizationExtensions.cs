using Fcg.Games.Api.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Fcg.Games.Api.Authorization;

/// <summary>Registers FCG authorization policies: RequireAuthenticatedUser, RequireAdmin, optional RequireScope.</summary>
public static class AuthorizationExtensions
{
    public static IServiceCollection AddFcgAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(FcgPolicies.RequireAuthenticatedUser, policy =>
                policy.RequireAuthenticatedUser());

            options.AddPolicy(FcgPolicies.RequireAdmin, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireRole(FcgRoles.Admin));
        });
        return services;
    }

    public static string RequireScopePolicyName(string scope) => FcgPolicies.RequireScopePrefix + scope;

    public static AuthorizationOptions AddFcgScopePolicy(this AuthorizationOptions options, string scope)
    {
        options.AddPolicy(RequireScopePolicyName(scope), policy =>
            policy.RequireAuthenticatedUser()
                .RequireAssertion(ctx => ctx.User.HasScope(scope)));
        return options;
    }
}
