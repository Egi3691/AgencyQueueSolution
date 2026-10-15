using Microsoft.EntityFrameworkCore;

namespace AgencyQueue.Api;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<ClosedDay> ClosedDays => Set<ClosedDay>();
    public DbSet<AgencySettings> Settings => Set<AgencySettings>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Appointment>().HasIndex(x => new { x.AppointmentDate, x.QueueNumber }).IsUnique();
        model.Entity<Appointment>().HasIndex(x => x.Token).IsUnique();
        model.Entity<ClosedDay>().HasIndex(x => x.Date).IsUnique();
        model.Entity<AgencySettings>().HasData(new AgencySettings { Id = 1, DailyCapacity = 20 });
    }
}
