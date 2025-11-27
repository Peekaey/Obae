using System.Collections.Generic;
using System.IO;
using Avalonia.Media.Imaging;
using Obae.Interfaces;

namespace Obae.Helpers;

public class ImageHelpers : IImageHelpers
{
    public List<Bitmap> ConvertMemoryStreamToBitmap(List<MemoryStream> beatmapImages)
    {
        var beatmapImagesBitmap = new List<Bitmap>();
        foreach (var image in beatmapImages)
        {
            image.Position = 0;
            var bitmap = new Bitmap(image);
            beatmapImagesBitmap.Add(bitmap);
        }
        return beatmapImagesBitmap;
    }
    
    public MemoryStream ConvertBitmapToMemoryStream(Bitmap bitmap)
    {
        var memoryStream = new MemoryStream();
        bitmap.Save(memoryStream);
        memoryStream.Position = 0;
        return memoryStream;
        
    }
}