using Leagues.Models.Client;
using Leagues.Models.Logging;
using System;
using System.Threading.Tasks;

namespace Leagues.Models.Services;

public static class match
{
    private static Logger logger = Logging.Logging.GetLogger();

    public static async Task<bool> AcceptMatchAsync()
    {
        try
        {
            using var client = GetClient.CreateClient();
            using var response = await client.PostAsync("lol-matchmaking/v1/ready-check/accept", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.Error($"Accept request failed: {ex.Message}");
            return false;
        }
    }

    public static bool AcceptMatch()
    {
        return AcceptMatchAsync().GetAwaiter().GetResult();
    }

    public static async Task<bool> DeclineMatchAsync()
    {
        try
        {
            using var client = GetClient.CreateClient();
            using var response = await client.PostAsync("lol-matchmaking/v1/ready-check/decline", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.Error($"Decline request failed: {ex.Message}");
            return false;
        }
    }

    public static bool DeclineMatch()
    {
        return DeclineMatchAsync().GetAwaiter().GetResult();
    }
}