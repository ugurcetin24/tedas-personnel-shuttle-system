using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tedas.Shuttle.Domain.Entities;

namespace Tedas.Shuttle.Infrastructure.Persistence.Configurations;

public sealed class SavedRouteConfiguration : IEntityTypeConfiguration<SavedRoute>
{
    public void Configure(EntityTypeBuilder<SavedRoute> builder)
    {
        builder.ToTable("SavedRoutes");

        builder.HasKey(route => route.Id);

        builder.Property(route => route.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(route => route.Geometry)
            .IsRequired();

        builder.Property(route => route.CreatedAt)
            .HasConversion(
                value => value.ToUnixTimeMilliseconds(),
                value => DateTimeOffset.FromUnixTimeMilliseconds(value))
            .IsRequired();

        builder.Property(route => route.UpdatedAt)
            .HasConversion(
                value => value.HasValue ? value.Value.ToUnixTimeMilliseconds() : (long?)null,
                value => value.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(value.Value) : null);

        builder.HasOne(route => route.ShuttleShift)
            .WithMany()
            .HasForeignKey(route => route.ShuttleShiftId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(route => route.ShuttleShiftId);
    }
}
