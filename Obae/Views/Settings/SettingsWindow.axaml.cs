using System;
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
    private SettingsWindowViewModel? WindowViewModel => DataContext as SettingsWindowViewModel;

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
        try
        {
            if (DataContext is not SettingsWindowViewModel viewModel) return;
            await WindowViewModel.BrowseFolderButtonAsync();
        }
        catch (Exception error)
        {
            // Swallow for now
        }
    }
    
    private async void ClearOsuCookieSessionValue_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is not SettingsWindowViewModel viewModel) return;
            await WindowViewModel.ClearOsuCookieSessionValue();
        }
        catch (Exception error)
        {
            // Swallow for now
        }
    }
}