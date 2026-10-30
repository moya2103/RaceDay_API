using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RaceDay.Web.Helpers;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class ParticipantController : Controller
{
    private readonly IApiClient _apiClient;

    public ParticipantController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> MyEvents()
    {
        if (HttpContext.Session.GetRole() != "Participant")
            return RedirectToAction("Index", "Home");

        var response = await _apiClient.GetAsync("api/enrolments/my");
        var enrolments = new List<MyEnrolmentDto>();

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            enrolments = JsonConvert.DeserializeObject<List<MyEnrolmentDto>>(content) ?? new List<MyEnrolmentDto>();
        }

        return View(enrolments);
    }

    public async Task<IActionResult> MyResults()
    {
        if (HttpContext.Session.GetRole() != "Participant")
            return RedirectToAction("Index", "Home");

        var response = await _apiClient.GetAsync("api/results/my");
        var results = new List<ParticipantResultDto>();

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            results = JsonConvert.DeserializeObject<List<ParticipantResultDto>>(content) ?? new List<ParticipantResultDto>();
        }

        return View(results);
    }
}