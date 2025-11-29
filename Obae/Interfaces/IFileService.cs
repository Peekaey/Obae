using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Obae.Models;

namespace Obae.Interfaces;

public interface IFileService
{
    ServiceResult CreateWorkingDirectory(string folderPath);
    List<MemoryStream>? GetImagesFromBeatmap(string beatmapFilePath);
    Task<FileSaveResult> SaveImageToDisk(IStorageFile storageFile, Bitmap imageBitmap);

}