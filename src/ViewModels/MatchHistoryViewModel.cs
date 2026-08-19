using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Leagues.Models.Mapper;

namespace Leagues.ViewModels;

public partial class MatchSummaryViewModel : ObservableObject
{
    [ObservableProperty] public partial BitmapImage? ChampionAvatar { get; private set; }
    [ObservableProperty] public partial MatchMapper.MatchSummary Summary { get; private set; }

    public MatchSummaryViewModel(MatchMapper.MatchSummary summary)
    {
        Summary = summary;
    }

    public async Task LoadAvatarAsync()
    {
        ChampionAvatar = await ChampionMapper.ChampionIdToImage(Summary.ChampionId);
    }
}