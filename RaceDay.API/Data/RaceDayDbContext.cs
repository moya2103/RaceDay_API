using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Organiser> Organisers => Set<Organiser>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email).IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.Role).HasMaxLength(20).IsRequired();

        modelBuilder.Entity<User>()
            .Property(u => u.Email).HasMaxLength(255).IsRequired();

        // Participant 1-1 User
        modelBuilder.Entity<Participant>()
            .HasOne(p => p.User)
            .WithOne(u => u.Participant)
            .HasForeignKey<Participant>(p => p.UserID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Participant>()
            .HasIndex(p => p.UserID).IsUnique();

        // Organiser 1-1 User
        modelBuilder.Entity<Organiser>()
            .HasOne(o => o.User)
            .WithOne(u => u.Organiser)
            .HasForeignKey<Organiser>(o => o.UserID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Organiser>()
            .HasIndex(o => o.UserID).IsUnique();

        // Event -> Organiser
        modelBuilder.Entity<Event>()
            .HasOne(e => e.Organiser)
            .WithMany(o => o.Events)
            .HasForeignKey(e => e.OrganiserID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Event>()
            .Property(e => e.Distance).HasPrecision(10, 2);

        modelBuilder.Entity<Event>()
            .Property(e => e.EventType).HasMaxLength(20).IsRequired();

        // Category -> Event
        modelBuilder.Entity<Category>()
            .HasOne(c => c.Event)
            .WithMany(e => e.Categories)
            .HasForeignKey(c => c.EventID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Category>()
            .Property(c => c.Distance).HasPrecision(10, 2);

        modelBuilder.Entity<Category>()
            .Property(c => c.EntryFee).HasPrecision(10, 2);

        // Enrolment -> Participant
        modelBuilder.Entity<Enrolment>()
            .HasOne(e => e.Participant)
            .WithMany(p => p.Enrolments)
            .HasForeignKey(e => e.ParticipantID)
            .OnDelete(DeleteBehavior.Cascade);

        // Enrolment -> Event
        modelBuilder.Entity<Enrolment>()
            .HasOne(e => e.Event)
            .WithMany(ev => ev.Enrolments)
            .HasForeignKey(e => e.EventID)
            .OnDelete(DeleteBehavior.Restrict);

        // Enrolment -> Category
        modelBuilder.Entity<Enrolment>()
            .HasOne(e => e.Category)
            .WithMany(c => c.Enrolments)
            .HasForeignKey(e => e.CategoryID)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique: one enrolment per participant per event
        modelBuilder.Entity<Enrolment>()
            .HasIndex(e => new { e.ParticipantID, e.EventID }).IsUnique();

        modelBuilder.Entity<Enrolment>()
            .Property(e => e.Status).HasMaxLength(20).IsRequired();

        modelBuilder.Entity<Enrolment>()
            .Property(e => e.PaymentStatus).HasMaxLength(20).IsRequired();

        // Result 1-1 Enrolment
        modelBuilder.Entity<Result>()
            .HasOne(r => r.Enrolment)
            .WithOne(e => e.Result)
            .HasForeignKey<Result>(r => r.EnrolmentID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Result>()
            .HasIndex(r => r.EnrolmentID).IsUnique();
    }
}
