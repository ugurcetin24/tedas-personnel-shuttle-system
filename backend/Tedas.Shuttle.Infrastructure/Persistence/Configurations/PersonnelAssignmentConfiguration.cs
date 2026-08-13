using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tedas.Shuttle.Domain.Entities;

namespace Tedas.Shuttle.Infrastructure.Persistence.Configurations;

public sealed class PersonnelAssignmentConfiguration : IEntityTypeConfiguration<PersonnelAssignment>
{
    public void Configure(EntityTypeBuilder<PersonnelAssignment> builder)
    {
        builder.ToTable("PersonnelAssignments");

        builder.HasKey(assignment => assignment.Id);

        builder.HasOne(assignment => assignment.Personnel)
            .WithMany()
            .HasForeignKey(assignment => assignment.PersonnelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(assignment => assignment.ShuttleShift)
            .WithMany()
            .HasForeignKey(assignment => assignment.ShuttleShiftId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(assignment => assignment.AssignedAt)
            .HasConversion(
                value => value.ToUnixTimeMilliseconds(),
                value => DateTimeOffset.FromUnixTimeMilliseconds(value))
            .IsRequired();

        builder.Property(assignment => assignment.DeactivatedAt)
            .HasConversion(
                value => value.HasValue ? value.Value.ToUnixTimeMilliseconds() : (long?)null,
                value => value.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(value.Value) : null);

        builder.HasIndex(assignment => assignment.PersonnelId)
            .IsUnique()
            .HasFilter("\"IsActive\" = 1");

        builder.HasIndex(assignment => assignment.ShuttleShiftId);
        builder.HasIndex(assignment => assignment.IsActive);
    }
}
