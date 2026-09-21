using Daily_Tracker.Domain.Configurations;
using Daily_Tracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Daily_Tracker.Infrastructure.Persistence;

public class DailyTrackerDbContext : DbContext
{
    public DailyTrackerDbContext(DbContextOptions<DailyTrackerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<WorkoutEntry> WorkoutEntries => Set<WorkoutEntry>();
    public DbSet<WorkoutExercise> WorkoutExercises => Set<WorkoutExercise>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new TenantConfiguration());
        modelBuilder.ApplyConfiguration(new ApplicationUserConfiguration());
        modelBuilder.ApplyConfiguration(new WorkoutEntryConfiguration());
        modelBuilder.ApplyConfiguration(new WorkoutExerciseConfiguration());
    }
}
