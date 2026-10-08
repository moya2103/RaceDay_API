using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Helpers;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly IPasswordHasher _hasher;

    public AuthController(RaceDayDbContext db, IPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    /// <summary>
    /// Registers a new user account as either an Organiser or a Participant.
    /// </summary>
    /// <param name="dto">Registration details: email, password, fullName, role, phoneNumber</param>
    /// <returns>201 Created with basic user info; 400 if email exists or role is invalid</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (dto.Role != "Organiser" && dto.Role != "Participant")
            return BadRequest(new { message = "Role must be Organiser or Participant" });

        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { message = "Email and password are required" });

        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest(new { message = "Email already registered" });

        var user = new User
        {
            Email = dto.Email,
            PasswordHash = _hasher.Hash(dto.Password),
            FullName = dto.FullName,
            Role = dto.Role,
            PhoneNumber = dto.PhoneNumber,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        if (dto.Role == "Participant")
        {
            _db.Participants.Add(new Participant
            {
                UserID = user.UserID,
                DateOfBirth = DateTime.UtcNow.AddYears(-20),
                Gender = "Other",
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            _db.Organisers.Add(new Organiser
            {
                UserID = user.UserID,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();

        return StatusCode(201, new { user.UserID, user.Email, user.FullName, user.Role });
    }

    /// <summary>
    /// Authenticates a user and creates a server-side session.
    /// </summary>
    /// <param name="dto">Login credentials: email and password</param>
    /// <returns>200 OK with user info; 401 if credentials are invalid</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email && u.IsActive);
        if (user == null || !_hasher.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid credentials" });

        HttpContext.Session.SetUser(user.UserID, user.Role, user.Email);
        return Ok(new { user.UserID, user.Email, user.FullName, user.Role });
    }

    /// <summary>
    /// Ends the current user session.
    /// </summary>
    /// <returns>200 OK</returns>
    [HttpPost("logout")]
    [SessionAuthorize]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// Returns the currently logged-in user's basic info.
    /// </summary>
    /// <returns>200 OK with user info; 401 if no active session</returns>
    [HttpGet("me")]
    [SessionAuthorize]
    public async Task<IActionResult> Me()
    {
        var userId = HttpContext.Session.GetUserId()!.Value;
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return Unauthorized(new { message = "User not found" });

        return Ok(new { user.UserID, user.Email, user.FullName, user.Role });
    }
}
