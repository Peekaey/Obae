using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
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
    private readonly IDownloadService _downloadService;
    private readonly IFileSystemService _fileSystemService;
    private readonly ILogger<BeatmapService> _logger;
    private readonly CachedAppSettings _cachedAppSettings;
    
    public BeatmapService(IDownloadService downloadService, IFileSystemService fileSystemService, ILogger<BeatmapService> logger
        , CachedAppSettings cachedAppSettings)
    {
        _downloadService = downloadService;
        _fileSystemService = fileSystemService;
        _logger = logger;
        _cachedAppSettings = cachedAppSettings;
    }
    
    public async Task<DownloadResult> DownloadBeatmapOrchestrator(string beatmapId, string defaultFolderPath, string cookie)
    {
        var workingDirectoryResult = _fileSystemService.CreateWorkingDirectory(defaultFolderPath);

        if (!workingDirectoryResult.Success)
        {
            return DownloadResult.AsFailure("Unable to create working directory before downloading beatmap");
        }

        DownloadServiceResult? downloadResult = null;
    
        if (!string.IsNullOrEmpty(cookie))
        {
            var userCookie = new UserCookie { Value = cookie };
            var downloadUrl = CreateDownloadUrl(beatmapId, null);
            downloadResult = await _downloadService.DownloadBeatmapFromOfficial(downloadUrl, userCookie, defaultFolderPath);
        }
        else
        {
            // Try each mirror with timeout
            var selectedMirrors = _cachedAppSettings.SelectedMirrorSources;
            var perMirrorTimeout = TimeSpan.FromMinutes(1);
            
            foreach (var mirrorSource in selectedMirrors)
            {
                var downloadUrl = CreateDownloadUrl(beatmapId, mirrorSource);

                // Create a timeout token for this specific mirror attempt
                using (var mirrorCts = new CancellationTokenSource(perMirrorTimeout))
                { 
                    mirrorCts.CancelAfter(perMirrorTimeout);

                    try
                    {
                        downloadResult = await _downloadService.DownloadBeatmapFromThirdParty(downloadUrl, defaultFolderPath, mirrorCts.Token
                        );

                        if (downloadResult.Success)
                        {
                            break; // Success, don't try other mirrors
                        }

                    }
                    catch (Exception ex)
                    {
                        var something = "";
                        // Swallow, continue to next
                    }
                }
            }

            // If we get here and downloadResult is still null or failed, all mirrors failed
            if (downloadResult == null || !downloadResult.Success)
            {
                return DownloadResult.AsFailure("All mirror sources failed to download the beatmap");
            }
        }
    
        if (downloadResult?.Success == true)
        {
            var images = _fileSystemService.GetImagesFromBeatmap(downloadResult.SavedBeatmapPath);
            return DownloadResult.AsSuccess(downloadResult.BeatmapName, images);
        }
        else
        {
            return DownloadResult.AsFailure(
                downloadResult?.ErrorMessage ?? "Download failed for unknown reason"
            );
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