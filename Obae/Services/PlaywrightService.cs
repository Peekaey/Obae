using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Obae.Interfaces;
using Obae.Models;

namespace Obae.Services;

public class PlaywrightService : IPlaywrightService
{

    public async Task<PlaywrightServiceResult> DownloadBeatmap(string url, UserCookie userCookie, string downloadPath)
    {
        var playwright = await Playwright.CreateAsync();
        IBrowser? browser = null;
        //TODO If application hangs on Downloading Beatmap, Playwright probably not been ininitialised properly
        //Ensure that that playwright.ps1 script is executed - install powershell if on macos to execute
        try
        {
            browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false
            });
        }
        catch (Exception e)
        {
            return PlaywrightServiceResult.AsFailure("Crashed when attempting to launch chromium browser. Please ensure that Playwright has been installed correctly" +
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
            return PlaywrightServiceResult.AsFailure("Beatmap not found");
        }
        
        var element = await page.QuerySelectorAsync("a.btn-osu-big.btn-osu-big--beatmapset-header");
        if (element != null)
        {
            await page.WaitForSelectorAsync("a.btn-osu-big.btn-osu-big--beatmapset-header");
        }
        else
        {
            return PlaywrightServiceResult.AsFailure("Unable to find download button. Check cookie value");
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
            return PlaywrightServiceResult.AsFailure("Failed to download beatmap");
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
                return PlaywrightServiceResult.AsSuccess(combinedDownloadPath, Path.GetFileNameWithoutExtension(headerFileName));
            }
        }
        catch (Exception e)
        {
            await browser.CloseAsync();
            return new PlaywrightServiceResult(false, e.Message);
        }
    }
}
