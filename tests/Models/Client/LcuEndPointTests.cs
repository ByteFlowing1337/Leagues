using Leagues.Models.Client;

namespace Leagues.Tests.Models.Client;

public class LcuEndPointTests
{
    [Fact]
    public void CurrentSummoner_ReturnsExpectedPath()
    {
        Assert.Equal("lol-summoner/v1/current-summoner", LcuEndPoint.CurrentSummoner);
    }

    [Fact]
    public void MatchHistory_UsesDefaultIndexes()
    {
        Assert.Equal(
            "lol-match-history/v1/products/lol/player-id/matches?begIndex=0&endIndex=20",
            LcuEndPoint.MatchHistory("player-id"));
    }

    [Fact]
    public void MatchHistory_UsesProvidedIndexes()
    {
        Assert.Equal(
            "lol-match-history/v1/products/lol/player-id/matches?begIndex=10&endIndex=30",
            LcuEndPoint.MatchHistory("player-id", 10, 30));
    }

    [Fact]
    public void GameDetails_IncludesGameId()
    {
        Assert.Equal("lol-match-history/v1/games/123456", LcuEndPoint.GameDetails(123456));
    }

    [Fact]
    public void ChampionAvatar_IncludesChampionId()
    {
        Assert.Equal("lol-game-data/assets/v1/champion-icons/99.png", LcuEndPoint.ChampionAvatar(99));
    }

    [Fact]
    public void Summoner_RemovesClientFormattingCharactersAndEscapesName()
    {
        const string copiedName = "\u2066Player Name\u2069#\u20661234\u2069";

        var endpoint = LcuEndPoint.Summoner(copiedName);

        Assert.Equal("lol-summoner/v1/summoners?name=Player%20Name%231234", endpoint);
    }

    [Fact]
    public void Summoner_TrimsAndNormalizesName()
    {
        const string decomposedName = "  e\u0301  ";

        var endpoint = LcuEndPoint.Summoner(decomposedName);

        Assert.Equal("lol-summoner/v1/summoners?name=%C3%A9", endpoint);
    }
}
