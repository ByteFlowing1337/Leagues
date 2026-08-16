using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using static Leagues.Models.Logging.Logging;

namespace Leagues.Models.Utils;

public static class Registry
{
    private static readonly RegistryHive[] RegistryHives =
    {
        RegistryHive.CurrentUser,
        RegistryHive.LocalMachine
    };

    private static readonly RegistryView[] RegistryViews =
    {
        RegistryView.Registry64,
        RegistryView.Registry32
    };

    public static string? GetRiotClientPath()
    {
        var driveRoots = GetFixedDriveRoots();

        return ResolveFirstExistingPath(
            TryGetProcessExecutablePath("RiotClientServices"),
            ResolveExecutableFromRegistry(
                new[]
                {
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\RiotClientServices.exe",
                    @"SOFTWARE\Riot Games\Riot Client",
                    @"SOFTWARE\WOW6432Node\Riot Games\Riot Client"
                },
                new[] { string.Empty, "Path", "InstallPath", "InstallLocation" },
                new[] { "RiotClientServices.exe" }),
            ResolveFromUninstallRegistry(
                new[] { "Riot Client", "League of Legends" },
                new[] { "DisplayIcon", "InstallLocation", "InstallSource", "UninstallString" },
                new[] { "RiotClientServices.exe" }),
            ResolveFromKnownFolders(
                new[] { "RiotClientServices.exe" },
                GetCandidateFolders(
                    @"Riot Games\Riot Client",
                    driveRoots)));
    }

    public static string? GetWegameLOLPath()
    {
        var driveRoots = GetFixedDriveRoots();

        return ResolveFirstExistingPath(
            TryGetProcessExecutablePath("LeagueClient"),
            ResolveExecutableFromRegistry(
                new[]
                {
                    @"SOFTWARE\Tencent\LOL",
                    @"SOFTWARE\Riot Games\League of Legends",
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\League of Legends.exe"
                },
                new[] { string.Empty, "setup", "Path", "InstallPath", "InstallLocation" },
                new[] { "League of Legends.exe", "LeagueClient.exe" }),
            ResolveFromUninstallRegistry(
                new[] { "League of Legends", "LOL", "英雄联盟" },
                new[] { "DisplayIcon", "InstallLocation", "InstallSource", "UninstallString" },
                new[] { "League of Legends.exe", "LeagueClient.exe" }),
            ResolveFromKnownFolders(
                new[] { "League of Legends.exe", "LeagueClient.exe" },
                GetCandidateFolders(
                    @"Riot Games\League of Legends",
                    driveRoots)));
    }

    public static string? GetWegamePath()
    {
        var driveRoots = GetFixedDriveRoots();

        return ResolveFirstExistingPath(
            TryGetProcessExecutablePath("wegame"),
            ResolveExecutableFromRegistry(
                new[]
                {
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\wegame.exe",
                    @"SOFTWARE\Tencent\WeGame",
                    @"SOFTWARE\Tencent\TGP",
                    @"wegame\DefaultIcon"
                },
                new[] { string.Empty, "Default", "Path", "InstallPath", "InstallLocation" },
                new[] { "wegame.exe", "tgp_launcher.exe" }),
            ResolveFromUninstallRegistry(
                new[] { "WeGame", "Tencent" },
                new[] { "DisplayIcon", "InstallLocation", "InstallSource", "UninstallString" },
                new[] { "wegame.exe", "tgp_launcher.exe" }),
            ResolveFromKnownFolders(
                new[] { "wegame.exe", "tgp_launcher.exe" },
                GetCandidateFolders(
                    @"Tencent\WeGame",
                    driveRoots)));
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
            errorMessage = "Unable to resolve WeGame/Riot/LoL launcher path from registry and system sources.";
            Logger.Error(errorMessage);
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
            launchedPath = launcherPath!;
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            Logger.Error(errorMessage);
            return false;
        }
    }

    private static string? ResolveExecutableFromRegistry(
        IEnumerable<string> registryPaths,
        IEnumerable<string> valueNames,
        IEnumerable<string> executableNames)
    {
        foreach (var hive in RegistryHives)
        {
            foreach (var view in RegistryViews)
            {
                foreach (var registryPath in registryPaths)
                {
                    try
                    {
                        using var baseKey = RegistryKey.OpenBaseKey(hive, view);
                        using var key = baseKey.OpenSubKey(registryPath);
                        if (key is null)
                        {
                            continue;
                        }

                        var resolved = ResolveExecutableFromRegistryKey(key, valueNames, executableNames);
                        if (!string.IsNullOrWhiteSpace(resolved))
                        {
                            return resolved;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Registry probe failed ({hive}/{view}/{registryPath}): {ex.Message}");
                    }
                }
            }
        }

        return null;
    }

    private static string? ResolveFromUninstallRegistry(
        IEnumerable<string> displayNameKeywords,
        IEnumerable<string> valueNames,
        IEnumerable<string> executableNames)
    {
        var uninstallRoots = new[]
        {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
        };

        foreach (var hive in RegistryHives)
        {
            foreach (var view in RegistryViews)
            {
                foreach (var uninstallRoot in uninstallRoots)
                {
                    try
                    {
                        using var baseKey = RegistryKey.OpenBaseKey(hive, view);
                        using var rootKey = baseKey.OpenSubKey(uninstallRoot);
                        if (rootKey is null)
                        {
                            continue;
                        }

                        foreach (var subKeyName in rootKey.GetSubKeyNames())
                        {
                            using var appKey = rootKey.OpenSubKey(subKeyName);
                            if (appKey is null)
                            {
                                continue;
                            }

                            var displayName = appKey.GetValue("DisplayName") as string;
                            if (!ContainsAnyKeyword(displayName, displayNameKeywords))
                            {
                                continue;
                            }

                            var resolved = ResolveExecutableFromRegistryKey(appKey, valueNames, executableNames);
                            if (!string.IsNullOrWhiteSpace(resolved))
                            {
                                return resolved;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(
                            $"Uninstall registry probe failed ({hive}/{view}/{uninstallRoot}): {ex.Message}");
                    }
                }
            }
        }

        return null;
    }

    private static string? ResolveExecutableFromRegistryKey(
        RegistryKey key,
        IEnumerable<string> valueNames,
        IEnumerable<string> executableNames)
    {
        foreach (var valueName in valueNames)
        {
            var raw = key.GetValue(valueName) as string;
            var resolved = ResolveExecutableFromRawValue(raw, executableNames);
            if (!string.IsNullOrWhiteSpace(resolved))
            {
                return resolved;
            }
        }

        return null;
    }

    private static string? ResolveExecutableFromRawValue(string? rawValue, IEnumerable<string> executableNames)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return null;
        }

        var expanded = Environment.ExpandEnvironmentVariables(rawValue!.Trim());

        var directPath = NormalizeExecutablePath(expanded);
        if (!string.IsNullOrWhiteSpace(directPath) && File.Exists(directPath))
        {
            return directPath;
        }

        var cleaned = expanded.Trim().Trim('"');
        if (!Directory.Exists(cleaned))
        {
            return null;
        }

        foreach (var executableName in executableNames)
        {
            var candidate = Path.Combine(cleaned, executableName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static string? ResolveFromKnownFolders(
        IEnumerable<string> executableNames,
        IEnumerable<string> folders)
    {
        foreach (var folder in folders)
        {
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            {
                continue;
            }

            foreach (var executableName in executableNames)
            {
                var candidate = Path.Combine(folder, executableName);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        return null;
    }

    private static string[] GetCandidateFolders(string relativePath, IEnumerable<string> driveRoots)
    {
        var preferredRoots = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
        };

        var folders = new List<string>();
        foreach (var root in preferredRoots)
        {
            if (!string.IsNullOrWhiteSpace(root))
            {
                folders.Add(Path.Combine(root, relativePath));
            }
        }

        foreach (var driveRoot in driveRoots)
        {
            if (!string.IsNullOrWhiteSpace(driveRoot))
            {
                folders.Add(Path.Combine(driveRoot, relativePath));
            }
        }

        return folders
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string[] GetFixedDriveRoots()
    {
        try
        {
            return DriveInfo
                .GetDrives()
                .Where(drive => drive.DriveType == DriveType.Fixed && drive.IsReady)
                .Select(drive => drive.RootDirectory.FullName)
                .ToArray();
        }
        catch (Exception ex)
        {
            Logger.Error($"Unable to enumerate local drives: {ex.Message}");
            return Array.Empty<string>();
        }
    }

    private static string? TryGetProcessExecutablePath(string processName)
    {
        foreach (var process in Process.GetProcessesByName(processName))
        {
            using (process)
            {
                try
                {
                    var processPath = process.MainModule?.FileName;
                    if (!string.IsNullOrWhiteSpace(processPath) && File.Exists(processPath))
                    {
                        return processPath;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Unable to inspect process path for {processName}: {ex.Message}");
                }
            }
        }

        return null;
    }

    private static bool ContainsAnyKeyword(string? source, IEnumerable<string> keywords)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return false;
        }

        foreach (var keyword in keywords)
        {
            if (source.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string? ResolveFirstExistingPath(params string?[] candidates)
    {
        foreach (var candidate in candidates)
        {
            if (!string.IsNullOrWhiteSpace(candidate) && File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static string? NormalizeExecutablePath(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return null;
        }

        var expanded = Environment.ExpandEnvironmentVariables(rawValue!.Trim());

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