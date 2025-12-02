using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Obae.ViewModels;

namespace Obae.Views.Settings;

public partial class SettingsWindow : Window
{
    private SettingsWindowViewModel WindowViewModel => DataContext as SettingsWindowViewModel;

    public SettingsWindow()
    {
        InitializeComponent();
    }
    
    private void StatusBar_OnBarPressed(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            this.BeginMoveDrag(e);
        }
    }
    
    private void Close(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
    
    private async void BrowseFolderButton_OnClickAsync(object sender, RoutedEventArgs e)
    {
        if (DataContext is not SettingsWindowViewModel viewModel) return;
        var folder = await ShowFolderPickerAsync();
            
        if (folder != null)
        {
            viewModel.DefaultFolderPath = folder;
        }
    }
    
    private async Task<string?> ShowFolderPickerAsync()
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Working Directory Folder",
            AllowMultiple = false
        });

        return folders.Count > 0 ? folders[0].Path.LocalPath : null;
    }

    private void ClearOsuCookieSessionValue_OnClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not SettingsWindowViewModel viewModel) return;
        viewModel.OsuSessionCookieValue = string.Empty;
    }
}