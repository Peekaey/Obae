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
        var fileService = App.ServiceProvider.GetRequiredService<IFileService>();
        var dataService = App.ServiceProvider.GetRequiredService<IDataService>();
        var settingsViewModel = new SettingsWindowViewModel(appSettings, fileService, dataService);
        var settingsWindow = new SettingsWindow
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            DataContext = settingsViewModel
        };
        settingsWindow.ShowDialog(this);
    }

    private void GetBeatmapForDownload(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                string userInput = textBox.Text;
                var downloadResult = ViewModel.DownloadBeatmap(userInput);
            }
        }
    }
    
    public async void SaveImageToDisk(object? sender, RoutedEventArgs e)
    {
        try
        {
            // Image Nested Object is MenuItem > Parent > Parent > Image with ImageData being in Image.Source as Bitmap
            if (sender is MenuItem menuItem &&
                menuItem.Parent is ContextMenu contextMenu &&
                contextMenu.Parent is Popup popup &&
                popup.Parent is Image image)
            {
                // Image.Source is final Image
                if (image.Source is Bitmap bitmap)
                {
                    var locationSavePrompt = new FilePickerSaveOptions
                    {
                        Title = "Save As",
                        SuggestedFileName = "osu-artwork",
                        FileTypeChoices = new[]
                        {
                            new FilePickerFileType("PNG Image") { Patterns = new[] { "*.png" } },
                            new FilePickerFileType("JPEG Image") { Patterns = new[] { "*.jpg", "*.jpeg" } },
                            new FilePickerFileType("All Files") { Patterns = new[] { "*.*" } }
                        }
                    };

                    var filePickerResult = await StorageProvider.SaveFilePickerAsync(locationSavePrompt);
                    ViewModel.SaveImageToDisk(filePickerResult, bitmap);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving image: {ex.Message}");
        }
    }

    public async void CopyImageToClipboard(object? sender, RoutedEventArgs e)
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
                    var topLevel = Application.Current.GetTopLevel();
                    var clipboard = topLevel?.Clipboard;
                    if (clipboard != null)
                    {
                        using var memoryStream = new MemoryStream();
                        bitmap.Save(memoryStream);
                        memoryStream.Position = 0;
                        var bytes = memoryStream.ToArray();

                        var dataObject = new DataObject();

                        // Add multiple formats for cross-platform compatibility
                        if (OperatingSystem.IsMacOS())
                        {
                            dataObject.Set("public.png", bytes);
                        }
                        else if (OperatingSystem.IsWindows())
                        {
                            dataObject.Set("PNG", bytes);
                        }
                        else
                        {
                            dataObject.Set("image/png", bytes);
                        }

                        await clipboard.SetDataObjectAsync(dataObject);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving image: {ex.Message}");
        }
    }
    
}