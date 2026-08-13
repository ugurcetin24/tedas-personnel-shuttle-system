using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tedas.Shuttle.Domain.Entities;

namespace Tedas.Shuttle.Infrastructure.Persistence.Configurations;

public sealed class ShuttleShiftConfiguration : IEntityTypeConfiguration<ShuttleShift>
{
    public void Configure(EntityTypeBuilder<ShuttleShift> builder)
    {
        builder.ToTable("ShuttleShifts");

        builder.HasKey(shift => shift.Id);

        builder.Property(shift => shift.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(shift => shift.ShiftType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(shift => shift.Capacity)
            .IsRequired();

        builder.Property(shift => shift.StartTime)
            .HasConversion<string>()
            .HasMaxLength(8)
            .IsRequired();

        builder.Property(shift => shift.EndTime)
            .HasConversion<string>()
            .HasMaxLength(8)
            .IsRequired();

        builder.HasOne(shift => shift.PhysicalShuttle)
            .WithMany(shuttle => shuttle.Shifts)
            .HasForeignKey(shift => shift.PhysicalShuttleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(shift => shift.PhysicalShuttleId);
        builder.HasIndex(shift => shift.IsActive);
    }
}
