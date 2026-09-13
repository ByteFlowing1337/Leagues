using Leagues.Models.Client;

namespace Leagues.Tests.Models.Client;

/// <summary>
/// Exercises the branching logic inside <see cref="Phase.TryExtractPhase"/>, the parser
/// responsible for turning raw LCU gameflow-phase websocket payloads into a phase name.
/// </summary>
public class PhaseTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryExtractPhase_NullOrWhitespacePayload_ReturnsNull(string? payload)
    {
        Assert.Null(Phase.TryExtractPhase(payload!));
    }

    [Fact]
    public void TryExtractPhase_InvalidJson_ReturnsNull()
    {
        Assert.Null(Phase.TryExtractPhase("not json"));
    }

    [Fact]
    public void TryExtractPhase_NonArrayPayload_ReturnsNull()
    {
        Assert.Null(Phase.TryExtractPhase("""{ "phase": "ReadyCheck" }"""));
    }

    [Fact]
    public void TryExtractPhase_ArrayTooShort_ReturnsNull()
    {
        Assert.Null(Phase.TryExtractPhase("[5, \"OnJsonApiEvent\"]"));
    }

    [Fact]
    public void TryExtractPhase_NonStringEventName_ReturnsNull()
    {
        Assert.Null(Phase.TryExtractPhase("[5, 42, \"ReadyCheck\"]"));
    }

    [Fact]
    public void TryExtractPhase_UnrecognizedEventName_ReturnsNull()
    {
        Assert.Null(Phase.TryExtractPhase("""[5, "SomeOtherEvent", "ReadyCheck"]"""));
    }

    [Theory]
    [InlineData("OnJsonApiEvent")]
    [InlineData("ONJSONAPIEVENT")]
    [InlineData("OnJsonApiEvent_lol-gameflow_v1_gameflow-phase")]
    [InlineData("ONJSONAPIEVENT_LOL-GAMEFLOW_V1_GAMEFLOW-PHASE")]
    public void TryExtractPhase_StringEventBody_ReturnsBodyDirectly(string eventName)
    {
        var payload = $"""[5, "{eventName}", "ReadyCheck"]""";

        Assert.Equal("ReadyCheck", Phase.TryExtractPhase(payload));
    }

    [Fact]
    public void TryExtractPhase_ObjectEventBodyWithMismatchedUri_ReturnsNull()
    {
        const string payload = """
                               [5, "OnJsonApiEvent", { "uri": "/lol-lobby/v2/lobby", "data": "ReadyCheck" }]
                               """;

        Assert.Null(Phase.TryExtractPhase(payload));
    }

    [Fact]
    public void TryExtractPhase_ObjectEventBodyWithMatchingUriAndData_ReturnsData()
    {
        const string payload = """
                               [5, "OnJsonApiEvent", { "uri": "/lol-gameflow/v1/gameflow-phase", "data": "ChampSelect" }]
                               """;

        Assert.Equal("ChampSelect", Phase.TryExtractPhase(payload));
    }

    [Fact]
    public void TryExtractPhase_ObjectEventBodyWithoutUri_StillReturnsData()
    {
        const string payload = """
                               [5, "OnJsonApiEvent", { "data": "InProgress" }]
                               """;

        Assert.Equal("InProgress", Phase.TryExtractPhase(payload));
    }

    [Fact]
    public void TryExtractPhase_ObjectEventBodyWithoutData_ReturnsNull()
    {
        const string payload = """
                               [5, "OnJsonApiEvent", { "uri": "/lol-gameflow/v1/gameflow-phase" }]
                               """;

        Assert.Null(Phase.TryExtractPhase(payload));
    }

    [Fact]
    public void TryExtractPhase_EventBodyIsNumber_ReturnsNull()
    {
        const string payload = """[5, "OnJsonApiEvent", 42]""";

        Assert.Null(Phase.TryExtractPhase(payload));
    }
}