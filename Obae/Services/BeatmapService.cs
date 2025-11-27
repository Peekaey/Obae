using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Obae.Interfaces;
using Obae.Models;

namespace Obae.Services;

public class BeatmapService : IBeatmapService
{
    private readonly string _baseBeatmapUrl = "https://osu.ppy.sh/beatmapsets/";
    private readonly IApiManagerService _apiManagerService;
    private readonly IPlaywrightService _playwrightService;
    private readonly IFileService _fileService;
    
    public BeatmapService(IApiManagerService apiManagerService, IPlaywrightService playwrightService, IFileService fileService)
    {
        _apiManagerService = apiManagerService;
        _playwrightService = playwrightService;
        _fileService = fileService;
    }
    
    public async Task<DownloadResult> DownloadBeatmap(string textInput, string defaultFolderPath, string cookie)
    {
        if (string.IsNullOrEmpty(cookie))
        {
            return DownloadResult.AsFailure("Cookie value has not been supplied");
        }
        
        var workingDirectoryResult = _fileService.CreateWorkingDirectory(defaultFolderPath);
        
        if (workingDirectoryResult.Success == false)
        {
            return DownloadResult.AsFailure("Unable to create working directory before downloading beatmap");
        }
        
        var userCookie = new UserCookie
        {
            Value = cookie
        };
        
        var downloadUrl = CreateDownloadUrl(textInput);
        var downloadResult = await _playwrightService.DownloadBeatmap(downloadUrl, userCookie,  defaultFolderPath);

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
    
    private string CreateDownloadUrl(string textInput)
    {
        return $"{_baseBeatmapUrl}{textInput}/download";
    }
}