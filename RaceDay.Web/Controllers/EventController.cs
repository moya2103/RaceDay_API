using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RaceDay.Web.Helpers;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class EventController : Controller
{
    private readonly IApiClient _apiClient;

    public EventController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var response = await _apiClient.GetAsync($"api/events/{id}");

        if (!response.IsSuccessStatusCode)
            return NotFound();

        var content = await response.Content.ReadAsStringAsync();
        var eventDetails = JsonConvert.DeserializeObject<EventDetailsDto>(content);

        return View(eventDetails);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol(int eventId, int categoryId)
    {
        if (!HttpContext.Session.IsAuthenticated() || HttpContext.Session.GetRole() != "Participant")
        {
            return RedirectToAction("Login", "Account");
        }

        var enrolData = new EnrolDto { CategoryId = categoryId };
        var response = await _apiClient.PostAsync($"api/events/{eventId}/enrol", enrolData);

        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "Successfully enrolled! Get ready for race day.";
            return RedirectToAction("MyEvents", "Participant");
        }

        var error = await response.Content.ReadAsStringAsync();
        TempData["ErrorMessage"] = "Failed to enrol. You may already be enrolled in this event.";
        return RedirectToAction("Details", new { id = eventId });
    }
}