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

public sealed partial class MainWindowViewModel : ObservableObject
{
    public static ObservableCollection<string> LogEntries => Entries;

    private readonly Phase phaseMonitor = new();
    private readonly Dispatcher dispatcher;
    private readonly DispatcherTimer clientPollTimer = new() { Interval = TimeSpan.FromSeconds(2) };

    [ObservableProperty] public partial string StatusText { get; private set; } = "Checking Client...";

    [ObservableProperty] public partial Visibility LaunchClientVisibility { get; set; } = Visibility.Visible;

    [ObservableProperty] public partial Visibility FeatureButtonsVisibility { get; set; } = Visibility.Collapsed;

    [ObservableProperty] public partial Visibility DeclineButtonVisibility { get; set; } = Visibility.Collapsed;

    [ObservableProperty] public partial Visibility AcceptButtonVisibility { get; set; } = Visibility.Collapsed;

    [ObservableProperty] public partial string AutoAcceptButtonText { get; set; }

    [ObservableProperty] public partial bool IsAutoAcceptEnabled { get; set; } = Setting.Config?.AutoAccept ?? false;

    private volatile bool suppressNextAutoAccept;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LaunchClientCommand))]
    [NotifyCanExecuteChangedFor(nameof(ToggleAutoAcceptCommand))]
    private partial bool IsClientRunning { get; set; }

    public MainWindowViewModel()
    {
        AutoAcceptButtonText = IsAutoAcceptEnabled ? "Disable AutoAccept" : "Enable AutoAccept";
        dispatcher = Application.Current.Dispatcher;
        phaseMonitor.PhaseChanged += OnPhaseChanged;
        phaseMonitor.MonitorError += OnPhaseMonitorError;
        clientPollTimer.Tick += ClientPollTimer_Tick;
    }

    public async Task InitializeAsync()
    {
        Logger.Info("Initializing...");
        await RefreshUiAsync();
        clientPollTimer.Start();
    }

    public async Task ShutdownAsync()
    {
        clientPollTimer.Stop();
        phaseMonitor.PhaseChanged -= OnPhaseChanged;
        phaseMonitor.MonitorError -= OnPhaseMonitorError;
        await phaseMonitor.StopAsync();
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
            await phaseMonitor.StopAsync();
            ShowLaunchMode();
            return;
        }

        ShowFeatureMode();
        if (!phaseMonitor.IsMonitoring)
        {
            SetStatus("Client detected, connecting to API...");
            await phaseMonitor.StartAsync();
        }
        else
        {
            SetStatus("Client connected.");
        }
    }

    private void ShowLaunchMode()
    {
        LaunchClientVisibility = Visibility.Visible;
        FeatureButtonsVisibility = Visibility.Collapsed;
        SetStatus("Client is not running.");
    }

    private void ShowFeatureMode()
    {
        LaunchClientVisibility = Visibility.Collapsed;
        FeatureButtonsVisibility = Visibility.Visible;
    }

    private async void OnPhaseChanged(object? sender, string phase)
    {
        var isReadyCheck = string.Equals(phase, "ReadyCheck", StringComparison.OrdinalIgnoreCase);
        DeclineButtonVisibility = isReadyCheck ? Visibility.Visible : Visibility.Collapsed;

        if (!isReadyCheck || !IsAutoAcceptEnabled)
        {
            return;
        }

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

    private static MatchHistory? _matchHistoryWindow;

    [RelayCommand]
    private static void PopMatchHistoryWindow()
    {
        if (_matchHistoryWindow is not null)
        {
            if (_matchHistoryWindow.WindowState == WindowState.Minimized)
                _matchHistoryWindow.WindowState = WindowState.Normal;
            _matchHistoryWindow.Activate();
            return;
        }

        _matchHistoryWindow = new MatchHistory();
        _matchHistoryWindow.Closed += (_, _) => _matchHistoryWindow = null;
        _matchHistoryWindow.Show();
    }

    private void SetStatus(string message) => RunOnUiThread(() => StatusText = message);

    private void RunOnUiThread(Action action)
    {
        if (dispatcher.CheckAccess())
        {
            action();
            return;
        }

        dispatcher.Invoke(action);
    }
}