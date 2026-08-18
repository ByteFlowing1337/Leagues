using System.Text.Json;
using System.Text.Json.Serialization;
using Leagues.Models.Client;
using Leagues.Models.Dto;
using Leagues.Models.Logging;
using Leagues.Models.Services;

namespace Leagues.Models.MatchMapper;

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

    public class MatchHistoryResponse
    {
        [JsonPropertyName("games")] public GamesWrapper Games { get; } = new();
    }

    public static async Task<List<MatchSummary>?> ToSummaries(string playerName, uint begIndex, uint endIndex)
    {
        var json = await Match.QueryAsync(playerName, 0, 20);
        if (json == null)
        {
            return null;
        }

        var playerUuid = await Uuid.FetchPlayerUuid(playerName);
        if (playerUuid == null)
        {
            return null;
        }

        var history = JsonSerializer.Deserialize<MatchHistoryResponse>(json);
        var summaries = history!.Games.Games
            .Select(game =>
            {
                // First, find the participant identity by puuid for the participant id (1-10)
                var identity = game.ParticipantIdentities
                    .FirstOrDefault(pi => pi.Player.Puuid == playerUuid);
                if (identity == null) return null;

                // Then, find the specific participant with the participant id
                var participant = game.Participants
                    .FirstOrDefault(p => p.ParticipantId == identity.ParticipantId);
                if (participant == null) return null;

                // Finally, summary the match using the participant stats
                // and other details
                return new MatchSummary(
                    gameId: game.GameId,
                    playedAt: DateTimeOffset.FromUnixTimeMilliseconds(game.GameCreation),
                    duration: TimeSpan.FromSeconds(game.GameDuration),
                    gameMode: game.GameMode,
                    championId: participant.ChampionId,
                    win: participant.Stats.Win,
                    kills: participant.Stats.Kills,
                    deaths: participant.Stats.Deaths,
                    assists: participant.Stats.Assists);
            })
            .Where(summary => summary != null)
            .ToList();
        return summaries!;
    }
}