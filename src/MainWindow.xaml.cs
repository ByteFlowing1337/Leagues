using System.Windows;
using Leagues.ViewModels;

namespace Leagues;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel viewModel = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await viewModel.InitializeAsync();
    }

    private async void Window_Closed(object? sender, EventArgs e)
    {
        await viewModel.ShutdownAsync();
    }
}