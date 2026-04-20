using System.Diagnostics;

namespace Leagues.Models.Logging;

public static class AppLog
{
    public static event Action<string>? MessageRaised;
    public static bool IsDebugEnabled { get; set; } = false;

    public static void Logging(string message, string level = "INFO")
    {
        if (IsDebugEnabled)
        {
            Debug.WriteLine($"[{level}] {message}");
            MessageRaised?.Invoke($"[{level}] {message}");
        }
    }
}