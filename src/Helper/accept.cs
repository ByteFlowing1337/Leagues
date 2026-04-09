using System.Net.Http;

namespace Leagues.Helper;

public static class Accept
{
    public static void StartAutoAccept()
    {
        var autoAcceptThread = new Thread(AutoAccept)
        {
            IsBackground = true
        };
        autoAcceptThread.Start();
    }

    public static void AutoAccept()
    {
        using var client = Client.GetClient();
        if (client == null)
        {
            Console.WriteLine("Failed to create HTTP client. Check credentials.");
            return;
        }

        while (true)
        {
            var status = GetMatchStatus(client);
            if (status != null && status.Contains("\"state\":\"InProgress\"", StringComparison.Ordinal))
            {
                Console.WriteLine("Match found! Attempting to accept...");
                var response = client.PostAsync("lol-matchmaking/v1/ready-check/accept", null).Result;
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Successfully accepted the match.");
                    break;
                }

                Console.WriteLine($"Failed to accept the match. Status code: {response.StatusCode}");
                break;
            }

            Thread.Sleep(1000);
        }
    }

    public static string? GetMatchStatus(HttpClient client)
    {
        var response = client.GetAsync("lol-matchmaking/v1/ready-check").Result;
        if (response.IsSuccessStatusCode)
        {
            var content = response.Content.ReadAsStringAsync().Result;
            return content;
        }

        Console.WriteLine($"Failed to get match status. Status code: {response.StatusCode}");
        return null;
    }
}
