using System.Collections.ObjectModel;

namespace Leagues.Models.Logging;

public static class Logging
{
    public static Logger GetLogger()
    {
        return new Logger();
    }
}

public enum LogLevel
{
    Debug,
    Info,
    Error,
    None
}

public class Logger
{
    private LogLevel level;
    public static ObservableCollection<string> LogEntries = new();


    public Logger()
    {
        this.level = LogLevel.Info;
    }

    public void Level(LogLevel logLevel)
    {
        this.level = logLevel;
    }

    public void Info(string message)
    {
        if (level <= LogLevel.Info)
            LogEntries.Add($"{DateTime.Now:HH:mm:ss} [INFO]: {message}");
    }

    public void Error(string message)
    {
        if (level <= LogLevel.Error)
            LogEntries.Add($"{DateTime.Now:HH:mm:ss} [ERROR]: {message}");
    }

    public void Debug(string message)
    {
        if (level <= LogLevel.Debug)
            LogEntries.Add($"{DateTime.Now:HH:mm:ss} [DEBUG]: {message}");
    }
}