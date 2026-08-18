using System.Text.Json;
using static Leagues.Models.Logging.Logging;
using static Leagues.Models.Client.LcuConnection;

namespace Leagues.Models.Client;

public static class Uuid
{
    /// <summary>
    /// Fetches the UUID of a player using their player ID in format "ABC#12345"
    /// </summary>
    /// <returns>UUID</returns>
    public static async Task<string?> FetchPlayerUuid(string playerId)
    {
        try
        {
            using var response = await LcuHttpClient.GetAsync(LcuEndPoint.Summoner(playerId));
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Logger.Error($"Summoner fetch failed ({(int)response.StatusCode}): {content}");
                return null;
            }

            using var doc = JsonDocument.Parse(content);
            if (!doc.RootElement.TryGetProperty("puuid", out var puuidProp))
            {
                Logger.Error($"No puuid in response: {content}");
                return null;
            }

            return puuidProp.GetString();
        }
        catch (Exception ex)
        {
            Logger.Error($"Error fetching player uuid: {ex.Message}");
            return null;
        }
    }
}