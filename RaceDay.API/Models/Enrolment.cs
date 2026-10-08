namespace RaceDay.API.Models;

public class Enrolment
{
    public int EnrolmentID { get; set; }
    public int ParticipantID { get; set; }
    public int EventID { get; set; }
    public int CategoryID { get; set; }
    public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending";
    public string PaymentStatus { get; set; } = "Unpaid";

    public Participant Participant { get; set; } = null!;
    public Event Event { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public Result? Result { get; set; }
}