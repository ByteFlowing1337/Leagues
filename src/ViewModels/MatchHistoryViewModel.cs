using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Leagues.Models.Mapper;

namespace Leagues.ViewModels;

public partial class MatchSummaryViewModel : ObservableObject
{
    [ObservableProperty] public partial BitmapImage? ChampionAvatar { get; private set; }
    [ObservableProperty] public partial MatchSummary Summary { get; private set; }
    private bool statsLoaded;

    public MatchSummaryViewModel(MatchSummary summary)
    {
        Summary = summary;
        HomeViewModel.PhaseMonitor.PhaseChanged += OnPhaseChanged;
    }

    public async Task LoadAvatarAsync()
    {
        ChampionAvatar = await ChampionMapper.ChampionIdToImage(Summary.ChampionId);
    }

    public async Task LoadSummonerStatsAsync(bool friend)
    {
    }

    private async void OnPhaseChanged(object? sender, string phase)
    {
        if (statsLoaded)
            return;
        switch (phase)
        {
            case "ChampSelect":
                await LoadSummonerStatsAsync(friend: true);
                break;
            case "GameStart" or "InProgress":
                await LoadSummonerStatsAsync(friend: false);
                break;
        }
    }
}