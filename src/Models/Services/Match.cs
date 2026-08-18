using Leagues.Models.Client;
using static Leagues.Models.Client.LcuConnection;
using static Leagues.Models.Logging.Logging;

namespace Leagues.Models.Services;

public static class Match
{
    public static async Task<bool> Accept()
    {
        try
        {
            using var response = await LcuHttpClient.PostAsync("lol-matchmaking/v1/ready-check/accept", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Logger.Error($"Accept request failed: {ex.Message}");
            return false;
        }
    }

    public static async Task<bool> Decline()
    {
        try
        {
            using var response = await LcuHttpClient.PostAsync("lol-matchmaking/v1/ready-check/decline", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Logger.Error($"Decline request failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Query the match history of a player using their player ID
    /// and the specified range of matches (begIndex to endIndex).
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="begIndex"></param>
    /// <param name="endIndex"></param>
    public static async Task<string?> QueryAsync(string playerId, int begIndex, int endIndex)
    {
        var playerUuid = await Uuid.FetchPlayerUuid(playerId);
        if (playerUuid == null)
            return null;

        var response = await LcuHttpClient.GetAsync(LcuEndPoint.MatchHistory(playerUuid, begIndex, endIndex));
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadAsStringAsync();

        Logger.Error($"Failed to query match history for player {playerId}: {response.ReasonPhrase}");
        return null;
    }
}