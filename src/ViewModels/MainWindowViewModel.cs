using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Leagues.Models.Utils;
using Leagues.Models.Client;
using static Leagues.Models.Logging.Logging;
using Leagues.Models.Services;

namespace Leagues.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    public static ObservableCollection<string> LogEntries => Entries;

    private readonly Phase phaseMonitor = new();
    private readonly Dispatcher dispatcher;
    private readonly DispatcherTimer clientPollTimer = new() { Interval = TimeSpan.FromSeconds(2) };

    [ObservableProperty] private string statusText = "Checking Client...";

    [ObservableProperty] private Visibility launchClientVisibility = Visibility.Visible;

    [ObservableProperty] private Visibility featureButtonsVisibility = Visibility.Collapsed;

    [ObservableProperty] private Visibility declineButtonVisibility = Visibility.Collapsed;

    [ObservableProperty] private Visibility acceptButtonVisibility = Visibility.Collapsed;

    [ObservableProperty] private string autoAcceptButtonText = _acEnabled ? "Disable AutoAccept" : "Enable AutoAccept";

    private static bool _acEnabled = Setting.Config?.AutoAccept ?? false;
    private volatile bool suppressNextAutoAccept;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LaunchClientCommand))]
    [NotifyCanExecuteChangedFor(nameof(ToggleAutoAcceptCommand))]
    private bool isClientRunning;

    public MainWindowViewModel()
    {
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
        await RefreshUiAsync();
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

    private void OnPhaseChanged(object? sender, string phase)
    {
        var isReadyCheck = string.Equals(phase, "ReadyCheck", StringComparison.OrdinalIgnoreCase);
        DeclineButtonVisibility = isReadyCheck ? Visibility.Visible : Visibility.Collapsed;

        if (!isReadyCheck || !_acEnabled)
        {
            return;
        }

        if (suppressNextAutoAccept)
        {
            suppressNextAutoAccept = false;
            return;
        }

        _ = TryAutoAcceptAsync();
    }

    private void OnPhaseMonitorError(object? sender, string message) => SetStatus(message);

    private static async Task TryAutoAcceptAsync()
    {
        var accepted = await Match.Accept();
        Logger.Info(accepted ? "Accepted" : "Failed to accept");
    }

    [RelayCommand(CanExecute = nameof(CanToggleAutoAccept))]
    private async Task ToggleAutoAccept()
    {
        _acEnabled = !_acEnabled;
        if (Setting.Config is not null)
        {
            Setting.Config.AutoAccept = _acEnabled;
            await Setting.UpdateSetting();
        }

        AutoAcceptButtonText = _acEnabled ? "Disable AutoAccept" : "Enable AutoAccept";
        Logger.Info(_acEnabled ? "AutoAccept enabled" : "AutoAccept disabled");
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
        if (_acEnabled)
        {
            suppressNextAutoAccept = true;
        }

        AcceptButtonVisibility = Visibility.Visible;
        var declined = await Match.Decline();
        Logger.Info(declined ? "Declined match" : "Failed to decline match");
    }

    [RelayCommand]
    private async Task AcceptMatchAsync()
    {
        var accepted = await Match.Accept();
        Logger.Info(accepted ? "Accepted match" : "Failed to accept match");
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