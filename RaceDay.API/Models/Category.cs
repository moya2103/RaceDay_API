namespace RaceDay.API.Models;

public class Category
{
    public int CategoryID { get; set; }
    public int EventID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public decimal? Distance { get; set; }
    public decimal? EntryFee { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Event Event { get; set; } = null!;
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}