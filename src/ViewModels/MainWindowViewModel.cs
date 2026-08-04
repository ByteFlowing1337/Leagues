using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Leagues.Models.Utils;
using Leagues.Models.Client;
using static Leagues.Models.Logging.Logging;
using Leagues.Models.Services;

namespace Leagues.ViewModels;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    public ObservableCollection<string> LogEntries => Entries;
    private readonly Phase phaseMonitor = new();
    private readonly Dispatcher dispatcher;
    private readonly DispatcherTimer clientPollTimer = new() { Interval = TimeSpan.FromSeconds(2) };

    private string statusText = "Checking Client...";
    private Visibility launchClientVisibility = Visibility.Visible;
    private Visibility featureButtonsVisibility = Visibility.Collapsed;
    private string autoAcceptButtonText = "Enable AutoAccept";
    private bool acEnabled;
    private volatile bool suppressNextAutoAccept;

    public MainWindowViewModel()
    {
        dispatcher = Application.Current.Dispatcher;

        LaunchClientCommand = new RelayCommand(LaunchClient, () => !Credential.IsLeagueClientRunning());
        ToggleAutoAcceptCommand = new RelayCommand(ToggleAutoAccept, Credential.IsLeagueClientRunning);
        DeclineMatchCommand = new AsyncRelayCommand(DeclineMatchAsync);

        phaseMonitor.PhaseChanged += PhaseChanged;
        phaseMonitor.MonitorError += OnPhaseMonitorError;
        clientPollTimer.Tick += ClientPollTimer_Tick;
    }

    public event PropertyChangedEventHandler? PropertyChanged;


    public string StatusText
    {
        get => statusText;
        private set => SetField(ref statusText, value);
    }

    public Visibility LaunchClientVisibility
    {
        get => launchClientVisibility;
        private set => SetField(ref launchClientVisibility, value);
    }

    public Visibility FeatureButtonsVisibility
    {
        get => featureButtonsVisibility;
        private set => SetField(ref featureButtonsVisibility, value);
    }


    public string AutoAcceptButtonText
    {
        get => autoAcceptButtonText;
        private set => SetField(ref autoAcceptButtonText, value);
    }

    public ICommand LaunchClientCommand { get; }
    public ICommand ToggleAutoAcceptCommand { get; }
    public ICommand DeclineMatchCommand { get; }

    public async Task InitializeAsync()
    {
        Logger.Info("Initializing...");
        await RefreshUiAsync();
        clientPollTimer.Start();
    }

    public async Task ShutdownAsync()
    {
        clientPollTimer.Stop();
        phaseMonitor.PhaseChanged -= PhaseChanged;
        phaseMonitor.MonitorError -= OnPhaseMonitorError;
        await phaseMonitor.StopAsync();
    }

    private async void ClientPollTimer_Tick(object? sender, EventArgs e)
    {
        await RefreshUiAsync();
    }

    private async Task RefreshUiAsync()
    {
        if (!Credential.IsLeagueClientRunning())
        {
            await phaseMonitor.StopAsync();
            ShowLaunchMode();
            return;
        }

        ShowFeatureMode();
        if (!phaseMonitor.IsMonitoring)
        {
            SetStatus("Client detected, connecting to API...");

            if (!await phaseMonitor.StartAsync())
            {
                SetStatus("Client detected, but monitoring could not be started.");
            }
            else
            {
                SetStatus("Client connected.");
            }
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


    private void PhaseChanged(object? sender, string phase)
    {
        bool isReadyCheck = string.Equals(phase, "ReadyCheck", StringComparison.OrdinalIgnoreCase);

        if (!isReadyCheck)
        {
            suppressNextAutoAccept = false;
        }

        if (!acEnabled || !isReadyCheck)
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


    private void OnPhaseMonitorError(object? sender, string message)
    {
        SetStatus(message);
    }


    private async Task TryAutoAcceptAsync()
    {
        var accepted = await Match.Accept();
        SetStatus(
            accepted ? "Accepted" : "Failed to accept");
    }

    private void ToggleAutoAccept()
    {
        acEnabled = !acEnabled;
        RunOnUiThread(() => { StatusText = acEnabled ? "AutoAccept enabled" : "AutoAccept disabled"; });
    }

    private void LaunchClient()
    {
        if (Registry.TryLaunchClient(out var launchedPath, out var errorMessage))
        {
            SetStatus($"Client launched!");
            Logger.Info($"{launchedPath}");
            return;
        }

        SetStatus($"Launch failed!");
        Logger.Error($"Launch failed: {errorMessage}");
    }

    private async Task DeclineMatchAsync()
    {
        suppressNextAutoAccept = true;
        var declined = await Match.Decline();
        SetStatus(declined ? "Declined match" : "Failed to decline match");
    }

    private void SetStatus(string message)
    {
        RunOnUiThread(() => { StatusText = message; });
    }

    private void RunOnUiThread(Action action)
    {
        if (dispatcher.CheckAccess())
        {
            action();
            return;
        }

        dispatcher.Invoke(action);
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private sealed class RelayCommand(Action execute, Func<bool> canExecute) : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return canExecute.Invoke();
        }

        public void Execute(object? parameter)
        {
            execute();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private sealed class AsyncRelayCommand(Func<Task> executeAsync, Func<bool>? canExecute = null) : ICommand
    {
        private bool isRunning;


        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return !isRunning && (canExecute?.Invoke() ?? true);
        }

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
            {
                return;
            }

            isRunning = true;
            RaiseCanExecuteChanged();

            try
            {
                await executeAsync();
            }
            catch (Exception ex)
            {
                Logger.Error($"Command execution failed: {ex.Message}");
            }
            finally
            {
                isRunning = false;
                RaiseCanExecuteChanged();
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}