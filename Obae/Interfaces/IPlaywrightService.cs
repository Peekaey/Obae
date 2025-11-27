using System.Threading.Tasks;
using Obae.Models;

namespace Obae.Interfaces;

public interface IPlaywrightService
{
    Task<PlaywrightServiceResult> DownloadBeatmap(string url, UserCookie userCookie, string downloadPath);
}