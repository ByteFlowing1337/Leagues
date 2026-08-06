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
    public static ObservableCollection<string> LogEntries => Entries;
    private readonly Phase phaseMonitor = new();
    private readonly Dispatcher dispatcher;
    private readonly DispatcherTimer clientPollTimer = new() { Interval = TimeSpan.FromSeconds(2) };

    private string statusText = "Checking Client...";
    private Visibility launchClientVisibility = Visibility.Visible;
    private Visibility featureButtonsVisibility = Visibility.Collapsed;
    private Visibility declineButtonVisibility = Visibility.Collapsed;
    private Visibility acceptButtonVisibility = Visibility.Collapsed;
    private string autoAcceptButtonText = "Enable AutoAccept";
    private bool acEnabled;
    private volatile bool suppressNextAutoAccept;

    public MainWindowViewModel()
    {
        dispatcher = Application.Current.Dispatcher;

        LaunchClientCommand = new RelayCommand(LaunchClient, () => !Credential.IsLeagueClientRunning());
        ToggleAutoAcceptCommand = new RelayCommand(ToggleAutoAccept, Credential.IsLeagueClientRunning);
        DeclineMatchCommand = new AsyncRelayCommand(DeclineMatchAsync);
        AcceptMatchCommand = new AsyncRelayCommand(AcceptMatchAsync);

        phaseMonitor.PhaseChanged += OnPhaseChanged;
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

    public Visibility DeclineButtonVisibility
    {
        get => declineButtonVisibility;
        private set => SetField(ref declineButtonVisibility, value);
    }

    public Visibility AcceptButtonVisibility
    {
        get => acceptButtonVisibility;
        private set => SetField(ref acceptButtonVisibility, value);
    }


    public string AutoAcceptButtonText
    {
        get => autoAcceptButtonText;
        private set => SetField(ref autoAcceptButtonText, value);
    }

    public RelayCommand LaunchClientCommand { get; }
    public RelayCommand ToggleAutoAcceptCommand { get; }
    public AsyncRelayCommand DeclineMatchCommand { get; }
    public AsyncRelayCommand AcceptMatchCommand { get; }

    private bool lastClientRunning;

    public async Task InitializeAsync()
    {
        Logger.Info("Initializing...");
        lastClientRunning = Credential.IsLeagueClientRunning();
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
        var isLeagueClientRunning = Credential.IsLeagueClientRunning();
        if (!isLeagueClientRunning)
        {
            await phaseMonitor.StopAsync();
            ShowLaunchMode();
        }
        else
        {
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

        if (isLeagueClientRunning != lastClientRunning)
        {
            lastClientRunning = isLeagueClientRunning;
            LaunchClientCommand.RaiseCanExecuteChanged();
            ToggleAutoAcceptCommand.RaiseCanExecuteChanged();
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
        // Only display the decline button when the phase is ReadyCheck
        DeclineButtonVisibility = isReadyCheck ? Visibility.Visible : Visibility.Collapsed;
        if (!isReadyCheck || !acEnabled)
        {
            return;
        }

        // If decline button is pressed while ac is enabled,
        // suppress the next auto accept to avoid auto accepting after declining
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
        Logger.Info(
            accepted ? "Accepted" : "Failed to accept");
    }

    private void ToggleAutoAccept()
    {
        acEnabled = !acEnabled;
        AutoAcceptButtonText = acEnabled ? "Disable AutoAccept" : "Enable AutoAccept";
        Logger.Info(acEnabled ? "AutoAccept enabled" : "AutoAccept disabled");
    }

    private void LaunchClient()
    {
        if (Registry.TryLaunchClient(out var launchedPath, out var errorMessage))
        {
            Logger.Info("Client launched");
            return;
        }

        Logger.Error($"Launch failed: {errorMessage}");
    }

    private async Task DeclineMatchAsync()
    {
        if (acEnabled)
        {
            suppressNextAutoAccept = true;
        }

        AcceptButtonVisibility = Visibility.Visible;
        var declined = await Match.Decline();
        Logger.Info(declined ? "Declined match" : "Failed to decline match");
    }

    private async Task AcceptMatchAsync()
    {
        var accepted = await Match.Accept();
        Logger.Info(accepted ? "Accepted match" : "Failed to accept match");
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

    public sealed class RelayCommand(Action execute, Func<bool> canExecute) : ICommand
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

    public sealed class AsyncRelayCommand(Func<Task> executeAsync, Func<bool>? canExecute = null) : ICommand
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