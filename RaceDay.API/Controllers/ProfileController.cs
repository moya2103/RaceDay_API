using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Helpers;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/profile")]
[SessionAuthorize]
public class ProfileController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly IPasswordHasher _hasher;

    public ProfileController(RaceDayDbContext db, IPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    /// <summary>
    /// Gets the current user's profile including role-specific details.
    /// </summary>
    /// <returns>200 OK with profile; 401 if not logged in</returns>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = HttpContext.Session.GetUserId()!.Value;
        var user = await _db.Users
            .Include(u => u.Participant)
            .Include(u => u.Organiser)
            .FirstOrDefaultAsync(u => u.UserID == userId);

        if (user == null) return NotFound();

        return Ok(new
        {
            user.UserID,
            user.Email,
            user.FullName,
            user.Role,
            user.PhoneNumber,
            Participant = user.Participant == null ? null : new
            {
                user.Participant.DateOfBirth,
                user.Participant.Gender,
                user.Participant.EmergencyContact
            },
            Organiser = user.Organiser == null ? null : new
            {
                user.Organiser.CompanyName,
                user.Organiser.OrganisationPhoneNumber
            }
        });
    }

    /// <summary>
    /// Updates the current user's profile. Field usage depends on role.
    /// </summary>
    /// <param name="dto">Fields to update</param>
    /// <returns>200 OK with updated profile; 400 on validation failure</returns>
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateProfileDto dto)
    {
        var userId = HttpContext.Session.GetUserId()!.Value;
        var user = await _db.Users
            .Include(u => u.Participant)
            .Include(u => u.Organiser)
            .FirstOrDefaultAsync(u => u.UserID == userId);

        if (user == null) return NotFound();

        user.FullName = dto.FullName;
        user.PhoneNumber = dto.PhoneNumber;

        if (user.Role == "Participant" && user.Participant != null)
        {
            if (dto.DateOfBirth.HasValue) user.Participant.DateOfBirth = dto.DateOfBirth.Value;
            if (!string.IsNullOrWhiteSpace(dto.Gender)) user.Participant.Gender = dto.Gender;
            user.Participant.EmergencyContact = dto.EmergencyContact;
        }
        else if (user.Role == "Organiser" && user.Organiser != null)
        {
            user.Organiser.CompanyName = dto.CompanyName;
            user.Organiser.OrganisationPhoneNumber = dto.OrganisationPhoneNumber;
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = "Profile updated" });
    }

    /// <summary>
    /// Changes the current user's password after verifying the current one.
    /// </summary>
    /// <param name="dto">Current and new password</param>
    /// <returns>200 OK; 400 if current password is incorrect</returns>
    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = HttpContext.Session.GetUserId()!.Value;
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        if (!_hasher.Verify(dto.CurrentPassword, user.PasswordHash))
            return BadRequest(new { message = "Current password is incorrect" });

        user.PasswordHash = _hasher.Hash(dto.NewPassword);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Password updated" });
    }
}