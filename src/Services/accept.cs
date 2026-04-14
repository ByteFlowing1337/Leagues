using Leagues.Models.Utils;
using Leagues.Logging;

namespace Leagues.Services;

public static class Accept
{
    public static bool IsAutoAcceptEnabled { get; private set; }

    public static void StartAutoAccept()
    {
        IsAutoAcceptEnabled = true;
    }

    public static void StopAutoAccept()
    {
        IsAutoAcceptEnabled = false;
    }

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
            AppLog.Logging($"Accept request failed: {ex.Message}");
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
            AppLog.Logging($"Decline request failed: {ex.Message}");
            return false;
        }
    }

    public static bool DeclineMatch()
    {
        return DeclineMatchAsync().GetAwaiter().GetResult();
    }
}