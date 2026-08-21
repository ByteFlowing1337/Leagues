using System.Text.Json;
using Leagues.Models.Client;
using Leagues.Models.Dto;
using Leagues.Models.Services;

namespace Leagues.Models.Mapper;

public static class MatchMapper
{
    public class MatchSummary(
        long gameId,
        DateTimeOffset playedAt,
        TimeSpan duration,
        string gameMode,
        int championId,
        bool win,
        int kills,
        int deaths,
        int assists)
    {
        public long GameId { get; } = gameId;
        public DateTimeOffset PlayedAt { get; } = playedAt;
        public TimeSpan Duration { get; } = duration;
        public string GameMode { get; } = gameMode;
        public int ChampionId { get; } = championId;
        public bool Win { get; } = win;
        public int Kills { get; } = kills;
        public int Deaths { get; } = deaths;
        public int Assists { get; } = assists;
    }

    public static async Task<List<MatchSummary>?> ToSummaries(string playerName, int begIndex, int endIndex)
    {
        var json = await Match.QueryAsync(playerName, begIndex, endIndex);
        if (json == null)
            return null;

        var playerUuid = await Uuid.FetchPlayerUuid(playerName);
        if (playerUuid == null)
            return null;

        var history = JsonSerializer.Deserialize<MatchHistoryResponse>(json);
        var summaries = history!.Games.Games
            .Select(game =>
            {
                var participant = game.Participants.First();
                return new MatchSummary(
                    gameId: game.GameId,
                    playedAt: DateTimeOffset.FromUnixTimeMilliseconds(game.GameCreation),
                    duration: TimeSpan.FromSeconds(game.GameDuration),
                    gameMode: game.GameMode,
                    championId: participant!.ChampionId,
                    win: participant.Stats.Win,
                    kills: participant.Stats.Kills,
                    deaths: participant.Stats.Deaths,
                    assists: participant.Stats.Assists);
            })
            .Where(summary => summary != null)
            .ToList();
        return summaries;
    }
}