namespace RaceDay.Web.Models;

public class EventDto
{
    public int EventID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal Distance { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? BannerImage { get; set; }
    public string Organiser { get; set; } = string.Empty;
}

public class CategoryDto
{
    public int CategoryID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public decimal? Distance { get; set; }
    public decimal? EntryFee { get; set; }
}

public class EventDetailsDto
{
    public int EventID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal Distance { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? BannerImage { get; set; }
    public List<CategoryDto> Categories { get; set; } = new();
}

public class EnrolDto
{
    public int CategoryId { get; set; }
}

public class ParticipantEventDto
{
    public int EventID { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
}

public class MyEnrolmentDto
{
    public int EnrolmentID { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public ParticipantEventDto Event { get; set; } = new();
    public string Category { get; set; } = string.Empty;
}

public class CreateEventDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EventDate { get; set; } = DateTime.Now.AddDays(30);
    public string Location { get; set; } = string.Empty;
    public decimal Distance { get; set; }
    public string EventType { get; set; } = "Run";
    public IFormFile? BannerImage { get; set; }
}

public class OrganiserEnrolmentDto
{
    public int EnrolmentID { get; set; }
    public DateTime EnrolmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public OrganiserParticipantDto Participant { get; set; } = new();
    public string Category { get; set; } = string.Empty;
}

public class OrganiserParticipantDto
{
    public int ParticipantID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class UpdateStatusDto
{
    public string Status { get; set; } = string.Empty;
}

public class CreateResultDto
{
    public string? FinishTime { get; set; }
    public int? Position { get; set; }
    public bool IsCompleted { get; set; }
    public string? Notes { get; set; }
}

public class ParticipantResultDto
{
    public int ResultID { get; set; }
    public string Event { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? FinishTime { get; set; }
    public int? Position { get; set; }
    public bool IsCompleted { get; set; }
    public string? Notes { get; set; }
}
public class ProfileDto
{
    public int UserID { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    // Participant specific
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? EmergencyContact { get; set; }

    // Organiser specific
    public string? CompanyName { get; set; }
    public string? OrganisationPhoneNumber { get; set; }
}

public class UpdateProfileDto
{
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? EmergencyContact { get; set; }
    public string? CompanyName { get; set; }
    public string? OrganisationPhoneNumber { get; set; }
}

