using System.Text.Json;
using Leagues.Models.Dto;

namespace Leagues.Tests.Models.Dto;

public class MatchHistoryDtoJsonTests
{
    [Fact]
    public void Deserialize_MapsNestedMatchHistoryData()
    {
        const string json = """
                            {
                              "accountId": 42,
                              "platformId": "NA1",
                              "games": {
                                "gameCount": 1,
                                "games": [
                                  {
                                    "gameId": 123456,
                                    "gameCreation": 1720000000000,
                                    "gameDuration": 1800,
                                    "gameMode": "CLASSIC",
                                    "participants": [
                                      {
                                        "championId": 99,
                                        "participantId": 1,
                                        "stats": {
                                          "kills": 10,
                                          "deaths": 2,
                                          "assists": 8,
                                          "win": true,
                                          "causedGameEndFromIGNBSurrender": true,
                                          "gameEndedInIGNBSurrender": true
                                        },
                                        "timeline": {
                                          "lane": "MIDDLE",
                                          "goldPerMinDeltas": { "0-10": 350.5 }
                                        }
                                      }
                                    ],
                                    "participantIdentities": [
                                      {
                                        "participantId": 1,
                                        "player": {
                                          "gameName": "Player",
                                          "tagLine": "1234",
                                          "puuid": "player-id"
                                        }
                                      }
                                    ],
                                    "teams": [
                                      {
                                        "teamId": 100,
                                        "firstDargon": true,
                                        "bans": [{ "championId": 1, "pickTurn": 1 }]
                                      }
                                    ]
                                  }
                                ]
                              }
                            }
                            """;

        var response = JsonSerializer.Deserialize<MatchHistoryResponse>(json)!;
        var game = Assert.Single(response.Games.Games);
        var participant = Assert.Single(game.Participants);
        var identity = Assert.Single(game.ParticipantIdentities);
        var team = Assert.Single(game.Teams);
        var ban = Assert.Single(team.Bans);

        Assert.Equal(42, response.AccountId);
        Assert.Equal("NA1", response.PlatformId);
        Assert.Equal(1, response.Games.GameCount);
        Assert.Equal(123456, game.GameId);
        Assert.Equal(99, participant.ChampionId);
        Assert.Equal(10, participant.Stats.Kills);
        Assert.Equal(2, participant.Stats.Deaths);
        Assert.Equal(8, participant.Stats.Assists);
        Assert.True(participant.Stats.Win);
        Assert.True(participant.Stats.CausedGameEndFromIgnbSurrender);
        Assert.True(participant.Stats.GameEndedInIgnbSurrender);
        Assert.Equal("MIDDLE", participant.Timeline.Lane);
        Assert.Equal(350.5, participant.Timeline.GoldPerMinDeltas["0-10"]);
        Assert.Equal("Player", identity.Player.GameName);
        Assert.Equal("1234", identity.Player.TagLine);
        Assert.Equal("player-id", identity.Player.Puuid);
        Assert.True(team.FirstDargon);
        Assert.Equal(1, ban.ChampionId);
        Assert.Equal(1, ban.PickTurn);
    }

    [Fact]
    public void Deserialize_EmptyObjectPreservesCollectionDefaults()
    {
        var response = JsonSerializer.Deserialize<MatchHistoryResponse>("{}")!;

        Assert.NotNull(response.Games);
        Assert.Empty(response.Games.Games);
        Assert.Equal(string.Empty, response.PlatformId);
    }
}