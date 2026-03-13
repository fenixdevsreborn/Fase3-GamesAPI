using Fcg.Shared.Auth;
using Fcg.Shared.Observability;
using Fcg.Games.Infrastructure.Extensions;
using Fcg.Games.Infrastructure.Persistence;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.Configure<Fcg.Games.Api.Controllers.InternalApiOptions>(
    builder.Configuration.GetSection(Fcg.Games.Api.Controllers.InternalApiOptions.SectionName));
builder.Services.AddGamesInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddFcgJwtBearer(builder.Configuration);
builder.Services.AddFcgAuthorization();
builder.Services.AddProjectObservability(builder.Configuration, "Fcg.Games.Api");
builder.Services.AddControllers();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<GamesDbContext>("db", tags: new[] { "ready" });

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info ??= new OpenApiInfo();
        document.Info.Title = "FCG Games API";
        document.Info.Version = "v1";
        document.Info.Description = "Game catalog, library, and purchase for FCG Cloud Platform. JWT from Users API.";
        return Task.CompletedTask;
    });
    options.AddDocumentTransformer<Fcg.Games.Api.OpenApi.BearerSecuritySchemeTransformer>();
});

var app = builder.Build();

app.UseFcgObservability();
app.UseMiddleware<Fcg.Games.Api.Middleware.ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") });

var enableOpenApi = !app.Environment.IsProduction() || app.Configuration.GetValue<bool>("EnableOpenApi");
if (enableOpenApi)
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.WithTitle("FCG Games API"));
}

app.MapGet("/api/discovery", (HttpContext ctx) => new
{
    service = "Fcg.Games.Api",
    basePath = ctx.Request.PathBase.Value?.TrimEnd('/') ?? "",
    openApiUrl = $"{ctx.Request.PathBase.Value?.TrimEnd('/')}/openapi/v1.json",
    docsUrl = $"{ctx.Request.PathBase.Value?.TrimEnd('/')}/scalar/v1",
    healthUrl = $"{ctx.Request.PathBase.Value?.TrimEnd('/')}/health"
}).AllowAnonymous();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GamesDbContext>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    if (!config.GetValue<bool>("UseInMemoryDatabase"))
        await db.Database.MigrateAsync();
}

app.Run();
