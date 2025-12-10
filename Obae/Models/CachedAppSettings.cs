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
    private readonly string _encryptionKey = "K7vJZq3mR9nX2wT8pL5hF4sA6bN1cM0yU3xV7jW9gE8=";

    public byte[] EncryptionKeyByteArray => Convert.FromBase64String(_encryptionKey);

    public List<MirrorSources> SelectedMirrorSources { get; set; } = new List<MirrorSources>
    {
        MirrorSources.Nerinyan,
        MirrorSources.BeatConnect
    };
}