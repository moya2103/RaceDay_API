using System.Text;
using Newtonsoft.Json;

namespace RaceDay.Web.Services;

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _sessionKey = "RaceDaySessionCookie";

    public ApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, IConfiguration config)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;

        // Set the base URL from appsettings
        _httpClient.BaseAddress = new Uri(config["ApiSettings:BaseUrl"]!);
    }

    // Helper to attach the session cookie from the browser to the API request
    private void AttachSessionCookie()
    {
        var sessionCookie = _httpContextAccessor.HttpContext?.Session.GetString(_sessionKey);
        if (!string.IsNullOrEmpty(sessionCookie))
        {
            _httpClient.DefaultRequestHeaders.Remove("Cookie");
            _httpClient.DefaultRequestHeaders.Add("Cookie", sessionCookie);
        }
    }

    // Helper to save the session cookie from the API response to the browser session
    private void SaveSessionCookie(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            var sessionCookie = cookies.FirstOrDefault(c => c.Contains(".RaceDay.Session"));
            if (sessionCookie != null)
            {
                // Extract just the cookie name and value
                var cookieValue = sessionCookie.Split(';')[0];
                _httpContextAccessor.HttpContext?.Session.SetString(_sessionKey, cookieValue);
            }
        }
    }

    public async Task<HttpResponseMessage> GetAsync(string endpoint)
    {
        AttachSessionCookie();
        var response = await _httpClient.GetAsync(endpoint);
        SaveSessionCookie(response);
        return response;
    }

    public async Task<HttpResponseMessage> PostAsync(string endpoint, object data)
    {
        AttachSessionCookie();
        var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(endpoint, content);
        SaveSessionCookie(response);
        return response;
    }

    public async Task<HttpResponseMessage> PutAsync(string endpoint, object data)
    {
        AttachSessionCookie();
        var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync(endpoint, content);
        SaveSessionCookie(response);
        return response;
    }

    public async Task<HttpResponseMessage> DeleteAsync(string endpoint)
    {
        AttachSessionCookie();
        var response = await _httpClient.DeleteAsync(endpoint);
        SaveSessionCookie(response);
        return response;
    }

    public async Task<HttpResponseMessage> PostFileAsync(string endpoint, IFormFile file)
    {
        AttachSessionCookie();
        var content = new MultipartFormDataContent();
        var stream = file.OpenReadStream();
        content.Add(new StreamContent(stream), "file", file.FileName);

        var response = await _httpClient.PostAsync(endpoint, content);
        SaveSessionCookie(response);
        return response;
    }

    public async Task<HttpResponseMessage> PostMultipartAsync(string endpoint, MultipartFormDataContent content)
    {
        AttachSessionCookie();
        var response = await _httpClient.PostAsync(endpoint, content);
        SaveSessionCookie(response);
        return response;
    }
}
