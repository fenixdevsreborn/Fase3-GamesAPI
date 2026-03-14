using Fcg.Games.Api.Observability;
using Xunit;

namespace Fcg.Games.UnitTests.Observability;

public class FcgMetersTests
{
    [Fact]
    public void Constructor_CreatesMeterWithName()
    {
        var meters = new FcgMeters("Fcg.Games.Api.Test");

        Assert.NotNull(meters.Meter);
        Assert.Equal("Fcg.Games.Api.Test", meters.Meter.Name);
    }

    [Fact]
    public void RecordGameCreated_DoesNotThrow()
    {
        var meters = new FcgMeters("Test");
        meters.RecordGameCreated();
    }

    [Fact]
    public void RecordLibraryItemAdded_DoesNotThrow()
    {
        var meters = new FcgMeters("Test");
        meters.RecordLibraryItemAdded();
    }

    [Fact]
    public void RecordRecommendationsGenerated_DoesNotThrow()
    {
        var meters = new FcgMeters("Test");
        meters.RecordRecommendationsGenerated(5);
    }
}
