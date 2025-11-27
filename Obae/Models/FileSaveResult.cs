namespace Obae.Models;

public class FileSaveResult : ServiceResult
{
    public string? SavePath { get; set; }

    public FileSaveResult(bool isSuccess, string? savePath = null, string? errorMessage = null)
        : base(isSuccess, errorMessage)
    {
        SavePath = savePath;
    }

    public static FileSaveResult AsFileSaveSuccess(string savePath)
    {
        return new FileSaveResult(true, savePath);
    }

    public static FileSaveResult AsFileSaveFailure(string errorMessage)
    {
        return new FileSaveResult(false, null, errorMessage);
    }
}