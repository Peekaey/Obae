using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Obae.Interfaces;
using Obae.Models;
using Obae.Models.Enums;

namespace Obae.Services;

public class BeatmapService : IBeatmapService
{
    private readonly string _officialBaseBeatmapUrl = "https://osu.ppy.sh/beatmapsets/";
    private readonly string _nerinyanBaseBeatmapUrl = "https://api.nerinyan.moe/d/";
    private readonly string _beatConnectBaseBeatmapUrl = "https://beatconnect.io/b/";
    private readonly string _osuDirectBaseBeatmapUrl = "https://osu.direct/api/d/";
    private readonly IApiManagerService _apiManagerService;
    private readonly IDownloadService _downloadService;
    private readonly IFileService _fileService;
    private readonly ILogger<BeatmapService> _logger;
    private readonly CachedAppSettings _cachedAppSettings;
    
    public BeatmapService(IApiManagerService apiManagerService, IDownloadService downloadService, IFileService fileService, ILogger<BeatmapService> logger
    , CachedAppSettings cachedAppSettings)
    {
        _apiManagerService = apiManagerService;
        _downloadService = downloadService;
        _fileService = fileService;
        _logger = logger;
        _cachedAppSettings = cachedAppSettings;
    }
    
    public async Task<DownloadResult> DownloadBeatmap(string beatmapId, string defaultFolderPath, string cookie)
    {
        // If OsuSessionCookie is provided
        // We want to query official mirror instead to save time and headache
        
        var workingDirectoryResult = _fileService.CreateWorkingDirectory(defaultFolderPath);

        if (workingDirectoryResult.Success == false)
        {
            return DownloadResult.AsFailure("Unable to create working directory before downloading beatmap");
        }

        DownloadServiceResult downloadResult = null;
        if (!string.IsNullOrEmpty(cookie))
        {
            var userCookie = new UserCookie
            {
                Value = cookie
            };
        
            var downloadUrl = CreateDownloadUrl(beatmapId, null);
            downloadResult = await _downloadService.DownloadBeatmapFromOfficial(downloadUrl, userCookie,  defaultFolderPath);
        }
        else
        {
            // Otherwise if no Value provided, we cannot query the official mirror, therefore we have to rely 
            // On third party mirrors - mirrors to search are in order from Nerinyan OsuDirect, > BeatConnect
            // And dependent on if user enabled which mirrors
            var selectedMirrors = _cachedAppSettings.SelectedMirrorSources;

            foreach (var mirrorSource in selectedMirrors)
            {
                var downloadUrl = CreateDownloadUrl(beatmapId, mirrorSource);
                downloadResult = await _downloadService.DownloadBeatmapFromThirdParty(downloadUrl, defaultFolderPath);
                if (downloadResult.Success == true)
                {
                    break;
                }
            }
        }
        
        if (downloadResult.Success)
        {
            var images = _fileService.GetImagesFromBeatmap(downloadResult.SavedBeatmapPath);
            return DownloadResult.AsSuccess(downloadResult.BeatmapName,images);
        }
        else
        {
            return DownloadResult.AsFailure(downloadResult.ErrorMessage);
        }
    }
    
    private string CreateDownloadUrl(string beatmapId, MirrorSources? mirrorSources)
    {
        if (mirrorSources == null)
        {
            return $"{_officialBaseBeatmapUrl}{beatmapId}/download";
        }

        if (mirrorSources == MirrorSources.Nerinyan)
        {
            return $"{_nerinyanBaseBeatmapUrl}{beatmapId}";
        }

        if (mirrorSources == MirrorSources.OsuDirect)
        {
            return $"{_osuDirectBaseBeatmapUrl}{beatmapId}";
        }
        
        // Last Mirror Source Left
        return $"{_beatConnectBaseBeatmapUrl}{beatmapId}";
        
    }
}