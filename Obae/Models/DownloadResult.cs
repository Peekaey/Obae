using System.Collections.Generic;
using System.IO;

namespace Obae.Models;

public class DownloadResult
{
    public bool Success { get; set; }
    public string StatusMessage { get; set; }
    public List<MemoryStream> Artworks { get; set; }
    
    public DownloadResult(bool isSuccess, string statusMessage, List<MemoryStream> artworks = null)
    {
        Success = isSuccess;
        StatusMessage = statusMessage;
        Artworks = artworks;
    }
    
    public static DownloadResult AsSuccess(string statusMessage, List<MemoryStream> artworks)
    {
        return new DownloadResult(true, statusMessage, artworks);
    }

    public static DownloadResult AsFailure(string statusMessage)
    {
        return new DownloadResult(false, statusMessage);
    }
}