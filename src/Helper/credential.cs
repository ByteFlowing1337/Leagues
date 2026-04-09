using System.Diagnostics;
using System.Management;
using System.Text.RegularExpressions;

namespace Leagues.Helper;

public static class Credential
{
    private const string TargetProcess = "LeagueClientUx";

    public static string? GetArgs()
    {
        try
        {
            var process = Process.GetProcessesByName(TargetProcess).FirstOrDefault();
            if (process is null)
            {
                Console.WriteLine("League client is not running.");
                return null;
            }

            var commandLine = GetCommandLineWMI(process.Id);
            if (string.IsNullOrWhiteSpace(commandLine))
            {
                Console.WriteLine($"Unable to read League client command line for PID {process.Id}.");
                return null;
            }

            return commandLine;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving command line: {ex.Message}");
            return null;
        }
    }

    public static string? GetToken()
    {
        var args = GetArgs();
        return TryReadArgumentValue(args, "--remoting-auth-token=");
    }

    public static int GetPort()
    {
        var args = GetArgs();
        var portValue = TryReadArgumentValue(args, "--app-port=");

        return int.TryParse(portValue, out var port) ? port : -1;
    }

    private static string? TryReadArgumentValue(string? arguments, string argumentName)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return null;
        }

        var match = Regex.Match(arguments, Regex.Escape(argumentName) + @"([^""\s]+)");
        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        return null;
    }

    private static string? GetCommandLineWMI(int processId)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher($"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {processId}");
            using var objects = searcher.Get();
            foreach (var obj in objects)
            {
                return obj["CommandLine"]?.ToString();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WMI error: {ex.Message}");
        }
        return null;
    }
}
