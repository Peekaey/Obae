using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Obae.Interfaces;
using Obae.Models;
using Obae.Models.Enums;

namespace Obae.ViewModels;

public class SettingsWindowViewModel : ViewModelBase , INotifyPropertyChanged
{
        private readonly CachedAppSettings _cachedAppSettings;
        private readonly IFileService _fileService;
        private readonly IDataService _dataService;
    
    public SettingsWindowViewModel(CachedAppSettings cachedAppSettings, IFileService fileService , IDataService dataService)
    {
        _cachedAppSettings = cachedAppSettings;
        _fileService = fileService;
        _dataService = dataService;
        
        // Populate available themes
        Themes = Enum.GetValues(typeof(Themes)).Cast<Themes>().ToList();
        MirrorSources = Enum.GetValues(typeof(MirrorSources)).Cast<MirrorSources>().ToList();
        _selectedMirrorSources = new ObservableCollection<MirrorSources>(_cachedAppSettings.SelectedMirrorSources ?? new List<MirrorSources>());
        _selectedMirrorSources.CollectionChanged += OnSelectedMirrorSourcesChanged;

    }
    
    // Gets properties on Initialisation from AppSettings
    // Working Directory
    public string DefaultFolderPath
    {
        get => _cachedAppSettings.DefaultFolderPath;
        set
        {
            if (_cachedAppSettings.DefaultFolderPath != value)
            {
                _cachedAppSettings.DefaultFolderPath = value;
                OnPropertyChanged(nameof(DefaultFolderPath));
            }
        }
    }
    
    // Value of the Users Cookie Session Value
    public string OsuSessionCookieValue
    {
        get => _cachedAppSettings.OsuCookieValue;
        set
        {
            if (_cachedAppSettings.OsuCookieValue != value)
            {
                _cachedAppSettings.OsuCookieValue = value;
                OnPropertyChanged(nameof(OsuSessionCookieValue));
            }
        }
    }
    
    public bool SaveSettingsToDatabase
    {
        get => _cachedAppSettings.SaveSettingsToDatabase;
        set
        { 
            OnPropertyChanged(nameof(SaveSettingsToDatabase));
        }
    }
    
    public List<MirrorSources> MirrorSources { get; }
    
    // Use ObservableCollection over List due to incompatibility issues with ListBox
    private readonly ObservableCollection<MirrorSources> _selectedMirrorSources;
    public ObservableCollection<MirrorSources> SelectedMirrorSources
    {
        get => _selectedMirrorSources;
    }
    
    public List<Themes> Themes { get; }

    public Themes SelectedTheme
    {
        get => _cachedAppSettings.SelectedTheme;
        set
        {
            if (_cachedAppSettings.SelectedTheme != value)
            {
                _cachedAppSettings.SelectedTheme = value;
                // Dynamically apply the new theme
                var app = (App)Application.Current;
                app.SetTheme(value);
                OnPropertyChanged(nameof(SelectedTheme));
            }
        }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        Console.WriteLine($"Property changed: {propertyName} to {GetType().GetProperty(propertyName)?.GetValue(this)}");
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        HandleSaveToDatabase();
    }
    
    // Subscribe to the SelectedMirrorSources over a setter due Avalonia modifying the existing collection instead of creating a new collection
    // Therefore the setter is never triggered
    private void OnSelectedMirrorSourcesChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        _cachedAppSettings.SelectedMirrorSources = _selectedMirrorSources.ToList();
        HandleSaveToDatabase();
    }

    // If SaveSettingsToDatabase is checked, store the preferences in the sqlite database
    // Otherwise we just keep it in memory |> Results in app launching with defaults next time
    private void HandleSaveToDatabase()
    {
        if (_cachedAppSettings.SaveSettingsToDatabase)
        {
            _dataService.SaveSettingsToDatabase(_cachedAppSettings);
        }
    }
}