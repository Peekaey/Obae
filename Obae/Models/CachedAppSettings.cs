using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Obae.Models.Enums;

namespace Obae.Models;

public class CachedAppSettings
{
    public string DefaultFolderPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Obae");
    public Themes SelectedTheme { get; set; } = Themes.Light;
    public string OsuCookieValue { get; set; } = string.Empty;
    public bool SaveSettingsToDatabase { get; set; } = false;

    public List<MirrorSources> SelectedMirrorSources { get; set; } = new List<MirrorSources>
    {
        MirrorSources.Nerinyan,
        MirrorSources.BeatConnect
    };
}