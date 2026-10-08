using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(RaceDayDbContext db, IPasswordHasher hasher)
    {
        // Only seed if empty
        if (await db.Users.AnyAsync()) return;

        // Default Organiser
        var organiserUser = new User
        {
            Email = "organiser@raceday.co.za",
            PasswordHash = hasher.Hash("Pass123!"),
            FullName = "Bongani Zulu",
            Role = "Organiser",
            PhoneNumber = "0829876543",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        db.Users.Add(organiserUser);
        await db.SaveChangesAsync();

        db.Organisers.Add(new Organiser
        {
            UserID = organiserUser.UserID,
            CompanyName = "Durban Running Club",
            OrganisationPhoneNumber = "0317654321",
            CreatedAt = DateTime.UtcNow
        });

        // Default Participant
        var participantUser = new User
        {
            Email = "participant@raceday.co.za",
            PasswordHash = hasher.Hash("Pass123!"),
            FullName = "Sandile Mkhize",
            Role = "Participant",
            PhoneNumber = "0713456789",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        db.Users.Add(participantUser);
        await db.SaveChangesAsync();

        db.Participants.Add(new Participant
        {
            UserID = participantUser.UserID,
            DateOfBirth = new DateTime(1992, 7, 20),
            Gender = "Male",
            PhoneNumber = "0713456789",
            EmergencyContact = "Lindiwe Mkhize - 0713456790",
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
    }
}