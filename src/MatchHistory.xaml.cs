using System.Windows.Input;
using Leagues.Models.MatchMapper;
using static Leagues.Models.Logging.Logging;

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
                Snackbar.MessageQueue?.Enqueue("Player name cannot be empty.");
                return;
            }

            var summaries = await MatchMapper.ToSummaries(playerName, 0, 20);
            if (summaries is null)
                Snackbar.MessageQueue?.Enqueue("No matches found or error fetching matches.");
            else if (summaries.Count == 0)
                Snackbar.MessageQueue?.Enqueue("No matches found.");
            else
                ResultsList.ItemsSource = summaries;
        }
        catch (Exception ex)
        {
            Snackbar.MessageQueue?.Enqueue($"Error fetching match history: {ex.Message}");
            Logger.Error($"Error fetching match history for player {InputPlayerName.Text}: {ex}");
        }
        finally
        {
            isQuerying = false;
        }
    }
}