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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Obae.Helpers;
using Obae.Interfaces;
using Obae.Models;
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

        services.AddSingleton<AppSettings>(provider =>
        {
            var settings = new AppSettings();
            if (File.Exists(settings.ConfigFilePath))
            {
                try
                {
                    
                    var configContexts = File.ReadAllText(settings.ConfigFilePath);
                    var configDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(configContexts);
                    settings.OsuCookieValue = configDictionary?["Cookie"];
                    settings.SelectedThemes = (Themes)Enum.Parse(typeof(Themes), configDictionary?["Theme"] ?? "0");
                    settings.SaveSettingsToConfigFile = true;                    settings.SaveSettingsToConfigFile = true;
                }
                catch (Exception e)
                {
                    // If can't read value from file, assume file is corrupt or invalid
                    // Do nothing and run with default settings
                    Console.WriteLine("Error reading config file: " + e.Message);
                }
            }
            return settings;
        });
        
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<SettingsWindowViewModel>();
        
        
        _serviceProvider = services.BuildServiceProvider();
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
        
        SetTheme(_serviceProvider.GetRequiredService<AppSettings>().SelectedThemes);
        base.OnFrameworkInitializationCompleted();
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