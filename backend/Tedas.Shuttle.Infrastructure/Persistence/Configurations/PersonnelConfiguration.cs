using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tedas.Shuttle.Domain.Entities;

namespace Tedas.Shuttle.Infrastructure.Persistence.Configurations;

public sealed class PersonnelConfiguration : IEntityTypeConfiguration<Personnel>
{
    public void Configure(EntityTypeBuilder<Personnel> builder)
    {
        builder.ToTable("Personnel");

        builder.HasKey(personnel => personnel.Id);

        builder.Property(personnel => personnel.RegistrationNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(personnel => personnel.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(personnel => personnel.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(personnel => personnel.Department)
            .HasMaxLength(150);

        builder.Property(personnel => personnel.Title)
            .HasMaxLength(150);

        builder.Property(personnel => personnel.Phone)
            .HasMaxLength(30);

        builder.Property(personnel => personnel.Email)
            .HasMaxLength(200);

        builder.Property(personnel => personnel.Address)
            .HasMaxLength(500);

        builder.Property(personnel => personnel.Latitude)
            .HasColumnType("decimal(9,6)");

        builder.Property(personnel => personnel.Longitude)
            .HasColumnType("decimal(9,6)");

        builder.HasIndex(personnel => personnel.RegistrationNumber)
            .IsUnique();

        builder.HasIndex(personnel => personnel.Department);
        builder.HasIndex(personnel => personnel.IsActive);
    }
}
