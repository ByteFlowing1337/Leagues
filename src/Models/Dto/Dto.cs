using System.Text.Json.Serialization;

namespace Leagues.Models.Dto;

public class GamesWrapper
{
    [JsonPropertyName("games")] public List<GameDto> Games { get; } = [];
}

public record GameDto
{
    [JsonPropertyName("gameId")] public long GameId { get; set; }
    [JsonPropertyName("gameCreation")] public long GameCreation { get; set; } // epoch ms
    [JsonPropertyName("gameDuration")] public int GameDuration { get; set; } // seconds
    [JsonPropertyName("gameMode")] public string GameMode { get; set; } = "";
    [JsonPropertyName("queueId")] public int QueueId { get; set; }
    [JsonPropertyName("participants")] public List<ParticipantDto> Participants { get; set; } = [];

    [JsonPropertyName("participantIdentities")]
    public List<ParticipantIdentityDto> ParticipantIdentities { get; set; } = [];
}

public record ParticipantDto
{
    // ranged in 1-10
    [JsonPropertyName("participantId")] public int ParticipantId { get; set; }
    [JsonPropertyName("championId")] public int ChampionId { get; set; }
    [JsonPropertyName("stats")] public StatsDto Stats { get; set; } = new();
}

public record StatsDto
{
    [JsonPropertyName("win")] public bool Win { get; set; }
    [JsonPropertyName("kills")] public int Kills { get; set; }
    [JsonPropertyName("deaths")] public int Deaths { get; set; }
    [JsonPropertyName("assists")] public int Assists { get; set; }
}

public record ParticipantIdentityDto
{
    /// <summary>
    /// ranged in 1-10
    /// </summary>
    [JsonPropertyName("participantId")]
    public int ParticipantId { get; set; }

    [JsonPropertyName("player")] public PlayerDto Player { get; set; } = new();
}

public record PlayerDto
{
    [JsonPropertyName("puuid")] public string Puuid { get; set; } = "";
    [JsonPropertyName("gameName")] public string GameName { get; set; } = "";
    [JsonPropertyName("tagLine")] public string TagLine { get; set; } = "";
}