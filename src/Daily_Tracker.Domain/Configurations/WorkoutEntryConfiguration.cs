using Daily_Tracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Daily_Tracker.Domain.Configurations;

public class WorkoutEntryConfiguration : IEntityTypeConfiguration<WorkoutEntry>
{
    public void Configure(EntityTypeBuilder<WorkoutEntry> builder)
    {
        builder.ToTable("workout_entries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        builder.Property(x => x.CreatedUtc)
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.UpdatedUtc)
            .HasColumnType("timestamp with time zone");

        builder.HasOne(x => x.Tenant)
            .WithMany(x => x.WorkoutEntries)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
