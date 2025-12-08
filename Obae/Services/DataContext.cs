using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Obae.Models;
using Obae.Models.Database;
using Obae.Models.Enums;

namespace Obae.Services;

public class DataContext : DbContext 
{
    public DbSet<AppSettings> AppSettings { get; set; }
    
    /// <summary>
    /// Gets the database path from environment variable or returns default path.
    /// On macOS bundle, OBAE_DB_PATH is set by the launcher script to ~/Library/Application Support/Obae/obae-app.db
    /// Set by the build script
    /// </summary>
    public static string GetDatabasePath()
    {
        var envPath = Environment.GetEnvironmentVariable("OBAE_DB_PATH");
        if (!string.IsNullOrEmpty(envPath))
        {
            return envPath;
        }
        
        // Fallback/original
        return "obae-app.db";
    }
    
    public static string GetConnectionString()
    {
        return $"Data Source={GetDatabasePath()}";
    }
    
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite(GetConnectionString());
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // base.OnModelCreating(modelBuilder);
        //
        // modelBuilder.Entity<AppSettings>(entity =>
        // {
        //     entity.HasKey(e => e.Id);
        //     
        //     var defaultMirrorSources = new List<MirrorSources>
        //     {
        //         MirrorSources.Nerinyan,
        //         MirrorSources.BeatConnect,
        //     };
        //
        //     entity.HasData(new AppSettings
        //     {
        //         Id = 1,
        //         Theme = Themes.Light,
        //         OsuCookieValue = null,
        //         DefaultFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Obae"),
        //         SaveSettingsToDatabase = false,
        //         SelectedMirrorSourcesJson = JsonSerializer.Serialize(defaultMirrorSources),
        //         LastUpdatedUtc = DateTime.UtcNow
        //     });
        // });
    }
    
}