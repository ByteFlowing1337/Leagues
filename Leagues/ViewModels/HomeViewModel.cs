using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Leagues.Models.Client;
using Leagues.Models.Services;
using Leagues.Models.Utils;
using static Leagues.Models.Logging.Logging;

namespace Leagues.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    [ObservableProperty] public partial string StatusText { get; private set; } = "Checking Client...";

    [ObservableProperty] public partial Visibility LaunchClientVisibility { get; set; } = Visibility.Visible;

    [ObservableProperty] public partial Visibility FeatureButtonsVisibility { get; set; } = Visibility.Collapsed;

    [ObservableProperty] public partial Visibility DeclineButtonVisibility { get; set; } = Visibility.Collapsed;

    [ObservableProperty] public partial Visibility AcceptButtonVisibility { get; set; } = Visibility.Collapsed;

    [ObservableProperty] public partial string AutoAcceptButtonText { get; set; }

    [ObservableProperty] public partial bool IsAutoAcceptEnabled { get; set; } = Setting.Config?.AutoAccept ?? false;

    private volatile bool suppressNextAutoAccept;
    private volatile bool showingFeatureMode;
    public static readonly Phase PhaseMonitor = new();
    public static ObservableCollection<string> LogEntries => Entries;

    private readonly Dispatcher dispatcher;
    private readonly DispatcherTimer clientPollTimer = new() { Interval = TimeSpan.FromSeconds(2) };

    public HomeViewModel()
    {
        AutoAcceptButtonText = IsAutoAcceptEnabled ? "Disable AutoAccept" : "Enable AutoAccept";
        dispatcher = Application.Current.Dispatcher;
        PhaseMonitor.PhaseChanged += OnPhaseChanged;
        PhaseMonitor.MonitorError += OnPhaseMonitorError;
        clientPollTimer.Tick += ClientPollTimer_Tick;
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LaunchClientCommand))]
    [NotifyCanExecuteChangedFor(nameof(ToggleAutoAcceptCommand))]
    private partial bool IsClientRunning { get; set; }

    private static async Task TryAutoAcceptAsync()
    {
        var accepted = await Match.Accept();
        Logger.Info(accepted ? "Match Accepted" : "Failed to accept match");
    }

    [RelayCommand(CanExecute = nameof(CanToggleAutoAccept))]
    private async Task ToggleAutoAccept()
    {
        if (Setting.Config is not null)
        {
            Setting.Config.AutoAccept = IsAutoAcceptEnabled;
            await Setting.UpdateSetting();
        }

        AutoAcceptButtonText = IsAutoAcceptEnabled ? "Disable AutoAccept" : "Enable AutoAccept";
        Logger.Info(IsAutoAcceptEnabled ? "AutoAccept enabled" : "AutoAccept disabled");
    }

    private bool CanToggleAutoAccept() => IsClientRunning;

    [RelayCommand(CanExecute = nameof(CanLaunchClient))]
    private void LaunchClient()
    {
        if (Registry.TryLaunchClient(out var _, out var errorMessage))
        {
            Logger.Info("Client launched");
            return;
        }

        Logger.Error($"Launch failed: {errorMessage}");
    }

    private bool CanLaunchClient() => !IsClientRunning;

    [RelayCommand]
    private async Task DeclineMatchAsync()
    {
        if (IsAutoAcceptEnabled)
        {
            suppressNextAutoAccept = true;
        }

        AcceptButtonVisibility = Visibility.Visible;
        var declined = await Match.Decline();
        Logger.Info(declined ? "Match declined" : "Failed to decline match");
    }

    [RelayCommand]
    private async Task AcceptMatchAsync()
    {
        var accepted = await Match.Accept();
        Logger.Info(accepted ? "Match accepted" : "Failed to accept match");
    }

    public async Task InitializeAsync()
    {
        await RefreshUiAsync();
        clientPollTimer.Start();
    }

    public async Task ShutdownAsync()
    {
        clientPollTimer.Stop();
        PhaseMonitor.PhaseChanged -= OnPhaseChanged;
        PhaseMonitor.MonitorError -= OnPhaseMonitorError;
        await PhaseMonitor.StopAsync();
    }

    private async void ClientPollTimer_Tick(object? sender, EventArgs e)
    {
        try
        {
            await RefreshUiAsync();
        }
        catch (Exception ex)
        {
            Logger.Error($"Error during client poll: {ex.Message}");
        }
    }

    private async Task RefreshUiAsync()
    {
        IsClientRunning = Credential.IsLeagueClientRunning();

        if (!IsClientRunning)
        {
            await PhaseMonitor.StopAsync();
            ShowLaunchMode();
            return;
        }

        if (!showingFeatureMode)
            ShowFeatureMode();

        if (!PhaseMonitor.IsMonitoring)
        {
            SetStatus("Client detected, connecting to API...");
            var success = await PhaseMonitor.StartAsync();
            if (!success)
                SetStatus("Failed to connect to API.");
        }
        else
            SetStatus("Client connected.");
    }

    private void ShowLaunchMode()
    {
        showingFeatureMode = false;
        LaunchClientVisibility = Visibility.Visible;
        FeatureButtonsVisibility = Visibility.Collapsed;
        SetStatus("Client is not running.");
    }

    private void ShowFeatureMode()
    {
        showingFeatureMode = true;
        LaunchClientVisibility = Visibility.Collapsed;
        FeatureButtonsVisibility = Visibility.Visible;
    }

    private async void OnPhaseChanged(object? sender, string phase)
    {
        var isReadyCheck = string.Equals(phase, "ReadyCheck", StringComparison.OrdinalIgnoreCase);
        DeclineButtonVisibility = isReadyCheck ? Visibility.Visible : Visibility.Collapsed;

        if (!isReadyCheck || !IsAutoAcceptEnabled)
            return;

        if (suppressNextAutoAccept)
        {
            suppressNextAutoAccept = false;
            return;
        }

        try
        {
            await TryAutoAcceptAsync();
        }
        catch (Exception ex)
        {
            Logger.Error($"Error during AutoAccept: {ex.Message}");
        }
    }

    private void OnPhaseMonitorError(object? sender, string message) => SetStatus(message);

    private void SetStatus(string message) => RunOnUiThread(() => StatusText = message);

    private void RunOnUiThread(Action action)
    {
        if (dispatcher.CheckAccess())
            action();
        else
            dispatcher.Invoke(action);
    }
}