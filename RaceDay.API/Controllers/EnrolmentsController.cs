using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Helpers;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api")]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EnrolmentsController(RaceDayDbContext db) => _db = db;

    /// <summary>
    /// Enrols the logged-in Participant into an event under a selected category.
    /// </summary>
    /// <param name="eventId">Event ID</param>
    /// <param name="dto">Category to enter</param>
    /// <returns>201 Created; 400 if already enrolled; 403 if not participant; 404 if event not found</returns>
    [HttpPost("events/{eventId}/enrol")]
    [SessionAuthorize("Participant")]
    public async Task<IActionResult> Enrol(int eventId, [FromBody] EnrolDto dto)
    {
        var userId = HttpContext.Session.GetUserId()!.Value;
        var participant = await _db.Participants.FirstOrDefaultAsync(p => p.UserID == userId);
        if (participant == null) return StatusCode(403, new { message = "Forbidden" });

        var ev = await _db.Events.FindAsync(eventId);
        if (ev == null) return NotFound(new { message = "Event not found" });

        var cat = await _db.Categories.FirstOrDefaultAsync(c => c.CategoryID == dto.CategoryId && c.EventID == eventId);
        if (cat == null) return BadRequest(new { message = "Category does not belong to this event" });

        var exists = await _db.Enrolments.AnyAsync(e => e.ParticipantID == participant.ParticipantID && e.EventID == eventId);
        if (exists) return BadRequest(new { message = "Already enrolled in this event" });

        var enrolment = new Enrolment
        {
            ParticipantID = participant.ParticipantID,
            EventID = eventId,
            CategoryID = dto.CategoryId,
            EnrolmentDate = DateTime.UtcNow,
            Status = "Pending",
            PaymentStatus = "Unpaid"
        };

        _db.Enrolments.Add(enrolment);
        await _db.SaveChangesAsync();

        return StatusCode(201, new { enrolment.EnrolmentID, enrolment.Status });
    }

    /// <summary>
    /// Lists all enrolments for an event. Organiser must own the event.
    /// </summary>
    /// <param name="eventId">Event ID</param>
    /// <returns>200 OK with enrolments; 403 if not owner; 404 if event not found</returns>
    [HttpGet("events/{eventId}/enrolments")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> GetForEvent(int eventId)
    {
        var userId = HttpContext.Session.GetUserId()!.Value;
        var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserID == userId);
        if (organiser == null) return StatusCode(403, new { message = "Forbidden" });

        var ev = await _db.Events.FindAsync(eventId);
        if (ev == null) return NotFound(new { message = "Event not found" });
        if (ev.OrganiserID != organiser.OrganiserID) return StatusCode(403, new { message = "Forbidden" });

        var enrolments = await _db.Enrolments
            .Include(e => e.Participant).ThenInclude(p => p.User)
            .Include(e => e.Category)
            .Where(e => e.EventID == eventId)
            .Select(e => new
            {
                e.EnrolmentID,
                e.EnrolmentDate,
                e.Status,
                e.PaymentStatus,
                Participant = new
                {
                    e.Participant.ParticipantID,
                    Name = e.Participant.User.FullName,
                    Email = e.Participant.User.Email
                },
                Category = e.Category.Name
            })
            .ToListAsync();

        return Ok(enrolments);
    }

    /// <summary>
    /// Lists the logged-in Participant's own enrolments.
    /// </summary>
    /// <returns>200 OK with enrolments; 403 if not participant</returns>
    [HttpGet("enrolments/my")]
    [SessionAuthorize("Participant")]
    public async Task<IActionResult> GetMy()
    {
        var userId = HttpContext.Session.GetUserId()!.Value;
        var participant = await _db.Participants.FirstOrDefaultAsync(p => p.UserID == userId);
        if (participant == null) return StatusCode(403, new { message = "Forbidden" });

        var enrolments = await _db.Enrolments
            .Include(e => e.Event)
            .Include(e => e.Category)
            .Where(e => e.ParticipantID == participant.ParticipantID)
            .Select(e => new
            {
                e.EnrolmentID,
                e.Status,
                e.PaymentStatus,
                e.EnrolmentDate,
                Event = new { e.Event.EventID, e.Event.Name, e.Event.EventDate, e.Event.Location },
                Category = e.Category.Name
            })
            .ToListAsync();

        return Ok(enrolments);
    }

    /// <summary>
    /// Withdraws (deletes) the participant's own enrolment.
    /// </summary>
    /// <param name="id">Enrolment ID</param>
    /// <returns>200 OK; 403 if not owner; 404 if not found</returns>
    [HttpDelete("enrolments/{id}")]
    [SessionAuthorize("Participant")]
    public async Task<IActionResult> Withdraw(int id)
    {
        var userId = HttpContext.Session.GetUserId()!.Value;
        var participant = await _db.Participants.FirstOrDefaultAsync(p => p.UserID == userId);
        if (participant == null) return StatusCode(403, new { message = "Forbidden" });

        var enrolment = await _db.Enrolments.FindAsync(id);
        if (enrolment == null) return NotFound(new { message = "Enrolment not found" });
        if (enrolment.ParticipantID != participant.ParticipantID) return StatusCode(403, new { message = "Forbidden" });

        _db.Enrolments.Remove(enrolment);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Withdrawn successfully" });
    }

    /// <summary>
    /// Updates the status of an enrolment. Organiser must own the event.
    /// </summary>
    /// <param name="id">Enrolment ID</param>
    /// <param name="dto">New status</param>
    /// <returns>200 OK; 400 invalid status; 403 not owner; 404 not found</returns>
    [HttpPut("enrolments/{id}/status")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateEnrolmentStatusDto dto)
    {
        var valid = new[] { "Pending", "Confirmed", "Completed", "Cancelled" };
        if (!valid.Contains(dto.Status))
            return BadRequest(new { message = "Invalid status" });

        var userId = HttpContext.Session.GetUserId()!.Value;
        var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserID == userId);
        if (organiser == null) return StatusCode(403, new { message = "Forbidden" });

        var enrolment = await _db.Enrolments
            .Include(e => e.Event)
            .FirstOrDefaultAsync(e => e.EnrolmentID == id);

        if (enrolment == null) return NotFound(new { message = "Enrolment not found" });
        if (enrolment.Event.OrganiserID != organiser.OrganiserID) return StatusCode(403, new { message = "Forbidden" });

        enrolment.Status = dto.Status;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Status updated" });
    }
}