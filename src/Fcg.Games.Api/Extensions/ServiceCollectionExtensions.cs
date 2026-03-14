using Fcg.Games.Api.Authentication;
using Fcg.Games.Api.Authorization;
using Fcg.Games.Api.Observability;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fcg.Games.Api.Extensions;

/// <summary>Combined DI extensions for Games API: JWT validation, Authorization, Observability, CurrentUser.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGamesApiAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFcgJwtBearer(configuration);
        services.AddFcgAuthorization();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
        return services;
    }

    public static IServiceCollection AddGamesApiObservability(this IServiceCollection services, IConfiguration configuration, string projectName = "Fcg.Games.Api")
    {
        services.AddProjectObservability(configuration, projectName);
        return services;
    }
}
