using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Obae.Helpers;
using Obae.Interfaces;
using Obae.Models;
using Cookie = Microsoft.Playwright.Cookie;

namespace Obae.Services;

public class DownloadService : IDownloadService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DownloadService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<DownloadServiceResult> DownloadBeatmapFromOfficial(string url, UserCookie userCookie, string downloadPath)
    {
        var playwright = await Playwright.CreateAsync();
        IBrowser? browser = null;
        //TODO If application hangs on Downloading Beatmap, Playwright probably not been ininitialised properly
        //Ensure that that playwright.ps1 script is executed - install powershell if on macos to execute
        try
        {
            browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });
        }
        catch (Exception e)
        {
            return DownloadServiceResult.AsFailure("Crashed when attempting to launch chromium browser. Please ensure that Playwright has been installed correctly" +
                                                     "and that the Playwright.ps1 script has been executed - https://playwright.dev/dotnet/docs/intro");
        }

        // Create a new context
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { AcceptDownloads = true });

        var cookie = new Cookie
        {
            Name = "osu_session",
            Domain = ".ppy.sh",
            Path = "/",
            Value = userCookie.Value,
            SameSite = SameSiteAttribute.Lax,
            Secure = true,
            HttpOnly = true

        };

        // Add the cookie to the context
        await context.AddCookiesAsync(new[] { cookie });
        var page = await context.NewPageAsync();


        // Attach event handlers to observe all network requests/responses:
        List<string> networkRequestUrls = new List<string>();
        page.Request += (_, request) =>
        {
            Console.WriteLine($"[Request] {request.Method} {request.Url}");
            networkRequestUrls.Add(request.Url);
        };
        page.Response += async (_, response) => { Console.WriteLine($"[Response] {response.Status} {response.Url}"); };
        
        var initialResponse = await page.GotoAsync(url);

        if (initialResponse.Status == 404)
        {
            return DownloadServiceResult.AsFailure("Beatmap not found");
        }
        
        var element = await page.QuerySelectorAsync("a.btn-osu-big.btn-osu-big--beatmapset-header");
        if (element != null)
        {
            await page.WaitForSelectorAsync("a.btn-osu-big.btn-osu-big--beatmapset-header");
        }
        else
        {
            return DownloadServiceResult.AsFailure("Unable to find download button. Check cookie value");
        }
        
        // Click Download Button to Trigger
        await page.ClickAsync("a.btn-osu-big.btn-osu-big--beatmapset-header");

        // Scan Network Requests/Responses To Find Mirror Url
        var downloadUrl = networkRequestUrls.Find(x => x.Contains("osumirror.idle.host"));

        using var handler = new HttpClientHandler();
        using var client = new HttpClient(handler);
        
        using var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
        if (!response.IsSuccessStatusCode)
        {
            return DownloadServiceResult.AsFailure("Failed to download beatmap");
        }
            
        var headerFileName = response.Content.Headers.ContentDisposition?.FileName.Replace("\"", "") ?? "beatmap.zip";
        var combinedDownloadPath = Path.Combine(downloadPath, headerFileName);
        try 
        {
            await using (var fileStream = new FileStream(combinedDownloadPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await response.Content.CopyToAsync(fileStream);
                await fileStream.FlushAsync(); // Ensure the file stream is flushed
                await browser.CloseAsync();
                return DownloadServiceResult.AsSuccess(combinedDownloadPath, Path.GetFileNameWithoutExtension(headerFileName));
            }
        }
        catch (Exception e)
        {
            await browser.CloseAsync();
            return new DownloadServiceResult(false, e.Message);
        }
    }

    public async Task<DownloadServiceResult> DownloadBeatmapFromThirdParty(string url, string downloadPath)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return DownloadServiceResult.AsFailure("Beatmap not found");
                }
                else
                {
                    return DownloadServiceResult.AsFailure("Failed to download beatmap");
                }
            }

            var headerFileName = response.Content.Headers.ContentDisposition?.FileName.Replace("\"", "") ??
                                 response.RequestMessage.RequestUri.ToString().GetMapNameFromOsuDirectRequestUri();
            
            var combinedDownloadPath = Path.Combine(downloadPath, headerFileName);
            try 
            {
                await using (var fileStream = new FileStream(combinedDownloadPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(fileStream);
                    await fileStream.FlushAsync(); // Ensure the file stream is flushed
                    return DownloadServiceResult.AsSuccess(combinedDownloadPath, Path.GetFileNameWithoutExtension(headerFileName));
                }
            }
            catch (Exception e)
            {
                return new DownloadServiceResult(false, e.Message);
            }
        }
        catch (Exception e)
        {
            return DownloadServiceResult.AsFailure(e.Message);
        }


    }
}
