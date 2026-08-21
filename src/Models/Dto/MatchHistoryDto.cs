using System.Text.Json.Serialization;

namespace Leagues.Models.Dto;

public class MatchHistoryResponse
{
    [JsonPropertyName("accountId")] public long AccountId { get; set; }

    [JsonPropertyName("games")] public GamesWrapper Games { get; set; } = new();

    [JsonPropertyName("platformId")] public string PlatformId { get; set; } = "";
}

public class GamesWrapper
{
    [JsonPropertyName("gameBeginDate")] public string GameBeginDate { get; set; } = "";

    [JsonPropertyName("gameCount")] public int GameCount { get; set; }

    [JsonPropertyName("gameEndDate")] public string GameEndDate { get; set; } = "";

    [JsonPropertyName("gameIndexBegin")] public int GameIndexBegin { get; set; }

    [JsonPropertyName("gameIndexEnd")] public int GameIndexEnd { get; set; }

    [JsonPropertyName("games")] public List<GameDto> Games { get; set; } = [];
}

public record GameDto
{
    [JsonPropertyName("endOfGameResult")] public string EndOfGameResult { get; set; } = "";

    [JsonPropertyName("gameCreation")] public long GameCreation { get; set; }

    [JsonPropertyName("gameCreationDate")] public string GameCreationDate { get; set; } = "";

    [JsonPropertyName("gameDuration")] public int GameDuration { get; set; }

    [JsonPropertyName("gameId")] public long GameId { get; set; }

    [JsonPropertyName("gameMode")] public string GameMode { get; set; } = "";

    [JsonPropertyName("gameModeMutators")] public List<string> GameModeMutators { get; set; } = [];

    [JsonPropertyName("gameType")] public string GameType { get; set; } = "";

    [JsonPropertyName("gameVersion")] public string GameVersion { get; set; } = "";

    [JsonPropertyName("mapId")] public int MapId { get; set; }

    [JsonPropertyName("participantIdentities")]
    public List<ParticipantIdentityDto> ParticipantIdentities { get; set; } = [];

    /// <summary>
    /// Contains only the participant corresponding to the queried summoner
    /// in this Match History response.
    /// </summary>
    [JsonPropertyName("participants")]
    public List<ParticipantDto> Participants { get; set; } = [];

    [JsonPropertyName("platformId")] public string PlatformId { get; set; } = "";

    [JsonPropertyName("queueId")] public int QueueId { get; set; }

    [JsonPropertyName("seasonId")] public int SeasonId { get; set; }

    [JsonPropertyName("teams")] public List<TeamDto> Teams { get; set; } = [];
}

public record ParticipantDto
{
    [JsonPropertyName("championId")] public int ChampionId { get; set; }

    [JsonPropertyName("highestAchievedSeasonTier")]
    public string HighestAchievedSeasonTier { get; set; } = "";

    [JsonPropertyName("participantId")] public int ParticipantId { get; set; }

    [JsonPropertyName("spell1Id")] public int Spell1Id { get; set; }

    [JsonPropertyName("spell2Id")] public int Spell2Id { get; set; }

    [JsonPropertyName("stats")] public StatsDto Stats { get; set; } = new();

    [JsonPropertyName("teamId")] public int TeamId { get; set; }

    [JsonPropertyName("timeline")] public TimelineDto Timeline { get; set; } = new();
}

public record StatsDto
{
    [JsonPropertyName("assists")] public int Assists { get; set; }

    [JsonPropertyName("causedEarlySurrender")]
    public bool CausedEarlySurrender { get; set; }

    [JsonPropertyName("causedGameEndFromIGNBSurrender")]
    public bool CausedGameEndFromIgnbSurrender { get; set; }

    [JsonPropertyName("champLevel")] public int ChampLevel { get; set; }

    [JsonPropertyName("combatPlayerScore")]
    public int CombatPlayerScore { get; set; }

    [JsonPropertyName("damageDealtToObjectives")]
    public int DamageDealtToObjectives { get; set; }

    [JsonPropertyName("damageDealtToTurrets")]
    public int DamageDealtToTurrets { get; set; }

    [JsonPropertyName("damageSelfMitigated")]
    public int DamageSelfMitigated { get; set; }

    [JsonPropertyName("deaths")] public int Deaths { get; set; }

    [JsonPropertyName("doubleKills")] public int DoubleKills { get; set; }

    [JsonPropertyName("earlySurrenderAccomplice")]
    public bool EarlySurrenderAccomplice { get; set; }

    [JsonPropertyName("firstBloodAssist")] public bool FirstBloodAssist { get; set; }

    [JsonPropertyName("firstBloodKill")] public bool FirstBloodKill { get; set; }

    [JsonPropertyName("firstInhibitorAssist")]
    public bool FirstInhibitorAssist { get; set; }

    [JsonPropertyName("firstInhibitorKill")]
    public bool FirstInhibitorKill { get; set; }

    [JsonPropertyName("firstTowerAssist")] public bool FirstTowerAssist { get; set; }

    [JsonPropertyName("firstTowerKill")] public bool FirstTowerKill { get; set; }

    [JsonPropertyName("gameEndedInEarlySurrender")]
    public bool GameEndedInEarlySurrender { get; set; }

    [JsonPropertyName("gameEndedInIGNBSurrender")]
    public bool GameEndedInIgnbSurrender { get; set; }

    [JsonPropertyName("gameEndedInSurrender")]
    public bool GameEndedInSurrender { get; set; }

    [JsonPropertyName("goldEarned")] public int GoldEarned { get; set; }

    [JsonPropertyName("goldSpent")] public int GoldSpent { get; set; }

    [JsonPropertyName("inhibitorKills")] public int InhibitorKills { get; set; }

    [JsonPropertyName("item0")] public int Item0 { get; set; }

    [JsonPropertyName("item1")] public int Item1 { get; set; }

    [JsonPropertyName("item2")] public int Item2 { get; set; }

    [JsonPropertyName("item3")] public int Item3 { get; set; }

    [JsonPropertyName("item4")] public int Item4 { get; set; }

    [JsonPropertyName("item5")] public int Item5 { get; set; }

    [JsonPropertyName("item6")] public int Item6 { get; set; }

    [JsonPropertyName("killingSprees")] public int KillingSprees { get; set; }

    [JsonPropertyName("kills")] public int Kills { get; set; }

    [JsonPropertyName("largestCriticalStrike")]
    public int LargestCriticalStrike { get; set; }

    [JsonPropertyName("largestKillingSpree")]
    public int LargestKillingSpree { get; set; }

    [JsonPropertyName("largestMultiKill")] public int LargestMultiKill { get; set; }

    [JsonPropertyName("longestTimeSpentLiving")]
    public int LongestTimeSpentLiving { get; set; }

    [JsonPropertyName("magicDamageDealt")] public int MagicDamageDealt { get; set; }

    [JsonPropertyName("magicDamageDealtToChampions")]
    public int MagicDamageDealtToChampions { get; set; }

    [JsonPropertyName("magicalDamageTaken")]
    public int MagicalDamageTaken { get; set; }

    [JsonPropertyName("neutralMinionsKilled")]
    public int NeutralMinionsKilled { get; set; }

    [JsonPropertyName("neutralMinionsKilledEnemyJungle")]
    public int NeutralMinionsKilledEnemyJungle { get; set; }

    [JsonPropertyName("neutralMinionsKilledTeamJungle")]
    public int NeutralMinionsKilledTeamJungle { get; set; }

    [JsonPropertyName("objectivePlayerScore")]
    public int ObjectivePlayerScore { get; set; }

    [JsonPropertyName("participantId")] public int ParticipantId { get; set; }

    [JsonPropertyName("pentaKills")] public int PentaKills { get; set; }

    [JsonPropertyName("perk0")] public int Perk0 { get; set; }

    [JsonPropertyName("perk0Var1")] public int Perk0Var1 { get; set; }

    [JsonPropertyName("perk0Var2")] public int Perk0Var2 { get; set; }

    [JsonPropertyName("perk0Var3")] public int Perk0Var3 { get; set; }

    [JsonPropertyName("perk1")] public int Perk1 { get; set; }

    [JsonPropertyName("perk1Var1")] public int Perk1Var1 { get; set; }

    [JsonPropertyName("perk1Var2")] public int Perk1Var2 { get; set; }

    [JsonPropertyName("perk1Var3")] public int Perk1Var3 { get; set; }

    [JsonPropertyName("perk2")] public int Perk2 { get; set; }

    [JsonPropertyName("perk2Var1")] public int Perk2Var1 { get; set; }

    [JsonPropertyName("perk2Var2")] public int Perk2Var2 { get; set; }

    [JsonPropertyName("perk2Var3")] public int Perk2Var3 { get; set; }

    [JsonPropertyName("perk3")] public int Perk3 { get; set; }

    [JsonPropertyName("perk3Var1")] public int Perk3Var1 { get; set; }

    [JsonPropertyName("perk3Var2")] public int Perk3Var2 { get; set; }

    [JsonPropertyName("perk3Var3")] public int Perk3Var3 { get; set; }

    [JsonPropertyName("perk4")] public int Perk4 { get; set; }

    [JsonPropertyName("perk4Var1")] public int Perk4Var1 { get; set; }

    [JsonPropertyName("perk4Var2")] public int Perk4Var2 { get; set; }

    [JsonPropertyName("perk4Var3")] public int Perk4Var3 { get; set; }

    [JsonPropertyName("perk5")] public int Perk5 { get; set; }

    [JsonPropertyName("perk5Var1")] public int Perk5Var1 { get; set; }

    [JsonPropertyName("perk5Var2")] public int Perk5Var2 { get; set; }

    [JsonPropertyName("perk5Var3")] public int Perk5Var3 { get; set; }

    [JsonPropertyName("perkPrimaryStyle")] public int PerkPrimaryStyle { get; set; }

    [JsonPropertyName("perkSubStyle")] public int PerkSubStyle { get; set; }

    [JsonPropertyName("physicalDamageDealt")]
    public int PhysicalDamageDealt { get; set; }

    [JsonPropertyName("physicalDamageDealtToChampions")]
    public int PhysicalDamageDealtToChampions { get; set; }

    [JsonPropertyName("physicalDamageTaken")]
    public int PhysicalDamageTaken { get; set; }

    [JsonPropertyName("playerAugment1")] public int PlayerAugment1 { get; set; }

    [JsonPropertyName("playerAugment2")] public int PlayerAugment2 { get; set; }

    [JsonPropertyName("playerAugment3")] public int PlayerAugment3 { get; set; }

    [JsonPropertyName("playerAugment4")] public int PlayerAugment4 { get; set; }

    [JsonPropertyName("playerAugment5")] public int PlayerAugment5 { get; set; }

    [JsonPropertyName("playerAugment6")] public int PlayerAugment6 { get; set; }

    [JsonPropertyName("playerScore0")] public int PlayerScore0 { get; set; }

    [JsonPropertyName("playerScore1")] public int PlayerScore1 { get; set; }

    [JsonPropertyName("playerScore2")] public int PlayerScore2 { get; set; }

    [JsonPropertyName("playerScore3")] public int PlayerScore3 { get; set; }

    [JsonPropertyName("playerScore4")] public int PlayerScore4 { get; set; }

    [JsonPropertyName("playerScore5")] public int PlayerScore5 { get; set; }

    [JsonPropertyName("playerScore6")] public int PlayerScore6 { get; set; }

    [JsonPropertyName("playerScore7")] public int PlayerScore7 { get; set; }

    [JsonPropertyName("playerScore8")] public int PlayerScore8 { get; set; }

    [JsonPropertyName("playerScore9")] public int PlayerScore9 { get; set; }

    [JsonPropertyName("playerSubteamId")] public int PlayerSubteamId { get; set; }

    [JsonPropertyName("quadraKills")] public int QuadraKills { get; set; }

    [JsonPropertyName("roleBoundItem")] public int RoleBoundItem { get; set; }

    [JsonPropertyName("sightWardsBoughtInGame")]
    public int SightWardsBoughtInGame { get; set; }

    [JsonPropertyName("subteamPlacement")] public int SubteamPlacement { get; set; }

    [JsonPropertyName("teamEarlySurrendered")]
    public bool TeamEarlySurrendered { get; set; }

    [JsonPropertyName("timeCCingOthers")] public int TimeCCingOthers { get; set; }

    [JsonPropertyName("totalDamageDealt")] public int TotalDamageDealt { get; set; }

    [JsonPropertyName("totalDamageDealtToChampions")]
    public int TotalDamageDealtToChampions { get; set; }

    [JsonPropertyName("totalDamageTaken")] public int TotalDamageTaken { get; set; }

    [JsonPropertyName("totalHeal")] public int TotalHeal { get; set; }

    [JsonPropertyName("totalMinionsKilled")]
    public int TotalMinionsKilled { get; set; }

    [JsonPropertyName("totalPlayerScore")] public int TotalPlayerScore { get; set; }

    [JsonPropertyName("totalScoreRank")] public int TotalScoreRank { get; set; }

    [JsonPropertyName("totalTimeCrowdControlDealt")]
    public int TotalTimeCrowdControlDealt { get; set; }

    [JsonPropertyName("totalUnitsHealed")] public int TotalUnitsHealed { get; set; }

    [JsonPropertyName("tripleKills")] public int TripleKills { get; set; }

    [JsonPropertyName("trueDamageDealt")] public int TrueDamageDealt { get; set; }

    [JsonPropertyName("trueDamageDealtToChampions")]
    public int TrueDamageDealtToChampions { get; set; }

    [JsonPropertyName("trueDamageTaken")] public int TrueDamageTaken { get; set; }

    [JsonPropertyName("turretKills")] public int TurretKills { get; set; }

    [JsonPropertyName("unrealKills")] public int UnrealKills { get; set; }

    [JsonPropertyName("visionScore")] public int VisionScore { get; set; }

    [JsonPropertyName("visionWardsBoughtInGame")]
    public int VisionWardsBoughtInGame { get; set; }

    [JsonPropertyName("wardsKilled")] public int WardsKilled { get; set; }

    [JsonPropertyName("wardsPlaced")] public int WardsPlaced { get; set; }

    [JsonPropertyName("wasSevereTransgressor")]
    public bool WasSevereTransgressor { get; set; }

    [JsonPropertyName("win")] public bool Win { get; set; }
}

public record TimelineDto
{
    [JsonPropertyName("creepsPerMinDeltas")]
    public Dictionary<string, double> CreepsPerMinDeltas { get; set; } = [];

    [JsonPropertyName("csDiffPerMinDeltas")]
    public Dictionary<string, double> CsDiffPerMinDeltas { get; set; } = [];

    [JsonPropertyName("damageTakenDiffPerMinDeltas")]
    public Dictionary<string, double> DamageTakenDiffPerMinDeltas { get; set; } = [];

    [JsonPropertyName("damageTakenPerMinDeltas")]
    public Dictionary<string, double> DamageTakenPerMinDeltas { get; set; } = [];

    [JsonPropertyName("goldPerMinDeltas")] public Dictionary<string, double> GoldPerMinDeltas { get; set; } = [];

    [JsonPropertyName("lane")] public string Lane { get; set; } = "";

    [JsonPropertyName("participantId")] public int ParticipantId { get; set; }

    [JsonPropertyName("role")] public string Role { get; set; } = "";

    [JsonPropertyName("xpDiffPerMinDeltas")]
    public Dictionary<string, double> XpDiffPerMinDeltas { get; set; } = [];

    [JsonPropertyName("xpPerMinDeltas")] public Dictionary<string, double> XpPerMinDeltas { get; set; } = [];
}

public record ParticipantIdentityDto
{
    [JsonPropertyName("participantId")] public int ParticipantId { get; set; }

    [JsonPropertyName("player")] public PlayerDto Player { get; set; } = new();
}

public record PlayerDto
{
    [JsonPropertyName("accountId")] public long AccountId { get; set; }

    [JsonPropertyName("currentAccountId")] public long CurrentAccountId { get; set; }

    [JsonPropertyName("currentPlatformId")]
    public string CurrentPlatformId { get; set; } = "";

    [JsonPropertyName("gameName")] public string GameName { get; set; } = "";

    [JsonPropertyName("matchHistoryUri")] public string MatchHistoryUri { get; set; } = "";

    [JsonPropertyName("platformId")] public string PlatformId { get; set; } = "";

    [JsonPropertyName("profileIcon")] public int ProfileIcon { get; set; }

    [JsonPropertyName("puuid")] public string Puuid { get; set; } = "";

    [JsonPropertyName("summonerId")] public long SummonerId { get; set; }

    [JsonPropertyName("summonerName")] public string SummonerName { get; set; } = "";

    [JsonPropertyName("tagLine")] public string TagLine { get; set; } = "";
}

public record TeamDto
{
    [JsonPropertyName("bans")] public List<BanDto> Bans { get; set; } = [];

    [JsonPropertyName("baronKills")] public int BaronKills { get; set; }

    [JsonPropertyName("dominionVictoryScore")]
    public int DominionVictoryScore { get; set; }

    [JsonPropertyName("dragonKills")] public int DragonKills { get; set; }

    [JsonPropertyName("firstBaron")] public bool FirstBaron { get; set; }

    [JsonPropertyName("firstBlood")] public bool FirstBlood { get; set; }

    [JsonPropertyName("firstDargon")] public bool FirstDargon { get; set; }

    [JsonPropertyName("firstInhibitor")] public bool FirstInhibitor { get; set; }

    [JsonPropertyName("firstTower")] public bool FirstTower { get; set; }

    [JsonPropertyName("hordeKills")] public int HordeKills { get; set; }

    [JsonPropertyName("inhibitorKills")] public int InhibitorKills { get; set; }

    [JsonPropertyName("riftHeraldKills")] public int RiftHeraldKills { get; set; }

    [JsonPropertyName("teamId")] public int TeamId { get; set; }

    [JsonPropertyName("towerKills")] public int TowerKills { get; set; }

    [JsonPropertyName("vilemawKills")] public int VilemawKills { get; set; }

    [JsonPropertyName("win")] public string Win { get; set; } = "";
}

public record BanDto
{
    [JsonPropertyName("championId")] public int ChampionId { get; set; }

    [JsonPropertyName("pickTurn")] public int PickTurn { get; set; }
}