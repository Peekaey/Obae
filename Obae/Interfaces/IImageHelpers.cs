using System.Collections.Generic;
using System.IO;
using Avalonia.Media.Imaging;

namespace Obae.Interfaces;

public interface IImageHelpers
{
    List<Bitmap> ConvertMemoryStreamToBitmap(List<MemoryStream> beatmapImages);
    MemoryStream ConvertBitmapToMemoryStream(Bitmap bitmap);
}