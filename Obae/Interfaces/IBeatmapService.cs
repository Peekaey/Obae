using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Obae.Models;

namespace Obae.Interfaces;

public interface IBeatmapService
{
    Task<DownloadResult> DownloadBeatmap(string beatmapId, string defaultFolderPath, string cookie);
}