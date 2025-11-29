using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Obae.Helpers;
using Obae.Interfaces;
using Obae.Models;
using Obae.Models.Database;
using Obae.Models.Enums;
using Obae.Services;
using Obae.ViewModels;
using Obae.Views;

namespace Obae;

public partial class App : Application
{
    private IServiceProvider _serviceProvider;
    public static IServiceProvider ServiceProvider { get; private set; }
    
    private StyleInclude? _lightTheme;
    private StyleInclude? _darkTheme;
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        _lightTheme = new StyleInclude(new Uri("avares://Obae/"))
        {
            Source = new Uri("avares://Obae/Themes/LightTheme.axaml")
        };

        _darkTheme = new StyleInclude(new Uri("avares://Obae/"))
        {
            Source = new Uri("avares://Obae/Themes/DarkTheme.axaml")
        };
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new CookieContainer());
        services.AddSingleton<IApiManagerService, ApiManagerService>();
        services.AddHttpClient<ApiManagerService>()
            .ConfigurePrimaryHttpMessageHandler(sp =>
            {
                return new HttpClientHandler
                {
                    AllowAutoRedirect = false,
                    CookieContainer = sp.GetRequiredService<CookieContainer>(),
                    UseCookies = true,
                };
            });
        services.AddSingleton<IBeatmapService, BeatmapService>();
        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<IValidationHelper, ValidationHelper>();
        services.AddSingleton<IPlaywrightService, PlaywrightService>();
        services.AddSingleton<IImageHelpers, ImageHelpers>();
        services.AddSingleton<IDataService, DataService>();
        services.AddDbContext<DataContext>(options => options.UseSqlite("Data Source=obae-app.db"));

        services.AddSingleton<CachedAppSettings>();
        
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<SettingsWindowViewModel>();
        

        _serviceProvider = services.BuildServiceProvider();
        
        
        InitialiseDatabase();
        InitialiseAppSettings();
        ServiceProvider = _serviceProvider;
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>()
            };
        }
        
        SetTheme(_serviceProvider.GetRequiredService<CachedAppSettings>().SelectedTheme);
        base.OnFrameworkInitializationCompleted();
    }

    private void InitialiseAppSettings()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
            var cachedSettings = scope.ServiceProvider.GetRequiredService<CachedAppSettings>();
            var dbSettings = dbContext.AppSettings.FirstOrDefault();

            if (dbSettings != null)
            {
                cachedSettings.DefaultFolderPath = dbSettings.DefaultFolderPath;
                cachedSettings.OsuCookieValue = dbSettings.OsuCookieValue ?? string.Empty;
                cachedSettings.SaveSettingsToDatabase = dbSettings.SaveSettingsToDatabase;
                cachedSettings.SelectedTheme = dbSettings.SelectedTheme;
                cachedSettings.SelectedMirrorSources = dbSettings.SelectedMirrorSources;
            }
        }
    }
    private void InitialiseDatabase()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
            dbContext.Database.EnsureCreated();

            var existingSettings = dbContext.AppSettings.FirstOrDefault();
            // If existing settings doesn't exist or existing settings does exist but save settings is false
            // |> Recreate the default app settings for launch
            if (existingSettings == null || existingSettings.SaveSettingsToDatabase == false)
            {
                // If settings existing and save settings was disabled |> was disabled from last launch
                // Therefore we want to wipe all pre existing configurations and start on a clean slate
                if (existingSettings != null && existingSettings.SaveSettingsToDatabase == false)
                {
                    dbContext.Remove(existingSettings);
                }
                
                var defaultMirrorSources = new List<MirrorSources>
                {
                    MirrorSources.Nerinyan,
                    MirrorSources.BeatConnect,
                };

                var defaultSettings = new AppSettings
                {
                    SelectedTheme = Themes.Light,
                    OsuCookieValue = null,
                    DefaultFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Obae"),
                    SaveSettingsToDatabase = false,
                    SelectedMirrorSourcesJson = JsonSerializer.Serialize(defaultMirrorSources),
                    LastUpdatedUtc = DateTime.UtcNow
                };
                
                dbContext.AppSettings.Add(defaultSettings);
                dbContext.SaveChanges();
            
                Console.WriteLine("Default app settings created in database");
            }
            
        }
    }
    
    public void SetTheme(Themes themes)
    {
        if (_lightTheme != null && Styles.Contains(_lightTheme))
        {
            Styles.Remove(_lightTheme);
        }

        if (_darkTheme != null && Styles.Contains(_darkTheme))
        {
            Styles.Remove(_darkTheme);
        }

        if (themes == Themes.Light && _lightTheme != null)
        {
            Styles.Add(_lightTheme);
        }
        else if (themes == Themes.Dark && _darkTheme != null)
        {
            Styles.Add(_darkTheme);
        }
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}