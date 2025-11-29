namespace Obae.Models;

public class PlaywrightServiceResult 
{
    public string SavedBeatmapPath { get; set; }
    public string BeatmapName { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    
    public PlaywrightServiceResult(bool isSuccess, string? errorMessage = null, string? savedBeatmapPath = null, string? beatmapName = null)
    {
        Success = isSuccess;
        ErrorMessage = errorMessage;
        SavedBeatmapPath = savedBeatmapPath;
        beatmapName = beatmapName;
    }
    
    public static PlaywrightServiceResult AsSuccess(string savedBeatmapPath, string beatmapName)
    {
        return new PlaywrightServiceResult(true)
        {
            SavedBeatmapPath = savedBeatmapPath,
            BeatmapName = beatmapName,
            Success = true
        };
    }
    
    public static PlaywrightServiceResult AsFailure(string errorMessage)
    {
        return new PlaywrightServiceResult(false, errorMessage);
    }
    
}