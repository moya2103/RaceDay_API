using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Helpers;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/events/{eventId}/categories")]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public CategoriesController(RaceDayDbContext db) => _db = db;

    /// <summary>
    /// Lists all categories for an event. Public.
    /// </summary>
    /// <param name="eventId">Event ID</param>
    /// <returns>200 OK with categories; 404 if event not found</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll(int eventId)
    {
        if (!await _db.Events.AnyAsync(e => e.EventID == eventId))
            return NotFound(new { message = "Event not found" });

        var cats = await _db.Categories
            .Where(c => c.EventID == eventId)
            .Select(c => new
            {
                c.CategoryID,
                c.Name,
                c.Description,
                c.MinAge,
                c.MaxAge,
                c.Distance,
                c.EntryFee
            })
            .ToListAsync();

        return Ok(cats);
    }

    /// <summary>
    /// Creates a category under an event. Organiser must own the event.
    /// </summary>
    /// <param name="eventId">Event ID</param>
    /// <param name="dto">Category details</param>
    /// <returns>201 Created; 403 if not owner; 404 if event not found</returns>
    [HttpPost]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> Create(int eventId, [FromBody] CreateCategoryDto dto)
    {
        var userId = HttpContext.Session.GetUserId()!.Value;
        var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserID == userId);
        if (organiser == null) return StatusCode(403, new { message = "Forbidden" });

        var ev = await _db.Events.FindAsync(eventId);
        if (ev == null) return NotFound(new { message = "Event not found" });
        if (ev.OrganiserID != organiser.OrganiserID) return StatusCode(403, new { message = "Forbidden" });

        var cat = new Category
        {
            EventID = eventId,
            Name = dto.Name,
            Description = dto.Description,
            MinAge = dto.MinAge,
            MaxAge = dto.MaxAge,
            Distance = dto.Distance,
            EntryFee = dto.EntryFee,
            CreatedAt = DateTime.UtcNow
        };

        _db.Categories.Add(cat);
        await _db.SaveChangesAsync();

        return StatusCode(201, new { cat.CategoryID, cat.Name });
    }

    /// <summary>
    /// Updates a category. Organiser must own the parent event.
    /// </summary>
    /// <param name="eventId">Event ID</param>
    /// <param name="id">Category ID</param>
    /// <param name="dto">Updated fields</param>
    /// <returns>200 OK; 403 if not owner; 404 if not found</returns>
    [HttpPut("{id}")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> Update(int eventId, int id, [FromBody] UpdateCategoryDto dto)
    {
        var userId = HttpContext.Session.GetUserId()!.Value;
        var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserID == userId);
        if (organiser == null) return StatusCode(403, new { message = "Forbidden" });

        var ev = await _db.Events.FindAsync(eventId);
        if (ev == null || ev.OrganiserID != organiser.OrganiserID) return StatusCode(403, new { message = "Forbidden" });

        var cat = await _db.Categories.FirstOrDefaultAsync(c => c.CategoryID == id && c.EventID == eventId);
        if (cat == null) return NotFound(new { message = "Category not found" });

        cat.Name = dto.Name;
        cat.Description = dto.Description;
        cat.MinAge = dto.MinAge;
        cat.MaxAge = dto.MaxAge;
        cat.Distance = dto.Distance;
        cat.EntryFee = dto.EntryFee;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Category updated" });
    }

    /// <summary>
    /// Deletes a category. Organiser must own the parent event.
    /// </summary>
    /// <param name="eventId">Event ID</param>
    /// <param name="id">Category ID</param>
    /// <returns>204 No Content; 403 if not owner; 404 if not found</returns>
    [HttpDelete("{id}")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> Delete(int eventId, int id)
    {
        var userId = HttpContext.Session.GetUserId()!.Value;
        var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserID == userId);
        if (organiser == null) return StatusCode(403, new { message = "Forbidden" });

        var ev = await _db.Events.FindAsync(eventId);
        if (ev == null || ev.OrganiserID != organiser.OrganiserID) return StatusCode(403, new { message = "Forbidden" });

        var cat = await _db.Categories.FirstOrDefaultAsync(c => c.CategoryID == id && c.EventID == eventId);
        if (cat == null) return NotFound(new { message = "Category not found" });

        _db.Categories.Remove(cat);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}