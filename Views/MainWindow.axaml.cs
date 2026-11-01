using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CourseApp.ViewModels;

namespace CourseApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ItemsList_DoubleTapped(object? sender, TappedEventArgs e)
    {
        (DataContext as MainViewModel)?.OpenSelectedItem();
    }

    private async void ImportButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;

        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Wybierz folder z plikami",
            AllowMultiple = false
        });

        if (folders.Count == 0) return;
        var path = folders[0].TryGetLocalPath();
        if (path is not null) await vm.ImportFolderAsync(path);
    }
}