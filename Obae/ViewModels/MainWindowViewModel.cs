using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using Obae.Helpers;
using Obae.Interfaces;
using Obae.Models;

namespace Obae.ViewModels;

public partial class MainWindowViewModel : ViewModelBase , INotifyPropertyChanged
{
    private readonly IBeatmapService _beatmapService;
    private readonly IImageHelpers _imageHelpers;
    private readonly CachedAppSettings _cachedAppSettings;
    private readonly IFileSystemService _fileSystemService;
    
    public MainWindowViewModel(IBeatmapService beatmapService, CachedAppSettings cachedAppSettings, IImageHelpers imageHelpers, IFileSystemService fileSystemService)
    {
        _beatmapService = beatmapService;
        _cachedAppSettings = cachedAppSettings;
        _imageHelpers = imageHelpers;
        _fileSystemService = fileSystemService;
        IsBusy = false;
    }
    public string BeatmapId { get; set; }
    
    private string _statusMessage;
    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (_isBusy != value)
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }
    }
    public ObservableCollection<MemoryStream> BeatmapImages { get; set; }
    private ObservableCollection<Bitmap> _beatmapImagesBitmap;
    public ObservableCollection<Bitmap> BeatmapImagesBitmap 
    { 
        get => _beatmapImagesBitmap; 
        set
        {
            _beatmapImagesBitmap = value;
            OnPropertyChanged(nameof(BeatmapImagesBitmap));
        }
    }
        
    public async Task DownloadBeatmap(string userInput)
    {
        StatusMessage = "Downloading Beatmap...";
        IsBusy = true;
        
        var beatmapId = userInput.ValidateBeatmapInput();

        if (string.IsNullOrEmpty(beatmapId))
        {
            StatusMessage = "Invalid Beatmap ID or Beatmap link provided!";
        }

            
        var beatmapImagesResult = await _beatmapService.DownloadBeatmapOrchestrator(beatmapId, _cachedAppSettings.DefaultFolderPath, _cachedAppSettings.OsuCookieValue);

        if (beatmapImagesResult.Success == false)
        {
            StatusMessage = $"Error downloading beatmap: {beatmapImagesResult.StatusMessage}";
        }

        if (beatmapImagesResult.Success == true && !beatmapImagesResult.Artworks.Any())
        {
            StatusMessage = $"No Artwork found for Beatmap: {beatmapImagesResult.StatusMessage}";
        }

        if (beatmapImagesResult.Success == true && beatmapImagesResult.Artworks.Any())
        {
            BeatmapImages = new ObservableCollection<MemoryStream>(beatmapImagesResult.Artworks);
            // Convert MemoryStream images to Bitmaps so Avalonia UI can Render them
            var convertedBitmaps = _imageHelpers.ConvertMemoryStreamToBitmap(beatmapImagesResult.Artworks);
            BeatmapImagesBitmap = new ObservableCollection<Bitmap>(convertedBitmaps);
            StatusMessage = $"Showing artwork for : {beatmapImagesResult.StatusMessage}";
        }
        
    }
    
    public async Task SaveImageToDisk(IStorageFile saveFilePath, Bitmap bitmap)
    {
        await _fileSystemService.SaveImageToDisk(saveFilePath, bitmap);
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        Console.WriteLine($"Property changed: {propertyName} to {GetType().GetProperty(propertyName)?.GetValue(this)}");
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public async Task SaveImageToDisk(Bitmap bitmap)
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
        
        var filePickerResult = await Application.Current.GetTopLevel().StorageProvider.SaveFilePickerAsync(locationSavePrompt);
        await SaveImageToDisk(filePickerResult, bitmap);
    }

    public async Task CopyImageToClipboard(Bitmap bitmap)
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