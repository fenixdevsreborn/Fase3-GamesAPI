namespace Fcg.Games.Api.Observability;

/// <summary>Options for AddProjectObservability.</summary>
public class ObservabilityOptions
{
    public const string SectionName = "Observability";

    public string ProjectName { get; set; } = "Fcg.Games.Api";
    public bool UseCorrelationId { get; set; } = true;
    public bool UseHttpMetrics { get; set; } = true;
    public bool UseExceptionObservability { get; set; } = true;
}
