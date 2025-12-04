namespace Obae.Models;

public class DownloadServiceResult 
{
    public string SavedBeatmapPath { get; set; }
    public string BeatmapName { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    
    public DownloadServiceResult(bool isSuccess, string? errorMessage = null, string? savedBeatmapPath = null, string? beatmapName = null)
    {
        Success = isSuccess;
        ErrorMessage = errorMessage;
        SavedBeatmapPath = savedBeatmapPath;
        beatmapName = beatmapName;
    }
    
    public static DownloadServiceResult AsSuccess(string savedBeatmapPath, string beatmapName)
    {
        return new DownloadServiceResult(true)
        {
            SavedBeatmapPath = savedBeatmapPath,
            BeatmapName = beatmapName,
            Success = true
        };
    }
    
    public static DownloadServiceResult AsFailure(string errorMessage)
    {
        return new DownloadServiceResult(false, errorMessage);
    }
    
}