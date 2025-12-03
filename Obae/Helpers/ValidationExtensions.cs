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
        
        // Means that the user returned beatmapID directly and parses as an integer fine
        // We do not need to do anything further.
        if (beatmapInput.Length == 6 && int.TryParse(beatmapInput, out _))
        {
            return beatmapInput;
        }
    
        // Gets beatmapset Id from full url - ie https://osu.ppy.sh/beatmapsets/371128#osu/814293
        if (beatmapInput.Contains("https://osu.ppy.sh/beatmapsets/"))
        {
            try
            {
                var delimitedInput = beatmapInput.Split("https://osu.ppy.sh/beatmapsets/");
                var takeFirstSix = delimitedInput[1].Substring(0, 6);
                if (takeFirstSix.Length == 6 && int.TryParse(takeFirstSix, out _))
                {
                    return takeFirstSix;
                }
            }
            catch (Exception ex)
            {
                // Swallow exception, return nothing
            }
        }

        return null;
    }
}