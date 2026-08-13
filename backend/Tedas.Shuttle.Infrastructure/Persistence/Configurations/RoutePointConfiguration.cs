using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tedas.Shuttle.Domain.Entities;

namespace Tedas.Shuttle.Infrastructure.Persistence.Configurations;

public sealed class RoutePointConfiguration : IEntityTypeConfiguration<RoutePoint>
{
    public void Configure(EntityTypeBuilder<RoutePoint> builder)
    {
        builder.ToTable("RoutePoints");

        builder.HasKey(routePoint => routePoint.Id);

        builder.Property(routePoint => routePoint.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(routePoint => routePoint.Address)
            .HasMaxLength(500);

        builder.Property(routePoint => routePoint.Latitude)
            .HasColumnType("decimal(9,6)");

        builder.Property(routePoint => routePoint.Longitude)
            .HasColumnType("decimal(9,6)");

        builder.HasOne(routePoint => routePoint.ShuttleShift)
            .WithMany()
            .HasForeignKey(routePoint => routePoint.ShuttleShiftId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(routePoint => new { routePoint.ShuttleShiftId, routePoint.Order })
            .IsUnique();

        builder.HasIndex(routePoint => routePoint.IsActive);
    }
}

