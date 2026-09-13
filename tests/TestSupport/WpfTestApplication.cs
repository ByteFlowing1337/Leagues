using System.Windows;

namespace Leagues.Tests.TestSupport;

/// <summary>
/// Some view models rely on <see cref="Application.Current"/> for their <see cref="System.Windows.Threading.Dispatcher"/>.
/// Tests exercising them must run under <c>[WpfFact]</c>/<c>[WpfTheory]</c> (from Xunit.StaFact), which serializes
/// execution onto a single STA/dispatcher thread, and must call this first to ensure an <see cref="Application"/> exists.
/// </summary>
internal static class WpfTestApplication
{
    public static void EnsureApplication()
    {
        if (Application.Current is null)
        {
            _ = new Application();
        }
    }
}