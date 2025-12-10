using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Obae.Helpers;
using Obae.Interfaces;
using Obae.Models;

namespace Obae.Services;

public class FileSystemSystemService : IFileSystemService
{
    
    public ServiceResult CreateWorkingDirectory(string folderPath)
    {
        try
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            return ServiceResult.AsSuccess();
        }
        catch (IOException e)
        {
            return ServiceResult.AsFailure(e.Message);
        }
    }
    
    public ServiceResult RemoveFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            return ServiceResult.AsSuccess();
        }
        catch (IOException e)
        {
            return ServiceResult.AsFailure(e.Message);
        }
    }

    public List<MemoryStream>? GetImagesFromBeatmap(string beatmapFilePath)
    {
        //Change file extension from .osz to .zip
        try
        {
            var zipFilePath = beatmapFilePath.Replace(".osz", ".zip");
            
            if (!File.Exists(zipFilePath))
            {
                File.Move(beatmapFilePath, zipFilePath);
            }
            var images = GetImageContentsFromZip(zipFilePath);
            
            // Delete zip file & osz file after extracting images to memory as we no longer need download
            // Could potentially just leave file to save on identical future calls
            // But better to clean up after ourselves as the savings are minial
            RemoveFile(zipFilePath);
            RemoveFile(beatmapFilePath);
            return images;
        }
        catch (IOException e)
        {
            return null;
        }
    }
        
    private List<MemoryStream> GetImageContentsFromZip(string zipFilePath)
    {
        // Look through zip path for files with .png or jpg extension
        var imageStreams = new List<MemoryStream>();
        
        using (var zipArchive = ZipFile.OpenRead(zipFilePath))
        {
            foreach (var entry in zipArchive.Entries)
            {
                // Check if the entry's name ends with .png or .jpg (case-insensitive)
                // && Skip Lyric Folder

                if (entry.FullName.ToLower().Contains("sb/") && !entry.FullName.ToLower().Contains("bg/"))
                {
                    continue;
                }
                
                if (entry.FullName.ToLower().Contains("lyric"))
                {
                    continue;
                }
                if (IsImageFile(entry.FullName))
                {
                    var memoryStream = new MemoryStream();
    
                    using (var entryStream = entry.Open())
                    {
                        entryStream.CopyTo(memoryStream);
                    }
                    memoryStream.Position = 0;
                    imageStreams.Add(memoryStream);
                }
            }
        }
        return imageStreams;
    }
    
    private bool IsImageFile(string fileName)
    {
        return fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase);
    }
    
    public async Task<FileSaveResult> SaveImageToDisk(IStorageFile storageFile, Bitmap imageBitmap)
    {
        try
        {
            await Task.Run(() =>
            {
                using var fileStream = File.Create(storageFile.Path.AbsolutePath);
                imageBitmap.Save(fileStream);
            });
            Console.WriteLine($"Saved image to {storageFile.Path.AbsolutePath}");
            return FileSaveResult.AsFileSaveSuccess(storageFile.Path.AbsolutePath);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error saving image: {e.Message}");
            return FileSaveResult.AsFileSaveFailure(e.Message);
        }
    }
    
    public async Task<string?> ShowFolderPickerAsync()
    {
        var folders = await Application.Current.GetTopLevel().StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Working Directory Folder",
            AllowMultiple = false
        });

        return folders.Count > 0 ? folders[0].Path.LocalPath : null;
    }
    
        
}