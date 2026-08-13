using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tedas.Shuttle.Domain.Entities;

namespace Tedas.Shuttle.Infrastructure.Persistence.Configurations;

public sealed class PhysicalShuttleConfiguration : IEntityTypeConfiguration<PhysicalShuttle>
{
    public void Configure(EntityTypeBuilder<PhysicalShuttle> builder)
    {
        builder.ToTable("PhysicalShuttles");

        builder.HasKey(shuttle => shuttle.Id);

        builder.Property(shuttle => shuttle.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(shuttle => shuttle.PlateNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(shuttle => shuttle.Description)
            .HasMaxLength(500);

        builder.HasIndex(shuttle => shuttle.Code)
            .IsUnique();

        builder.HasIndex(shuttle => shuttle.PlateNumber);
        builder.HasIndex(shuttle => shuttle.IsActive);
    }
}
