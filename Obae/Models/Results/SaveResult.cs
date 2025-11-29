namespace Obae.Models;

public class SaveResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }

    public SaveResult(bool isSuccess, string? errorMessage = "null")
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public static SaveResult AsSuccess()
    {
        return new SaveResult(true, null);
    }

    public static SaveResult AsFailure(string? errorMessage)
    {
        return new SaveResult(false, errorMessage);
    }
}