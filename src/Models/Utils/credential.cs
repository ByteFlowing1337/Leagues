using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Leagues.Logging;
using Leagues.Services;

namespace Leagues.Models.Utils;

public static class Credential
{
    private const string TargetProcess = "LeagueClientUx";
    private const uint ProcessQueryLimitedInformation = 0x1000;
    private const int ProcessCommandLineInformationClass = 60;
    private const uint NtStatusSuccess = 0x00000000;
    private const uint NtStatusInfoLengthMismatch = 0xC0000004;

    public static bool IsLeagueClientRunning()
    {
        return Process.GetProcessesByName(TargetProcess).Length > 0;
    }

    public static string? GetArgs()
    {
        string? fallbackArgs = null;

        foreach (var process in Process.GetProcessesByName(TargetProcess))
        {
            using (process)
            {
                var commandLine = TryGetProcessCommandLine(process.Id);
                if (string.IsNullOrWhiteSpace(commandLine))
                {
                    continue;
                }

                // Prefer the process instance that already exposes both required args.
                var hasToken = TryReadArgumentValue(commandLine, "--remoting-auth-token=") is not null;
                var hasPort = TryReadArgumentValue(commandLine, "--app-port=") is not null;

                if (hasToken && hasPort)
                {
                    return commandLine;
                }

                fallbackArgs ??= commandLine;
            }
        }

        if (!string.IsNullOrWhiteSpace(fallbackArgs))
        {
            AppLog.Logging("League client command line found, but required arguments are incomplete.");
            return fallbackArgs;
        }

        AppLog.Logging("League client is not running or command line is unavailable.");
        return null;
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

    private static string? TryGetProcessCommandLine(int processId)
    {
        var processHandle = OpenProcess(ProcessQueryLimitedInformation, false, processId);
        if (processHandle == IntPtr.Zero)
        {
            return null;
        }

        try
        {
            var queryStatus = NtQueryInformationProcess(
                processHandle,
                ProcessCommandLineInformationClass,
                IntPtr.Zero,
                0,
                out var requiredLength);

            if (queryStatus != NtStatusInfoLengthMismatch && queryStatus != NtStatusSuccess)
            {
                return null;
            }

            if (requiredLength <= 0)
            {
                return null;
            }

            var buffer = Marshal.AllocHGlobal(requiredLength);
            try
            {
                var status = NtQueryInformationProcess(
                    processHandle,
                    ProcessCommandLineInformationClass,
                    buffer,
                    requiredLength,
                    out _);

                if (status != NtStatusSuccess)
                {
                    return null;
                }

                var unicodeString = Marshal.PtrToStructure<UnicodeString>(buffer);
                if (unicodeString.Length == 0 || unicodeString.Buffer == IntPtr.Zero)
                {
                    return null;
                }

                return Marshal.PtrToStringUni(unicodeString.Buffer, unicodeString.Length / 2);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        catch (Exception)
        {
            return null;
        }
        finally
        {
            _ = CloseHandle(processHandle);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct UnicodeString
    {
        public ushort Length;
        public ushort MaximumLength;
        public IntPtr Buffer;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(uint processAccess, bool inheritHandle, int processId);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr handle);

    [DllImport("ntdll.dll")]
    private static extern uint NtQueryInformationProcess(
        IntPtr processHandle,
        int processInformationClass,
        IntPtr processInformation,
        int processInformationLength,
        out int returnLength);
}
