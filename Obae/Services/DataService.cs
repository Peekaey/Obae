using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Obae.Interfaces;
using Obae.Models;

namespace Obae.Services;

public class DataService : IDataService
{
    private readonly ILogger<DataService> _logger;
    private readonly DataContext _dataContext;

    public DataService(ILogger<DataService> logger, DataContext dataContext)
    {
        _logger = logger;
        _dataContext = dataContext;
    }

    public async Task<SaveResult> SaveSettingsToDatabase(CachedAppSettings cachedAppSettings)
    {
        try
        {
            using (var transactionAsync = await _dataContext.Database.BeginTransactionAsync())
            {
                // It is ok to use FirstOrDefault as we assume that only 1 record for app settings exists
                // -- Seeded at initial launch if it doesn't exist
                var existingSettings = await _dataContext.AppSettings.FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(cachedAppSettings.OsuCookieValue))
                {
                    existingSettings.OsuCookieValue = string.Empty;
                }
                else
                {
                    existingSettings.OsuCookieValue = EncryptionHelper.Encrypt(cachedAppSettings.OsuCookieValue,cachedAppSettings.EncryptionKeyByteArray).EncryptedData;
                }
                
                existingSettings.SelectedTheme = cachedAppSettings.SelectedTheme;
                existingSettings.DefaultFolderPath = cachedAppSettings.DefaultFolderPath;
                existingSettings.SaveSettingsToDatabase = cachedAppSettings.SaveSettingsToDatabase;
                existingSettings.LastUpdatedUtc = DateTime.UtcNow;
                existingSettings.SelectedMirrorSourcesJson = JsonSerializer.Serialize(cachedAppSettings.SelectedMirrorSources);
                await _dataContext.SaveChangesAsync();
                await transactionAsync.CommitAsync();
                return SaveResult.AsSuccess();
            }
        } catch (Exception ex)
        {
            Debug.WriteLine($"Failed to save appsettings: {ex.Message}");
            return SaveResult.AsFailure($"Failed to save appsettings: {ex.Message}");
        }
    }
    
}