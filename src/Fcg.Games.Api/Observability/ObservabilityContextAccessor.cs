namespace Fcg.Games.Api.Observability;

/// <summary>Implementation that reads from current Activity.</summary>
public sealed class ObservabilityContextAccessor : IObservabilityContextAccessor
{
    public string? TraceId => ObservabilityContext.GetCurrentTraceId();
    public string? CorrelationId => ObservabilityContext.GetCurrentCorrelationId();
}
