using System.Collections.ObjectModel;

namespace Leagues.Models.Logging;

public static class Logging
{
    public static readonly ObservableCollection<string> Entries = [];
    public static readonly _Logger Logger = new();
}

public enum LogLevel
{
    Debug,
    Info,
    Error,
}

public class _Logger(LogLevel logLevel = LogLevel.Info)
{
    public void Info(string message)
    {
        if (logLevel <= LogLevel.Info)
            Logging.Entries.Add($"{DateTime.Now:HH:mm:ss} [INFO]: {message}");
    }

    public void Error(string message)
    {
        if (logLevel <= LogLevel.Error)
            Logging.Entries.Add($"{DateTime.Now:HH:mm:ss} [ERROR]: {message}");
    }

    public void Debug(string message)
    {
        if (logLevel <= LogLevel.Debug)
            Logging.Entries.Add($"{DateTime.Now:HH:mm:ss} [DEBUG]: {message}");
    }
}