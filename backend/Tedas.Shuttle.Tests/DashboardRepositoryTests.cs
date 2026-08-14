using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tedas.Shuttle.Domain.Entities;
using Tedas.Shuttle.Domain.Enums;
using Tedas.Shuttle.Infrastructure.Persistence;
using Tedas.Shuttle.Infrastructure.Repositories;

namespace Tedas.Shuttle.Tests;

public sealed class DashboardRepositoryTests
{
    [Fact]
    public async Task GetSummaryAsync_WithSeedData_ReturnsMetricsAndOccupancy()
    {
        await using var fixture = await DashboardFixture.CreateAsync();
        var shuttle = new PhysicalShuttle("SERVIS-01", "06 TEST 01", "Test servis", DateTimeOffset.UtcNow);
        var shift = new ShuttleShift(
            shuttle.Id,
            "Sabah",
            ShiftType.Morning,
            20,
            new TimeOnly(7, 30),
            new TimeOnly(9, 0),
            DateTimeOffset.UtcNow);
        var assigned = new Personnel(
            "TEST-1001",
            "Ayse",
            "Demir",
            "BT",
            null,
            null,
            null,
            null,
            null,
            null,
            DateTimeOffset.UtcNow);
        var unassigned = new Personnel(
            "TEST-1002",
            "Mehmet",
            "Yilmaz",
            "IK",
            null,
            null,
            null,
            null,
            null,
            null,
            DateTimeOffset.UtcNow);
        var routePoint = new RoutePoint(
            shift.Id,
            1,
            "TEDAS",
            null,
            39.925m,
            32.854m,
            DateTimeOffset.UtcNow);

        fixture.DbContext.PhysicalShuttles.Add(shuttle);
        fixture.DbContext.ShuttleShifts.Add(shift);
        fixture.DbContext.Personnel.AddRange(assigned, unassigned);
        fixture.DbContext.PersonnelAssignments.Add(new PersonnelAssignment(
            assigned.Id,
            shift.Id,
            null,
            DateTimeOffset.UtcNow));
        fixture.DbContext.RoutePoints.Add(routePoint);
        fixture.DbContext.SavedRoutes.Add(new SavedRoute(
            shift.Id,
            "Sabah Rotasi",
            1200,
            600,
            "{}",
            DateTimeOffset.UtcNow));
        await fixture.DbContext.SaveChangesAsync();
        var repository = new DashboardRepository(fixture.DbContext);

        var summary = await repository.GetSummaryAsync(CancellationToken.None);

        Assert.Equal(2, summary.Metrics.TotalPersonnel);
        Assert.Equal(2, summary.Metrics.ActivePersonnel);
        Assert.Equal(1, summary.Metrics.AssignedPersonnel);
        Assert.Equal(1, summary.Metrics.UnassignedPersonnel);
        Assert.Equal(1, summary.Metrics.ActiveShuttles);
        Assert.Equal(1, summary.Metrics.ActiveShifts);
        Assert.Equal(1, summary.Metrics.RoutePointCount);
        Assert.Equal(1, summary.Metrics.SavedRouteCount);
        Assert.Single(summary.ShiftOccupancies);
        Assert.Equal(1, summary.ShiftOccupancies[0].Occupancy);
        Assert.Equal(19, summary.ShiftOccupancies[0].AvailableSeats);
        Assert.Equal(5m, summary.ShiftOccupancies[0].UtilizationPercent);
    }

    private sealed class DashboardFixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        public AppDbContext DbContext { get; }

        private DashboardFixture(SqliteConnection connection, AppDbContext dbContext)
        {
            _connection = connection;
            DbContext = dbContext;
        }

        public static async Task<DashboardFixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            var dbContext = new AppDbContext(options);
            await dbContext.Database.EnsureCreatedAsync();

            return new DashboardFixture(connection, dbContext);
        }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
