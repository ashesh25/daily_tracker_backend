using Daily_Tracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Daily_Tracker.Domain.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.Slug)
            .IsUnique();

        builder.Property(x => x.CreatedUtc)
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.UpdatedUtc)
            .HasColumnType("timestamp with time zone");
    }
}
