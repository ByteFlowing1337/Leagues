using System.Data;
using System.Reflection;
using System.Windows;
using Leagues.Helper;
namespace Leagues;
public partial class MainWindow : Window
{
    bool ac_enabled = false;
    public MainWindow()
    {
        InitializeComponent();
        }
    private void Toggle_AC(object sender, RoutedEventArgs e)
    {
        ac_enabled = !ac_enabled; 
        if (ac_enabled)
        {
            TBHelloWorld.Text = "Auto-Accept Enabled"; 
            acbtn.Content = "Disable Auto-Accept";
            Accept.StartAutoAccept(); 
        }

    }
}