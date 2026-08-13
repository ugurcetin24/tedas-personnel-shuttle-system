using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tedas.Shuttle.Domain.Entities;

namespace Tedas.Shuttle.Infrastructure.Persistence.Configurations;

public sealed class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        builder.ToTable("Drivers");

        builder.HasKey(driver => driver.Id);

        builder.Property(driver => driver.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(driver => driver.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(driver => driver.Phone)
            .HasMaxLength(30);

        builder.Property(driver => driver.LicenseNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasOne(driver => driver.ShuttleShift)
            .WithOne()
            .HasForeignKey<Driver>(driver => driver.ShuttleShiftId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(driver => driver.LicenseNumber)
            .IsUnique();

        builder.HasIndex(driver => driver.ShuttleShiftId)
            .IsUnique()
            .HasFilter("\"ShuttleShiftId\" IS NOT NULL");

        builder.HasIndex(driver => driver.IsActive);
    }
}

