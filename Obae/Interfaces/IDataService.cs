using System.Threading.Tasks;
using Obae.Models;

namespace Obae.Interfaces;

public interface IDataService
{
    Task<SaveResult> SaveSettingsToDatabase(CachedAppSettings cachedAppSettings);
}