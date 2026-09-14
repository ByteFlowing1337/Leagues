using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Leagues.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    private static readonly HomeView Home = new();
    private static readonly MatchView MatchView = new();
    private static readonly Misc Misc = new();


    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ShowHomeViewCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowMatchViewCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowMiscViewCommand))]
    public partial object CurrentViewModel { get; set; } = Home;

    [RelayCommand(CanExecute = nameof(CanShowHomeView))]
    private void ShowHomeView()
    {
        CurrentViewModel = Home;
    }

    private bool CanShowHomeView() => CurrentViewModel.GetType() != typeof(HomeView);

    [RelayCommand(CanExecute = nameof(CanShowMatchView))]
    private void ShowMatchView()
    {
        CurrentViewModel = MatchView;
    }

    private bool CanShowMatchView() => CurrentViewModel.GetType() != typeof(MatchView);

    [RelayCommand(CanExecute = nameof(CanShowMiscView))]
    private void ShowMiscView()
    {
        CurrentViewModel = Misc;
    }

    private bool CanShowMiscView() => CurrentViewModel.GetType() != typeof(Misc);
}