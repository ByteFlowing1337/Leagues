namespace Leagues.ViewModels;

public class MatchStatsViewModel
{
    public MatchStatsViewModel()
    {
        HomeViewModel.PhaseMonitor.PhaseChanged += OnPhaseChanged;
    }

    private bool allStatsLoaded;

    public async Task LoadSummonerStatsAsync(bool friend)
    {
        allStatsLoaded = true;
    }

    private async void OnPhaseChanged(object? sender, string phase)
    {
        if (allStatsLoaded)
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