namespace RaceDay.API.Models;

public class Result
{
    public int ResultID { get; set; }
    public int EnrolmentID { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public int? Position { get; set; }
    public bool IsCompleted { get; set; } = false;
    public string? Notes { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    public Enrolment Enrolment { get; set; } = null!;
}