using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Leagues.ViewModels;


namespace Leagues;

/// <summary>
/// Interaction logic for HomeView.xaml
/// </summary>
public partial class HomeView
{
    public HomeView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }


    public static HomeView CreateHomeView() => new() { DataContext = new HomeViewModel() };

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
            Clipboard.SetText(entry);
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is HomeViewModel vm)
            await vm.InitializeAsync();
    }

    private async void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is HomeViewModel vm)
            await vm.ShutdownAsync();
    }
}