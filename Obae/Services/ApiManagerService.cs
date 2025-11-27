using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Obae.Interfaces;
using Obae.Models;

namespace Obae.Services;

public class ApiManagerService : IApiManagerService
{
    private readonly ILogger<ApiManagerService> _logger;
    private readonly HttpClient _httpClient;
    private readonly CookieContainer _cookieContainer;
    
    // Unused Service - Can remove when wanted
    public ApiManagerService(ILogger<ApiManagerService> logger, HttpClient httpClient, CookieContainer cookieContainer)
    {
        _logger = logger;
        _httpClient = httpClient;
        _cookieContainer = cookieContainer;
    }

    public async Task<T> Get<T>(string url)
    {
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"Failed to get data from {url}");
            return default;
        }
    

        if(response.Headers.Location != null)
        {
            string location = response.Headers.Location.ToString();
            _logger.LogInformation($"Redirect Location: {location}");
        }
        
        if (response.Headers.TryGetValues("cache-control", out var cacheControlValues))
        {
            string cacheControl = cacheControlValues.FirstOrDefault();
            _logger.LogInformation($"Cache-Control: {cacheControl}");
        }
        
        var data = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(data);
    }

    public void AddUserCookie(UserCookie userCookie)
    {
        var cookie = new Cookie
        {
            Name = userCookie.Name,
            Domain = userCookie.Domain,
            Path = userCookie.Path,
            Expires = userCookie.Expires
        };
        _cookieContainer.Add(cookie);
    }
    
    
}