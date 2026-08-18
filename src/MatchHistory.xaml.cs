using System.Windows.Input;
using static Leagues.Models.Logging.Logging;
using Leagues.Models.MatchMapper;

namespace Leagues;

public partial class MatchHistory
{
    // To avoid race if user entered too quickly?
    private bool isQuerying;

    public MatchHistory()
    {
        InitializeComponent();
    }

    private async void QueryMatch_OnEnterKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || isQuerying)
            return;
        // To avoid race if user entered too quickly?
        isQuerying = true;
        ResultsList.ItemsSource = null;

        try
        {
            var playerName = InputPlayerName.Text;

            if (string.IsNullOrWhiteSpace(playerName))
            {
                ResultsList.ItemsSource = new[] { "Enter a player name." };
                return;
            }

            var summaries = await MatchMapper.ToSummaries(playerName, 0, 20);
            if (summaries is null)
            {
                ResultsList.ItemsSource = new[] { "No matches found or error fetching matches." };
            }
            else if (summaries.Count == 0)
            {
                ResultsList.ItemsSource = new[] { "No matches found." };
            }

            ResultsList.ItemsSource = summaries;
        }
        catch (Exception ex)
        {
            Logger.Error($"Match query failed: {ex.Message}");
            ResultsList.ItemsSource = new[] { "Error fetching matches." };
        }
        finally
        {
            isQuerying = false;
        }
    }
}