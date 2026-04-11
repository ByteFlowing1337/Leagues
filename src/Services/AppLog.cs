using System.Diagnostics;

namespace Leagues.Services;

public static class AppLog
{
    public static event Action<string>? MessageRaised;

    public static void Info(string message)
    {
        Debug.WriteLine(message);
        MessageRaised?.Invoke(message);
    }
}