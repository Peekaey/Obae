using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Text.Json;
using Obae.Models.Enums;

namespace Obae.Models.Database;

public class AppSettings
{
    public int Id { get; set; }
    public Themes SelectedTheme { get; set; }
    public string? OsuCookieValue { get; set; }
    public string DefaultFolderPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Obae");
    public bool SaveSettingsToDatabase { get; set; }
    public string SelectedMirrorSourcesJson { get; set; } = "[]";
    public DateTime LastUpdatedUtc { get; set; }

    [NotMapped]
    public List<MirrorSources> SelectedMirrorSources
    {
        get => JsonSerializer.Deserialize<List<MirrorSources>>(SelectedMirrorSourcesJson) ?? new List<MirrorSources>();
        set => SelectedMirrorSourcesJson = JsonSerializer.Serialize(value);
    }
}