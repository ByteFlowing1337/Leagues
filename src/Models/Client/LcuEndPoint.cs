using System.Globalization;
using System.Text;

namespace Leagues.Models.Client;

public static class LcuEndPoint
{
    /// <summary>
    /// Returns the endpoint for fetching a summoner's information by their player name.
    /// </summary>
    /// <param name="playerName"></param>
    /// <returns></returns>
    public static string Summoner(string playerName)
    {
        // The player name copied from LOL client contains special chars,
        // [U+2066] Name [U+2069] # [U+2066] num [U+2069]
        var cleanedPlayerName = new string([
            .. playerName
                .Where(c =>
                {
                    var cat = CharUnicodeInfo.GetUnicodeCategory(c);
                    return cat != UnicodeCategory.Format && cat != UnicodeCategory.Control;
                })
        ]);

        cleanedPlayerName.Trim().Normalize(NormalizationForm.FormC);
        return $"lol-summoner/v1/summoners?name={Uri.EscapeDataString(cleanedPlayerName)}";
    }

    public static string CurrentSummoner
        => "lol-summoner/v1/current-summoner";

    public static string MatchHistory(string uuid, int begIndex = 0, int endIndex = 20)
        => $"lol-match-history/v1/products/lol/{uuid}/matches?begIndex={begIndex}&endIndex={endIndex}";

    public static string GameDetails(long gameId)
        => $"lol-match-history/v1/games/{gameId}";

    public static string ChampionAvatar(int championId)
        => $"lol-game-data/assets/v1/champion-icons/{championId}.png";
}