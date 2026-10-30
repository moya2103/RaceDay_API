using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RaceDay.Web.Models;
using RaceDay.Web.Services;
using System.Diagnostics;

namespace RaceDay.Web.Controllers;

public class HomeController : Controller
{
    private readonly IApiClient _apiClient;

    public HomeController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        // Fetch events from the API
        var response = await _apiClient.GetAsync("api/events");
        var events = new List<EventDto>();

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            events = JsonConvert.DeserializeObject<List<EventDto>>(content) ?? new List<EventDto>();
        }

        return View(events);
    }

    public IActionResult Privacy()
    {
        return View();
    }
}