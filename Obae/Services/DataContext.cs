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
    
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Fallback connection string if not configured via DI
            optionsBuilder.UseSqlite("Data Source=obae-app.db");
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