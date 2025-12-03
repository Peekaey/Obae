using System.Net.Mime;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.VisualTree;

namespace Obae.Helpers;

public static class ApplicationExtensions
{
    // https://github.com/AvaloniaUI/Avalonia/discussions/11170#discussioncomment-10714434
    /// <summary>
    /// Returns the TopLevel from the main window or view. 
    /// </summary>
    /// <param name="app">The application to get the TopLevel for.</param>
    /// <returns>A TopLevel object.</returns>
    public static TopLevel? GetTopLevel(this Application? app)
    {
        if (app?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
    
        if (app?.ApplicationLifetime is ISingleViewApplicationLifetime viewApp)
        {
            var visualRoot = viewApp.MainView?.GetVisualRoot();
            return visualRoot as TopLevel;
        }
    
        return null;
    }
}