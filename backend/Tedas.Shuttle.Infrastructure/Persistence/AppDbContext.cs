using Microsoft.EntityFrameworkCore;
using Tedas.Shuttle.Domain.Entities;

namespace Tedas.Shuttle.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Personnel> Personnel => Set<Personnel>();

    public DbSet<PhysicalShuttle> PhysicalShuttles => Set<PhysicalShuttle>();

    public DbSet<ShuttleShift> ShuttleShifts => Set<ShuttleShift>();

    public DbSet<Driver> Drivers => Set<Driver>();

    public DbSet<PersonnelAssignment> PersonnelAssignments => Set<PersonnelAssignment>();

    public DbSet<RoutePoint> RoutePoints => Set<RoutePoint>();

    public DbSet<SavedRoute> SavedRoutes => Set<SavedRoute>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(null);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
