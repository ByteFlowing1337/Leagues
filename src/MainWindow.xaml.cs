using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

    /// <summary>
    /// Stores the entry text to the clipboard.
    /// </summary>
    private void LogEntries_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var listBox = sender as ListBox;
        if (e.OriginalSource is not DependencyObject element)
            return;
        if (ItemsControl.ContainerFromElement(listBox, element) is not ListBoxItem listBoxElement)
            return;

        if (listBoxElement.DataContext is string entry)
        {
            Clipboard.SetText(entry);
        }
    }
}