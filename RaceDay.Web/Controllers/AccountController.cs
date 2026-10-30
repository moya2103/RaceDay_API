using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RaceDay.Web.Helpers;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class AccountController : Controller
{
    private readonly IApiClient _apiClient;

    public AccountController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (HttpContext.Session.IsAuthenticated())
            return RedirectToAction("Index", "Home");

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginDto model)
    {
        if (!ModelState.IsValid) return View(model);

        var response = await _apiClient.PostAsync("api/auth/login", model);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<UserResponseDto>(content);

            if (user != null)
            {
                // Store user info in MVC Session
                HttpContext.Session.SetUser(user.UserID, user.Role, user.FullName);

                if (user.Role == "Organiser")
                    return RedirectToAction("Dashboard", "Organiser");
                else
                    return RedirectToAction("Index", "Home");
            }
        }

        ModelState.AddModelError("", "Invalid email or password.");
        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (HttpContext.Session.IsAuthenticated())
            return RedirectToAction("Index", "Home");

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterDto model)
    {
        if (!ModelState.IsValid) return View(model);

        var response = await _apiClient.PostAsync("api/auth/register", model);

        if (response.IsSuccessStatusCode)
        {
            // Automatically log them in after registering
            var loginDto = new LoginDto { Email = model.Email, Password = model.Password };
            return await Login(loginDto);
        }

        var error = await response.Content.ReadAsStringAsync();
        ModelState.AddModelError("", "Registration failed. " + error);
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _apiClient.PostAsync("api/auth/logout", new { });
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}
