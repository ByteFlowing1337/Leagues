using System.Windows;
using Leagues.Tests.TestSupport;
using Leagues.ViewModels;

namespace Leagues.Tests.ViewModels;

/// <summary>
/// Covers the branching logic in <see cref="HomeViewModel"/> around ready-check handling and
/// auto-accept suppression. These tests invoke the private <c>OnPhaseChanged</c> handler directly
/// (rather than raising the shared static <c>PhaseMonitor.PhaseChanged</c> event) so tests stay
/// isolated from one another and never touch the network-calling accept path.
/// </summary>
public class HomeViewModelTests
{
    private static HomeViewModel CreateViewModel()
    {
        WpfTestApplication.EnsureApplication();
        return new HomeViewModel();
    }

    private static void RaisePhaseChanged(HomeViewModel viewModel, string phase) =>
        viewModel.InvokePrivateMethod("OnPhaseChanged", null, phase);

    [WpfFact]
    public void OnPhaseChanged_NonReadyCheckPhase_HidesDeclineButtonAndLeavesSuppressFlagUntouched()
    {
        var viewModel = CreateViewModel();
        viewModel.IsAutoAcceptEnabled = true;
        viewModel.SetPrivateField("suppressNextAutoAccept", true);

        RaisePhaseChanged(viewModel, "ChampSelect");

        Assert.Equal(Visibility.Collapsed, viewModel.DeclineButtonVisibility);
        Assert.True(viewModel.GetPrivateField<bool>("suppressNextAutoAccept"));
    }

    [WpfFact]
    public void OnPhaseChanged_ReadyCheckWithAutoAcceptDisabled_ShowsDeclineButtonAndLeavesSuppressFlagUntouched()
    {
        var viewModel = CreateViewModel();
        viewModel.IsAutoAcceptEnabled = false;
        viewModel.SetPrivateField("suppressNextAutoAccept", true);

        RaisePhaseChanged(viewModel, "ReadyCheck");

        Assert.Equal(Visibility.Visible, viewModel.DeclineButtonVisibility);
        Assert.True(viewModel.GetPrivateField<bool>("suppressNextAutoAccept"));
    }

    [WpfFact]
    public void OnPhaseChanged_ReadyCheckWithAutoAcceptAndPendingSuppression_ConsumesSuppressFlagWithoutAccepting()
    {
        var viewModel = CreateViewModel();
        viewModel.IsAutoAcceptEnabled = true;
        viewModel.SetPrivateField("suppressNextAutoAccept", true);

        RaisePhaseChanged(viewModel, "ReadyCheck");

        Assert.Equal(Visibility.Visible, viewModel.DeclineButtonVisibility);
        Assert.False(viewModel.GetPrivateField<bool>("suppressNextAutoAccept"));
    }

    [WpfFact]
    public void OnPhaseChanged_PhaseNameIsCaseInsensitive()
    {
        var viewModel = CreateViewModel();
        // Pinned explicitly so this test's outcome never depends on the AutoAccept value
        // persisted in the local settings.json; only DeclineButtonVisibility is under test here.
        viewModel.IsAutoAcceptEnabled = false;

        RaisePhaseChanged(viewModel, "readycheck");

        Assert.Equal(Visibility.Visible, viewModel.DeclineButtonVisibility);
    }

    [WpfTheory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void LaunchClientCommand_CanExecute_ReflectsInverseOfClientRunningState(
        bool isClientRunning, bool expectedCanExecute)
    {
        var viewModel = CreateViewModel();
        viewModel.SetPrivateProperty("IsClientRunning", isClientRunning);

        Assert.Equal(expectedCanExecute, viewModel.LaunchClientCommand.CanExecute(null));
    }

    [WpfTheory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void ToggleAutoAcceptCommand_CanExecute_ReflectsClientRunningState(
        bool isClientRunning, bool expectedCanExecute)
    {
        var viewModel = CreateViewModel();
        viewModel.SetPrivateProperty("IsClientRunning", isClientRunning);

        Assert.Equal(expectedCanExecute, viewModel.ToggleAutoAcceptCommand.CanExecute(null));
    }
}
