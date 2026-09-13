using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Leagues.ViewModels;

public partial class MatchViewModel : ObservableObject
{
    private static readonly MatchSearchView SearchView = new() { DataContext = new MatchSearchView() };


    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ShowSearchViewCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowStatsViewCommand))]
    public partial object CurrentViewModel { get; set; } = SearchView;

    [RelayCommand(CanExecute = nameof(CanShowSearchView))]
    private void ShowSearchView()
    {
        CurrentViewModel = SearchView;
    }

    private bool CanShowSearchView() => CurrentViewModel.GetType() != typeof(MatchSearchView);

    [RelayCommand(CanExecute = nameof(CanShowStatsView))]
    private void ShowStatsView()
    {
        CurrentViewModel = new MatchStatsView();
    }

    private bool CanShowStatsView() => CurrentViewModel.GetType() != typeof(MatchStatsView);
}