using Leagues.Models.Mapper;

namespace Leagues.Tests.Models.Mapper;

public class MatchSummaryTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var playedAt = new DateTimeOffset(2026, 9, 13, 14, 30, 0, TimeSpan.FromHours(2));
        var duration = TimeSpan.FromMinutes(31.5);

        var summary = new MatchSummary(
            gameId: 123456,
            playedAt: playedAt,
            duration: duration,
            gameMode: "CLASSIC",
            championId: 99,
            win: true,
            kills: 10,
            deaths: 2,
            assists: 8);

        Assert.Equal(123456, summary.GameId);
        Assert.Equal(playedAt, summary.PlayedAt);
        Assert.Equal(duration, summary.Duration);
        Assert.Equal("CLASSIC", summary.GameMode);
        Assert.Equal(99, summary.ChampionId);
        Assert.True(summary.Win);
        Assert.Equal(10, summary.Kills);
        Assert.Equal(2, summary.Deaths);
        Assert.Equal(8, summary.Assists);
    }
}