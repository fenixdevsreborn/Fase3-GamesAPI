using Fcg.Games.Application.Services;
using Fcg.Games.Domain.Repositories;
using Fcg.Games.Infrastructure.DelegatingHandlers;
using Fcg.Games.Infrastructure.Persistence;
using Fcg.Games.Infrastructure.Repositories;
using Fcg.Games.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;

namespace Fcg.Games.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGamesInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment? hostEnvironment = null,
        Action<IHttpClientBuilder>? configurePaymentsHttpClient = null)
    {
        var useInMemory = configuration.GetValue<bool>("UseInMemoryDatabase")
            || string.Equals(hostEnvironment?.EnvironmentName, "Testing", StringComparison.OrdinalIgnoreCase);

        if (useInMemory)
            services.AddDbContext<GamesDbContext>(options => options.UseInMemoryDatabase("FcgGamesTests"));
        else
        {
            var cs = configuration.GetConnectionString("GamesDb")
                ?? throw new InvalidOperationException("ConnectionStrings:GamesDb is required.");
            services.AddDbContext<GamesDbContext>(options => options.UseNpgsql(cs));
        }

        services.Configure<PaymentsApiOptions>(configuration.GetSection(PaymentsApiOptions.SectionName));

        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IUserGameLibraryRepository, UserGameLibraryRepository>();
        services.AddScoped<IGameService, GameService>();
        services.AddScoped<ILibraryService, LibraryService>();
        services.AddScoped<IPurchaseService, PurchaseService>();
        services.AddScoped<ForwardAuthorizationHandler>();

        var paymentsClientBuilder = services.AddHttpClient<IPaymentsApiClient, PaymentsApiClient>()
            .AddHttpMessageHandler<ForwardAuthorizationHandler>()
            .ConfigureHttpClient((sp, client) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var baseUrl = config[$"{PaymentsApiOptions.SectionName}:BaseAddress"];
                if (!string.IsNullOrEmpty(baseUrl))
                    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
                client.Timeout = TimeSpan.FromSeconds(15);
            });
        configurePaymentsHttpClient?.Invoke(paymentsClientBuilder);
        paymentsClientBuilder.AddStandardResilienceHandler(options =>
        {
            options.Retry.MaxRetryAttempts = 2;
            options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
            options.Retry.UseJitter = true;
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(15);
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(15);
        });

        return services;
    }
}
