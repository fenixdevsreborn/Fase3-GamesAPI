namespace Fcg.Games.Api.Observability;

/// <summary>Metric names (snake_case) for Games API.</summary>
public static class FcgMetricNames
{
    public const string HttpServerRequestCount = "http.server.request.count";
    public const string HttpServerRequestDuration = "http.server.request.duration";
    public const string HttpServerActiveRequests = "http.server.active_requests";
    public const string GamesCreated = "games.created";
    public const string GamesUpdated = "games.updated";
    public const string GamesDeleted = "games.deleted";
    public const string LibraryItemsAdded = "library.items.added";
    public const string LibraryItemsRemoved = "library.items.removed";
    public const string RecommendationsGenerated = "recommendations.generated";
    public const string ExceptionsCount = "exceptions.count";

    public const string TagHttpMethod = "http.request.method";
    public const string TagHttpRoute = "http.route";
    public const string TagHttpStatusCode = "http.response.status_code";
    public const string TagExceptionType = "exception.type";
}
