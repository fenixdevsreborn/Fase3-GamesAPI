namespace Fcg.Games.Api.Observability;

/// <summary>Property names for structured logs.</summary>
public static class FcgLogPropertyNames
{
    public const string TraceId = "TraceId";
    public const string SpanId = "SpanId";
    public const string CorrelationId = "CorrelationId";
    public const string ExceptionType = "ExceptionType";
    public const string HttpStatusCode = "HttpStatusCode";
}
