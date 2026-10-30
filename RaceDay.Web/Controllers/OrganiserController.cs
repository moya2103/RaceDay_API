using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RaceDay.Web.Helpers;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class OrganiserController : Controller
{
    private readonly IApiClient _apiClient;

    public OrganiserController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Dashboard()
    {
        if (HttpContext.Session.GetRole() != "Organiser")
            return RedirectToAction("Login", "Account");

        var response = await _apiClient.GetAsync("api/events");
        var allEvents = new List<EventDto>();

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            allEvents = JsonConvert.DeserializeObject<List<EventDto>>(content) ?? new List<EventDto>();
        }

        
        return View(allEvents);
    }

    [HttpGet]
    public IActionResult CreateEvent()
    {
        if (HttpContext.Session.GetRole() != "Organiser")
            return RedirectToAction("Login", "Account");

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateEvent(CreateEventDto model)
    {
        if (HttpContext.Session.GetRole() != "Organiser")
            return RedirectToAction("Login", "Account");

        if (!ModelState.IsValid) return View(model);

        // Prepare Multipart form data for the API
        var multipartContent = new MultipartFormDataContent();
        multipartContent.Add(new StringContent(model.Name), "Name");
        multipartContent.Add(new StringContent(model.Description), "Description");
        multipartContent.Add(new StringContent(model.EventDate.ToString("yyyy-MM-ddTHH:mm:ss")), "EventDate");
        multipartContent.Add(new StringContent(model.Location), "Location");
        multipartContent.Add(new StringContent(model.Distance.ToString()), "Distance");
        multipartContent.Add(new StringContent(model.EventType), "EventType");

        if (model.BannerImage != null && model.BannerImage.Length > 0)
        {
            var stream = model.BannerImage.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(model.BannerImage.ContentType);
            multipartContent.Add(fileContent, "bannerImage", model.BannerImage.FileName);
        }

        var response = await _apiClient.PostMultipartAsync("api/events", multipartContent);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var createdEvent = JsonConvert.DeserializeObject<EventDto>(content);

            TempData["SuccessMessage"] = "Event created successfully!";
            if (createdEvent != null)
                return RedirectToAction("ManageCategories", new { eventId = createdEvent.EventID });
        }

        // indicates error if any
        var errorContent = await response.Content.ReadAsStringAsync();
        ModelState.AddModelError("", $"API Error ({(int)response.StatusCode}): {errorContent}");

        return View(model);
    }

    public class CreateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public decimal? Distance { get; set; }
        public decimal? EntryFee { get; set; }
    }

    [HttpGet]
    public async Task<IActionResult> ManageCategories(int eventId)
    {
        if (HttpContext.Session.GetRole() != "Organiser")
            return RedirectToAction("Login", "Account");

        // Fetch the event details
        var eventResp = await _apiClient.GetAsync($"api/events/{eventId}");
        var eventDetails = new EventDetailsDto();
        if (eventResp.IsSuccessStatusCode)
        {
            var content = await eventResp.Content.ReadAsStringAsync();
            eventDetails = JsonConvert.DeserializeObject<EventDetailsDto>(content) ?? new EventDetailsDto();
        }

        ViewBag.EventId = eventId;
        ViewBag.EventName = eventDetails.Name;

        return View(eventDetails.Categories);
    }

    [HttpPost]
    public async Task<IActionResult> AddCategory(int eventId, CreateCategoryDto model)
    {
        if (HttpContext.Session.GetRole() != "Organiser")
            return RedirectToAction("Login", "Account");

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please check the category data.";
            return RedirectToAction("ManageCategories", new { eventId });
        }

        var response = await _apiClient.PostAsync($"api/events/{eventId}/categories", model);

        if (response.IsSuccessStatusCode)
            TempData["SuccessMessage"] = "Category added successfully!";
        else
            TempData["ErrorMessage"] = "Failed to add category.";

        return RedirectToAction("ManageCategories", new { eventId });
    }

    [HttpGet]
    public async Task<IActionResult> ViewEnrolments(int eventId)
    {
        if (HttpContext.Session.GetRole() != "Organiser")
            return RedirectToAction("Login", "Account");

        // Get event name for the header
        var eventResp = await _apiClient.GetAsync($"api/events/{eventId}");
        if (eventResp.IsSuccessStatusCode)
        {
            var evContent = await eventResp.Content.ReadAsStringAsync();

            // THE FIX IS HERE: We use a dynamic object to just grab the name safely
            var evDynamic = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(evContent);
            ViewBag.EventName = evDynamic?["name"]?.ToString() ?? "Unknown Event";
        }
        ViewBag.EventId = eventId;

        // Get enrolments
        var response = await _apiClient.GetAsync($"api/events/{eventId}/enrolments");
        var enrolments = new List<OrganiserEnrolmentDto>();

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            enrolments = JsonConvert.DeserializeObject<List<OrganiserEnrolmentDto>>(content) ?? new List<OrganiserEnrolmentDto>();
        }

        return View(enrolments);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateEnrolmentStatus(int enrolmentId, int eventId, string status)
    {
        var response = await _apiClient.PutAsync($"api/enrolments/{enrolmentId}/status", new UpdateStatusDto { Status = status });

        if (response.IsSuccessStatusCode)
            TempData["SuccessMessage"] = "Status updated successfully.";
        else
            TempData["ErrorMessage"] = "Failed to update status.";

        return RedirectToAction("ViewEnrolments", new { eventId });
    }

    [HttpGet]
    public IActionResult CaptureResult(int enrolmentId, int eventId)
    {
        if (HttpContext.Session.GetRole() != "Organiser")
            return RedirectToAction("Login", "Account");

        ViewBag.EnrolmentId = enrolmentId;
        ViewBag.EventId = eventId;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CaptureResult(int enrolmentId, int eventId, CreateResultDto model)
    {
        var response = await _apiClient.PostAsync($"api/enrolments/{enrolmentId}/result", model);

        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "Result captured successfully!";
            return RedirectToAction("ViewEnrolments", new { eventId });
        }

        TempData["ErrorMessage"] = "Failed to capture result. It may already be recorded.";
        return RedirectToAction("CaptureResult", new { enrolmentId, eventId });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteEvent(int eventId)
    {
        if (HttpContext.Session.GetRole() != "Organiser")
            return RedirectToAction("Login", "Account");

        var response = await _apiClient.DeleteAsync($"api/events/{eventId}");

        if (response.IsSuccessStatusCode)
            TempData["SuccessMessage"] = "Event deleted successfully.";
        else
            TempData["ErrorMessage"] = "Failed to delete event.";

        return RedirectToAction("Dashboard");
    }
}
