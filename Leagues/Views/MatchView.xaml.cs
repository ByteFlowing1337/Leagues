using Leagues.ViewModels;

namespace Leagues;

public partial class MatchView
{
    public MatchView()
    {
        InitializeComponent();
        DataContext = new MatchViewModel();
    }
}