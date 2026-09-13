using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Leagues.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    private static readonly HomeView Home = HomeView.CreateHomeView();


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
        CurrentViewModel = new MatchHistory();
    }

    private bool CanShowMatchView() => CurrentViewModel.GetType() != typeof(MatchHistory);

    [RelayCommand(CanExecute = nameof(CanShowMiscView))]
    private void ShowMiscView()
    {
        CurrentViewModel = new Misc();
    }

    private bool CanShowMiscView() => CurrentViewModel.GetType() != typeof(Misc);
}