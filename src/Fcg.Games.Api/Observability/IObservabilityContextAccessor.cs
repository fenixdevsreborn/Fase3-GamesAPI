namespace Fcg.Games.Api.Observability;

/// <summary>Access to current observability context (trace/correlation).</summary>
public interface IObservabilityContextAccessor
{
    string? TraceId { get; }
    string? CorrelationId { get; }
}
