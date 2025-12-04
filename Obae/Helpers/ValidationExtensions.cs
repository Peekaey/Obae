using System;

namespace Obae.Helpers;

public static class ValidationExtensions
{
    public static string? ValidateBeatmapInput(this string? beatmapInput)
    {
        // Means user did not input anything
        if (beatmapInput == null)
        {
            return null;
        }

        var beatmapId = string.Empty;
    
        // Gets beatmapset Id from full url - ie https://osu.ppy.sh/beatmapsets/371128#osu/814293
        if (beatmapInput.Contains("https://osu.ppy.sh/beatmapsets/"))
        {
            try
            {
                var splitBaseUrl = beatmapInput.Split("https://osu.ppy.sh/beatmapsets/");
                var splitMiddle = splitBaseUrl[1].Split("#");
                beatmapId = splitMiddle[0];
            }
            catch (Exception ex)
            {
                // Swallow exception, return nothing
            }
        }

        if (!int.TryParse(beatmapId, out var _))
        {
            return null;
        }

        return beatmapId;
    }
}