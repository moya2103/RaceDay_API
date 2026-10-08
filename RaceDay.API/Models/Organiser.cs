namespace RaceDay.API.Models;

public class Organiser
{
    public int OrganiserID { get; set; }
    public int UserID { get; set; }
    public string? CompanyName { get; set; }
    public string? OrganisationPhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public ICollection<Event> Events { get; set; } = new List<Event>();
}