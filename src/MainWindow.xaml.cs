using System.Windows;
using Leagues.ViewModels;

namespace Leagues;

public partial class MainWindow
{
    private readonly MainWindowViewModel viewModel = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await viewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void Window_Closed(object? sender, EventArgs e)
    {
        try
        {
            await viewModel.ShutdownAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}