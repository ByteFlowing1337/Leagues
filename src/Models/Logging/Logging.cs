using System.Collections.ObjectModel;
using System.Windows;

namespace Leagues.Models.Logging;

public static class Logging
{
    public static readonly ObservableCollection<string> Entries = [];
    public static readonly Logger Logger = new();
}

public enum LogLevel
{
    Debug,
    Info,
    Error,
}

public class Logger(LogLevel logLevel = LogLevel.Info)
{
    public void Info(string message)
    {
        if (logLevel <= LogLevel.Info)
            Log(message, "INFO");
    }

    public void Error(string message)
    {
        if (logLevel <= LogLevel.Error)
            Log(message, "ERROR");
    }

    public void Debug(string message)
    {
        if (logLevel <= LogLevel.Debug)
            Log(message, "DEBUG");
    }

    private static void Log(string message, string level)
    {
        // For thread affinity, we need to use the Dispatcher to update the ObservableCollection from a non-UI thread.:
        // https://stackoverflow.com/questions/18331723
        Application.Current.Dispatcher.Invoke((Action)delegate
        {
            Logging.Entries.Add($"{DateTime.Now:HH:m:ss} {level}: {message}");
        });
    }
}