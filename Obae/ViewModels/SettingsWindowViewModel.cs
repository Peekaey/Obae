using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Obae.Interfaces;
using Obae.Models;

namespace Obae.ViewModels;

public class SettingsWindowViewModel : ViewModelBase , INotifyPropertyChanged
{
        private readonly AppSettings _appSettings;
        private readonly IFileService _fileService;
    
    public SettingsWindowViewModel(AppSettings appSettings, IFileService fileService)
    {
        _appSettings = appSettings;
        _fileService = fileService;
        // Populate available themes
        Themes = Enum.GetValues(typeof(Themes)).Cast<Themes>().ToList();
    }
    
    // Gets properties on Initialisation from AppSettings
    // Working Directory
    public string DefaultFolderPath
    {
        get => _appSettings.DefaultFolderPath;
        set
        {
            if (_appSettings.DefaultFolderPath != value)
            {
                _appSettings.DefaultFolderPath = value;
                OnPropertyChanged(nameof(DefaultFolderPath));
            }
        }
    }
    
    // Value of the Users Cookie Session Value
    public string OsuSessionCookieValue
    {
        get => _appSettings.OsuCookieValue;
        set
        {
            if (_appSettings.OsuCookieValue != value)
            {
                _appSettings.OsuCookieValue = value;

                if (_appSettings.SaveSettingsToConfigFile == true)
                { 
                    _fileService.SaveSettingsToJsonConfig(_appSettings.DefaultFolderPath,
                        _appSettings.ConfigFilePath, _appSettings.OsuCookieValue, _appSettings.SelectedThemes.ToString());
                }
                
                OnPropertyChanged(nameof(OsuSessionCookieValue));
            }
        }
    }
    
    public bool SaveSettingsToConfigFile
    {
        get => _appSettings.SaveSettingsToConfigFile;
        set
        {
            if (_appSettings.SaveSettingsToConfigFile != value)
            {
                _appSettings.SaveSettingsToConfigFile = value;

                if (_appSettings.SaveSettingsToConfigFile == true)
                {
                        _fileService.SaveSettingsToJsonConfig(_appSettings.DefaultFolderPath,
                            _appSettings.ConfigFilePath, _appSettings.OsuCookieValue, _appSettings.SelectedThemes.ToString());
                }
                else
                {
                    _fileService.RemoveSettingsJsonConfig(_appSettings.ConfigFilePath);
                    _appSettings.OsuCookieValue = string.Empty;
                }
                OnPropertyChanged(nameof(SaveSettingsToConfigFile));
            }
        }
    }
    
    public List<Themes> Themes { get; }

    public Themes SelectedTheme
    {
        get => _appSettings.SelectedThemes;
        set
        {
            if (_appSettings.SelectedThemes != value)
            {
                _appSettings.SelectedThemes = value;
                // Dynamically apply the new theme
                var app = (App)Application.Current;
                app.SetTheme(value);
                
                if (_appSettings.SaveSettingsToConfigFile == true)
                {
                    _fileService.SaveSettingsToJsonConfig(_appSettings.DefaultFolderPath,
                        _appSettings.ConfigFilePath, _appSettings.OsuCookieValue, _appSettings.SelectedThemes.ToString());
                }
                
                OnPropertyChanged(nameof(SelectedTheme));

            }
        }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        Console.WriteLine($"Property changed: {propertyName} to {GetType().GetProperty(propertyName)?.GetValue(this)}");
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}