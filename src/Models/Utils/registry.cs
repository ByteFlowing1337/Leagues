using Microsoft.Win32;
using Leagues.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
namespace Leagues.Utils;

public static class ReadRegistry
{

    public static string? GetRiotClientPath()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Riot Games\Riot Client");
            var raw = key?.GetValue("Path") as string;
            return NormalizeExecutablePath(raw);
        }
        catch (Exception ex)
        {
            AppLog.Logging($"Error accessing registry: {ex.Message}");
            return null;
        }
    }

    public static string? GetWegameLOLPath()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Tencent\LOL");
            var raw = key?.GetValue("setup") as string;
            var setupPath = NormalizeExecutablePath(raw);

            if (!string.IsNullOrWhiteSpace(setupPath))
            {
                return setupPath;
            }

            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            var expanded = Environment.ExpandEnvironmentVariables(raw.Trim().Trim('"'));
            if (Directory.Exists(expanded))
            {
                var candidate = Path.Combine(expanded, "League of Legends.exe");
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            AppLog.Logging($"Error accessing registry: {ex.Message}");
            return null;
        }
    }

    public static string? GetWegamePath()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"wegame\DefaultIcon");
            var raw = key?.GetValue("Default") as string;
            return NormalizeExecutablePath(raw);
        }
        catch (Exception ex)
        {
            AppLog.Logging($"Error accessing registry: {ex.Message}");
            return null;
        }
    }

    public static string? GetPreferredLauncherPath()
    {
        var wegamePath = GetWegamePath();
        if (!string.IsNullOrWhiteSpace(wegamePath) && File.Exists(wegamePath))
        {
            return wegamePath;
        }

        var riotPath = GetRiotClientPath();
        if (!string.IsNullOrWhiteSpace(riotPath) && File.Exists(riotPath))
        {
            return riotPath;
        }

        var lolPath = GetWegameLOLPath();
        if (!string.IsNullOrWhiteSpace(lolPath) && File.Exists(lolPath))
        {
            return lolPath;
        }

        return null;
    }

    public static bool TryLaunchClient(out string launchedPath, out string errorMessage)
    {
        launchedPath = string.Empty;
        errorMessage = string.Empty;

        var launcherPath = GetPreferredLauncherPath();
        if (string.IsNullOrWhiteSpace(launcherPath))
        {
            errorMessage = "Unable to resolve WeGame/Riot/LoL client path from registry.";
            return false;
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = launcherPath,
                UseShellExecute = true
            };

            if (launcherPath.Contains("RiotClientServices.exe", StringComparison.OrdinalIgnoreCase))
            {
                startInfo.Arguments = "--launch-product=league_of_legends --launch-patchline=live";
            }

            Process.Start(startInfo);
            launchedPath = launcherPath;
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }

    private static string? NormalizeExecutablePath(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return null;
        }

        var expanded = Environment.ExpandEnvironmentVariables(rawValue.Trim());

        var quotedMatch = Regex.Match(expanded, "\"(?<path>[^\"]+?\\.exe)\"", RegexOptions.IgnoreCase);
        if (quotedMatch.Success)
        {
            return quotedMatch.Groups["path"].Value;
        }

        var exeMatch = Regex.Match(expanded, @"(?<path>[A-Za-z]:\\[^,]+?\.exe)", RegexOptions.IgnoreCase);
        if (exeMatch.Success)
        {
            return exeMatch.Groups["path"].Value;
        }

        var cleaned = expanded.Trim('"');
        return cleaned.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? cleaned : null;
    }
}