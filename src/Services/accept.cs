using System.Net.Http;
using System.Threading;

namespace Leagues.Helper;

public static class Accept
{
    private static CancellationTokenSource? cancellationTokenSource;
    private static Thread? autoAcceptThread;
    public static void StartAutoAccept()
    {
        cancellationTokenSource = new CancellationTokenSource();
        autoAcceptThread = new Thread(() => AutoAccept(cancellationTokenSource.Token))
        {
            IsBackground = true
        };
        autoAcceptThread.Start();
    }
    public static void StopAutoAccept()
    {
        cancellationTokenSource?.Cancel();
    }

    public static void AutoAccept(CancellationToken token)
    {
        using var client = Client.GetClient();
        if (client == null)
        {
            Console.WriteLine("Failed to create HTTP client. Check credentials.");
            return;
        }

        while (!token.IsCancellationRequested)
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
            if(token.WaitHandle.WaitOne(1000))
            {
                break;
            }
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
