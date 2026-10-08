namespace RaceDay.API.Models;

public class Participant
{
    public int ParticipantID { get; set; }
    public int UserID { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = "Other";
    public string? PhoneNumber { get; set; }
    public string? EmergencyContact { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}