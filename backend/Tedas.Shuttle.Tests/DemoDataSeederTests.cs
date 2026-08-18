using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Tedas.Shuttle.Infrastructure.Persistence;

namespace Tedas.Shuttle.Tests;

public sealed class DemoDataSeederTests
{
    [Fact]
    public async Task Seed_WithEmptyDatabase_CreatesDemoDataAndIsIdempotent()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext = new AppDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        DemoDataSeeder.Seed(dbContext, NullLogger.Instance);
        var firstCounts = await CountDemoDataAsync(dbContext);

        DemoDataSeeder.Seed(dbContext, NullLogger.Instance);
        var secondCounts = await CountDemoDataAsync(dbContext);

        Assert.Equal(new DemoDataCounts(24, 5, 9, 6, 18, 20, 5), firstCounts);
        Assert.Equal(firstCounts, secondCounts);
    }

    private static async Task<DemoDataCounts> CountDemoDataAsync(AppDbContext dbContext)
    {
        return new DemoDataCounts(
            await dbContext.Personnel.CountAsync(),
            await dbContext.PhysicalShuttles.CountAsync(),
            await dbContext.ShuttleShifts.CountAsync(),
            await dbContext.Drivers.CountAsync(),
            await dbContext.PersonnelAssignments.CountAsync(),
            await dbContext.RoutePoints.CountAsync(),
            await dbContext.SavedRoutes.CountAsync());
    }

    private sealed record DemoDataCounts(
        int Personnel,
        int Shuttles,
        int Shifts,
        int Drivers,
        int Assignments,
        int RoutePoints,
        int SavedRoutes);
}
