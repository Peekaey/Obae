using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using Obae.Helpers;
using Obae.Interfaces;
using Obae.Models;
using Obae.ViewModels;
using Obae.Views.Settings;

namespace Obae.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel ViewModel => DataContext as MainWindowViewModel;
    public MainWindow()
    {
        InitializeComponent();

        if (OperatingSystem.IsWindows())
        {
            StatusBar.IsVisible = true;
            ExtendClientAreaToDecorationsHint = true;
        }
        else
        {
            StatusBar.IsVisible = false;
            ExtendClientAreaToDecorationsHint = false;
        }
    }

    private void StatusBar_OnBarPressed(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            this.BeginMoveDrag(e);
        }
    }
    private void Minimise(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private void Maximise(object sender, RoutedEventArgs e)
    {
        this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void Close(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
    
    private void RenderSettingsPage(object? sender, Avalonia.Interactivity.RoutedEventArgs eventArgs)
    {
        var appSettings = App.ServiceProvider.GetRequiredService<CachedAppSettings>();
        var fileService = App.ServiceProvider.GetRequiredService<IFileSystemService>();
        var dataService = App.ServiceProvider.GetRequiredService<IDataService>();
        var settingsViewModel = new SettingsWindowViewModel(appSettings, fileService, dataService);
        var settingsWindow = new SettingsWindow
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            DataContext = settingsViewModel
        };
        settingsWindow.ShowDialog(this);
    }

    private async void GetBeatmapForDownload(object sender, KeyEventArgs e)
    {
        try
        {
            if (e.Key == Key.Enter)
            {
                var textBox = sender as TextBox;
                if (textBox != null)
                {
                    string userInput = textBox.Text;
                    ViewModel.DownloadBeatmap(userInput);
                }
            }
        }
        catch (Exception error)
        {
            // Swallow for now
        }
    }
    
    public async void SaveImageToDisk_MenuItem(object? sender, RoutedEventArgs e)
    {
        try
        {
            // Image Nested Object is MenuItem > Parent > Parent > Image with ImageData being in Image.Source as Bitmap
            if (sender is MenuItem menuItem && menuItem.Parent is ContextMenu contextMenu && contextMenu.Parent is Popup popup && popup.Parent is Image image)
            {
                // Image.Source is final Image
                if (image.Source is Bitmap bitmap)
                {
                    await ViewModel.SaveImageToDisk(bitmap);
                }
            }
        }
        catch (Exception ex)
        {
            // Swallow for now
        }
    }

    public async void CopyImageToClipboard_MenuItem(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is MenuItem menuItem &&
                menuItem.Parent is ContextMenu contextMenu &&
                contextMenu.Parent is Popup popup &&
                popup.Parent is Image image)
            {
                if (image.Source is Bitmap bitmap)
                {
                    ViewModel.CopyImageToClipboard(bitmap);
                }
            }
        }
        catch (Exception ex)
        {
            // Swallow for now
        }
    }
    
}