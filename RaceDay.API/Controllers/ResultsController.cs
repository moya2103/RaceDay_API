using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Helpers;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api")]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public ResultsController(RaceDayDbContext db) => _db = db;

    /// <summary>
    /// Records a result for an enrolment. Organiser must own the event.
    /// Also marks the enrolment as Completed if IsCompleted is true.
    /// </summary>
    /// <param name="id">Enrolment ID</param>
    /// <param name="dto">Finish time (hh:mm:ss), position, completion flag, notes</param>
    /// <returns>201 Created; 400 invalid time; 403 not owner; 404 not found</returns>
    [HttpPost("enrolments/{id}/result")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> Create(int id, [FromBody] CreateResultDto dto)
    {
        var userId = HttpContext.Session.GetUserId()!.Value;
        var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserID == userId);
        if (organiser == null) return StatusCode(403, new { message = "Forbidden" });

        var enrolment = await _db.Enrolments
            .Include(e => e.Event)
            .FirstOrDefaultAsync(e => e.EnrolmentID == id);

        if (enrolment == null) return NotFound(new { message = "Enrolment not found" });
        if (enrolment.Event.OrganiserID != organiser.OrganiserID) return StatusCode(403, new { message = "Forbidden" });

        TimeSpan? finish = null;
        if (!string.IsNullOrWhiteSpace(dto.FinishTime))
        {
            if (!TimeSpan.TryParse(dto.FinishTime, out var parsed))
                return BadRequest(new { message = "FinishTime must be in hh:mm:ss format" });
            finish = parsed;
        }

        if (await _db.Results.AnyAsync(r => r.EnrolmentID == id))
            return BadRequest(new { message = "Result already recorded for this enrolment" });

        var result = new Result
        {
            EnrolmentID = id,
            FinishTime = finish,
            Position = dto.Position,
            IsCompleted = dto.IsCompleted,
            Notes = dto.Notes,
            RecordedAt = DateTime.UtcNow
        };

        _db.Results.Add(result);

        if (dto.IsCompleted)
            enrolment.Status = "Completed";

        await _db.SaveChangesAsync();
        return StatusCode(201, new { result.ResultID });
    }

    /// <summary>
    /// Gets the result for a specific enrolment.
    /// </summary>
    /// <param name="id">Enrolment ID</param>
    /// <returns>200 OK; 403 if not owner or participant; 404 if not found</returns>
    [HttpGet("enrolments/{id}/result")]
    [SessionAuthorize("Organiser", "Participant")]
    public async Task<IActionResult> GetForEnrolment(int id)
    {
        var userId = HttpContext.Session.GetUserId()!.Value;
        var role = HttpContext.Session.GetRole();

        var enrolment = await _db.Enrolments
            .Include(e => e.Event)
            .Include(e => e.Participant)
            .Include(e => e.Result)
            .FirstOrDefaultAsync(e => e.EnrolmentID == id);

        if (enrolment == null) return NotFound(new { message = "Enrolment not found" });

        if (role == "Organiser")
        {
            var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserID == userId);
            if (organiser == null || enrolment.Event.OrganiserID != organiser.OrganiserID) return StatusCode(403, new { message = "Forbidden" });
        }
        else if (role == "Participant")
        {
            var participant = await _db.Participants.FirstOrDefaultAsync(p => p.UserID == userId);
            if (participant == null || enrolment.ParticipantID != participant.ParticipantID) return StatusCode(403, new { message = "Forbidden" });
        }

        if (enrolment.Result == null) return NotFound(new { message = "No result yet" });

        return Ok(new
        {
            enrolment.Result.ResultID,
            enrolment.Result.FinishTime,
            enrolment.Result.Position,
            enrolment.Result.IsCompleted,
            enrolment.Result.Notes,
            enrolment.Result.RecordedAt,
            Event = enrolment.Event.Name
        });
    }

    /// <summary>
    /// Gets the logged-in Participant's personal race history.
    /// </summary>
    /// <returns>200 OK with list of results; 403 if not participant</returns>
    [HttpGet("results/my")]
    [SessionAuthorize("Participant")]
    public async Task<IActionResult> GetMy()
    {
        var userId = HttpContext.Session.GetUserId()!.Value;
        var participant = await _db.Participants.FirstOrDefaultAsync(p => p.UserID == userId);
        if (participant == null) return StatusCode(403, new { message = "Forbidden" });

        var results = await _db.Results
            .Include(r => r.Enrolment).ThenInclude(e => e.Event)
            .Include(r => r.Enrolment).ThenInclude(e => e.Category)
            .Where(r => r.Enrolment.ParticipantID == participant.ParticipantID)
            .OrderByDescending(r => r.Enrolment.Event.EventDate)
            .Select(r => new
            {
                r.ResultID,
                Event = r.Enrolment.Event.Name,
                Date = r.Enrolment.Event.EventDate,
                Category = r.Enrolment.Category.Name,
                r.FinishTime,
                r.Position,
                r.IsCompleted,
                r.Notes
            })
            .ToListAsync();

        return Ok(results);
    }
}