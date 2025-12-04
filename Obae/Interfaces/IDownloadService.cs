using System.Threading.Tasks;
using Obae.Models;

namespace Obae.Interfaces;

public interface IDownloadService
{
    Task<DownloadServiceResult> DownloadBeatmapFromOfficial(string url, UserCookie userCookie, string downloadPath);
    Task<DownloadServiceResult> DownloadBeatmapFromThirdParty(string url, string downloadPath);
}