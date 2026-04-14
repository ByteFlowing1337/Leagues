using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Leagues.Logging;
using Leagues.Utils;
using Leagues.Services;

namespace Leagues.ViewModels;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private const int MaxLogEntries = 300;
    private readonly Phase phaseMonitor = new();
    private readonly Dispatcher dispatcher;
    private readonly DispatcherTimer clientPollTimer = new() { Interval = TimeSpan.FromSeconds(2) };

    private string statusText = "Checking Client...";
    private Visibility launchClientVisibility = Visibility.Visible;
    private Visibility featureButtonsVisibility = Visibility.Collapsed;
    private string autoAcceptButtonText = "Enable Auto-Accept";
    private bool acEnabled;

    public MainWindowViewModel()
    {
        dispatcher = Application.Current.Dispatcher;

        LaunchClientCommand = new RelayCommand(LaunchClient);
        ToggleAutoAcceptCommand = new RelayCommand(ToggleAutoAccept);
        DeclineMatchCommand = new AsyncRelayCommand(DeclineMatchAsync);

        phaseMonitor.PhaseChanged += OnPhaseChanged;
        phaseMonitor.MonitorError += OnPhaseMonitorError;
        AppLog.MessageRaised += OnLogMessage;
        clientPollTimer.Tick += ClientPollTimer_Tick;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<string> LogEntries { get; } = new();

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
        AppendLog("Application started, checking client status");
        await RefreshUiAsync();
        clientPollTimer.Start();
    }

    public async Task ShutdownAsync()
    {
        clientPollTimer.Stop();
        phaseMonitor.PhaseChanged -= OnPhaseChanged;
        phaseMonitor.MonitorError -= OnPhaseMonitorError;
        AppLog.MessageRaised -= OnLogMessage;
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
            SetStatus("Client detected, connecting to API...", appendLog: false);

            await phaseMonitor.StartAsync();

        }
    }

    private void ShowLaunchMode()
    {
        LaunchClientVisibility = Visibility.Visible;
        FeatureButtonsVisibility = Visibility.Collapsed;
        SetStatus("Client is not running.", appendLog: false);
    }

    private void ShowFeatureMode()
    {
        LaunchClientVisibility = Visibility.Collapsed;
        FeatureButtonsVisibility = Visibility.Visible;
    }
    
    private volatile bool suppressNextAutoAccept = false;
    private void OnPhaseChanged(string phase)
    {
        
        RunOnUiThread(() =>
        {
            StatusText = acEnabled ? "Auto-accept enabled" : "Auto-accept disabled";
        });


        if (!acEnabled && !string.Equals(phase, "ReadyCheck", StringComparison.OrdinalIgnoreCase))
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
    

    private void OnPhaseMonitorError(string message)
    {
        SetStatus(message, appendLog: true);
    }

    private void OnLogMessage(string message)
    {
        SetStatus(message, appendLog: true);
    }

    private async Task TryAutoAcceptAsync()
    {
        var accepted = await Accept.AcceptMatchAsync();
        SetStatus(accepted ? "ReadyCheck detected, accepted automatically" : "ReadyCheck detected, but auto-accept failed", appendLog: true);
    }

    private void ToggleAutoAccept()
    {
        acEnabled = !acEnabled;

        if (acEnabled)
        {
            AutoAcceptButtonText = "Disable Auto-Accept";
            Accept.StartAutoAccept();
            SetStatus("Auto-accept is enabled", appendLog: true);
            return;
        }

        AutoAcceptButtonText = "Enable Auto-Accept";
        Accept.StopAutoAccept();
        SetStatus("Auto-accept is disabled", appendLog: true);
    }

    private void LaunchClient()
    {
        if (ReadRegistry.TryLaunchClient(out var launchedPath, out var errorMessage))
        {
            SetStatus($"Client launched: {launchedPath}", appendLog: true);
            return;
        }

        SetStatus($"Launch failed: {errorMessage}", appendLog: true);
    }

    private async Task DeclineMatchAsync()
    {
        var declined = await Accept.DeclineMatchAsync();

        suppressNextAutoAccept = true;

        SetStatus(declined ? "Decline match request sent" : "Failed to decline match", appendLog: true);
    }

    private void SetStatus(string message, bool appendLog)
    {
        RunOnUiThread(() =>
        {
            StatusText = message;
            if (appendLog)
            {
                AppendLog(message);
            }
        });
    }

    private void AppendLog(string message)
    {
        LogEntries.Add($"{DateTime.Now:HH:mm:ss} {message}");

        if (LogEntries.Count > MaxLogEntries)
        {
            LogEntries.RemoveAt(0);
        }
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

    private sealed class RelayCommand : ICommand
    {
        private readonly Action execute;
        private readonly Func<bool>? canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return canExecute?.Invoke() ?? true;
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

    private sealed class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> executeAsync;
        private readonly Func<bool>? canExecute;
        private bool isRunning;

        public AsyncRelayCommand(Func<Task> executeAsync, Func<bool>? canExecute = null)
        {
            this.executeAsync = executeAsync;
            this.canExecute = canExecute;
        }

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
                AppLog.Logging($"Command execution failed: {ex.Message}");
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