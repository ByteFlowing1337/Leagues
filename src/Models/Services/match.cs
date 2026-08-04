using Leagues.Models.Client;
using static Leagues.Models.Logging.Logging;
using static Leagues.Models.Client.LcuConnection;

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
}