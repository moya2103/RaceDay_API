using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Helpers;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EventsController(RaceDayDbContext db) => _db = db;

    /// <summary>
    /// Lists all active events. Public. Optional filters by eventType and start date.
    /// </summary>
    /// <param name="type">Optional event type filter: Run, Walk, or Cycle</param>
    /// <param name="from">Optional earliest event date</param>
    /// <returns>200 OK with list of events</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? type, [FromQuery] DateTime? from)
    {
        var query = _db.Events
            .Include(e => e.Organiser).ThenInclude(o => o.User)
            .Where(e => e.IsActive);

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(e => e.EventType == type);

        if (from.HasValue)
            query = query.Where(e => e.EventDate >= from);

        var events = await query
            .OrderBy(e => e.EventDate)
            .Select(e => new
            {
                e.EventID,
                e.Name,
                e.Description,
                e.EventDate,
                e.Location,
                e.Distance,
                e.EventType,
                e.BannerImage,
                Organiser = e.Organiser.User.FullName
            })
            .ToListAsync();

        return Ok(events);
    }

    /// <summary>
    /// Gets a single event with its categories. Public.
    /// </summary>
    /// <param name="id">Event ID</param>
    /// <returns>200 OK with event; 404 if not found</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var ev = await _db.Events
            .Include(e => e.Categories)
            .Include(e => e.Organiser).ThenInclude(o => o.User)
            .FirstOrDefaultAsync(e => e.EventID == id && e.IsActive);

        if (ev == null) return NotFound(new { message = "Event not found" });

        return Ok(new
        {
            ev.EventID,
            ev.Name,
            ev.Description,
            ev.EventDate,
            ev.Location,
            ev.Distance,
            ev.EventType,
            ev.BannerImage,
            Organiser = new
            {
                ev.Organiser.OrganiserID,
                Name = ev.Organiser.User.FullName,
                ev.Organiser.CompanyName
            },
            Categories = ev.Categories.Select(c => new
            {
                c.CategoryID,
                c.Name,
                c.Description,
                c.MinAge,
                c.MaxAge,
                c.Distance,
                c.EntryFee
            })
        });
    }

    /// <summary>
    /// Creates a new event. Organiser only.
    /// </summary>
    /// <param name="dto">Event details</param>
    /// <returns>201 Created with event; 400 on validation; 403 if not organiser</returns>
    [HttpPost]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> Create([FromBody] CreateEventDto dto)
    {
        if (dto.EventType != "Run" && dto.EventType != "Walk" && dto.EventType != "Cycle")
            return BadRequest(new { message = "EventType must be Run, Walk, or Cycle" });

        var userId = HttpContext.Session.GetUserId()!.Value;
        var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserID == userId);
        if (organiser == null) return StatusCode(403, new { message = "Forbidden" });

        var ev = new Event
        {
            OrganiserID = organiser.OrganiserID,
            Name = dto.Name,
            Description = dto.Description,
            EventDate = dto.EventDate,
            Location = dto.Location,
            Distance = dto.Distance,
            EventType = dto.EventType,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _db.Events.Add(ev);
        await _db.SaveChangesAsync();

        return StatusCode(201, new { ev.EventID, ev.Name });
    }

    /// <summary>
    /// Updates an existing event. Organiser must own the event.
    /// </summary>
    /// <param name="id">Event ID</param>
    /// <param name="dto">Updated fields</param>
    /// <returns>200 OK; 403 if not owner; 404 if not found</returns>
    [HttpPut("{id}")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEventDto dto)
    {
        var userId = HttpContext.Session.GetUserId()!.Value;
        var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserID == userId);
        if (organiser == null) return StatusCode(403, new { message = "Forbidden" });

        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound(new { message = "Event not found" });
        if (ev.OrganiserID != organiser.OrganiserID) return StatusCode(403, new { message = "Forbidden" });

        ev.Name = dto.Name;
        ev.Description = dto.Description;
        ev.EventDate = dto.EventDate;
        ev.Location = dto.Location;
        ev.Distance = dto.Distance;
        ev.EventType = dto.EventType;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Event updated" });
    }

    /// <summary>
    /// Soft-deletes an event. Organiser must own the event.
    /// </summary>
    /// <param name="id">Event ID</param>
    /// <returns>204 No Content; 403 if not owner; 404 if not found</returns>
    [HttpDelete("{id}")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = HttpContext.Session.GetUserId()!.Value;
        var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserID == userId);
        if (organiser == null) return StatusCode(403, new { message = "Forbidden" });

        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound(new { message = "Event not found" });
        if (ev.OrganiserID != organiser.OrganiserID) return StatusCode(403, new { message = "Forbidden" });

        ev.IsActive = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}