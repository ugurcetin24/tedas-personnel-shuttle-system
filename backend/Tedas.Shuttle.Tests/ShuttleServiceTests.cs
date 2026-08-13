using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Application.DTOs.Shuttles;
using Tedas.Shuttle.Application.Services;
using Tedas.Shuttle.Application.Validators;
using Tedas.Shuttle.Infrastructure.Persistence;
using Tedas.Shuttle.Infrastructure.Repositories;

namespace Tedas.Shuttle.Tests;

public sealed class ShuttleServiceTests
{
    [Fact]
    public async Task CreateAsync_WithValidRequest_CreatesActiveShuttle()
    {
        await using var fixture = await ShuttleServiceFixture.CreateAsync();
        var service = fixture.CreateService();

        var shuttle = await service.CreateAsync(
            CreateRequest("SERVIS-01", "06 TEST 01"),
            CancellationToken.None);

        Assert.Equal("SERVIS-01", shuttle.Code);
        Assert.Equal("06 TEST 01", shuttle.PlateNumber);
        Assert.True(shuttle.IsActive);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateCode_ThrowsConflict()
    {
        await using var fixture = await ShuttleServiceFixture.CreateAsync();
        var service = fixture.CreateService();
        await service.CreateAsync(CreateRequest("SERVIS-01", "06 TEST 01"), CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BusinessConflictException>(() =>
            service.CreateAsync(CreateRequest(" servis-01 ", "06 TEST 02"), CancellationToken.None));

        Assert.Equal("SHUTTLE_CODE_DUPLICATE", exception.Code);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingShuttle_UpdatesPlateAndDescription()
    {
        await using var fixture = await ShuttleServiceFixture.CreateAsync();
        var service = fixture.CreateService();
        var shuttle = await service.CreateAsync(CreateRequest("SERVIS-01", "06 TEST 01"), CancellationToken.None);

        var updated = await service.UpdateAsync(
            shuttle.Id,
            new UpdateShuttleRequest("06 TEST 99", "Güncel açıklama"),
            CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal("06 TEST 99", updated.PlateNumber);
        Assert.Equal("Güncel açıklama", updated.Description);
    }

    [Fact]
    public async Task UpdateStatusAsync_WithExistingShuttle_ChangesActiveStatus()
    {
        await using var fixture = await ShuttleServiceFixture.CreateAsync();
        var service = fixture.CreateService();
        var shuttle = await service.CreateAsync(CreateRequest("SERVIS-01", "06 TEST 01"), CancellationToken.None);

        var updated = await service.UpdateStatusAsync(
            shuttle.Id,
            new UpdateShuttleStatusRequest(IsActive: false),
            CancellationToken.None);

        Assert.NotNull(updated);
        Assert.False(updated.IsActive);
    }

    [Fact]
    public async Task SearchAsync_FiltersByPlateNumber()
    {
        await using var fixture = await ShuttleServiceFixture.CreateAsync();
        var service = fixture.CreateService();
        await service.CreateAsync(CreateRequest("SERVIS-01", "06 TEST 01"), CancellationToken.None);
        await service.CreateAsync(CreateRequest("SERVIS-02", "34 TEST 02"), CancellationToken.None);

        var result = await service.SearchAsync(
            new ShuttleQuery(PlateNumber: "34"),
            CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        Assert.Equal("SERVIS-02", result.Items.Single().Code);
    }

    private static CreateShuttleRequest CreateRequest(string code, string plateNumber)
    {
        return new CreateShuttleRequest(
            code,
            plateNumber,
            "Test servis aracı");
    }

    private sealed class ShuttleServiceFixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        public AppDbContext DbContext { get; }

        private ShuttleServiceFixture(SqliteConnection connection, AppDbContext dbContext)
        {
            _connection = connection;
            DbContext = dbContext;
        }

        public static async Task<ShuttleServiceFixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            var dbContext = new AppDbContext(options);
            await dbContext.Database.EnsureCreatedAsync();

            return new ShuttleServiceFixture(connection, dbContext);
        }

        public ShuttleService CreateService()
        {
            return new ShuttleService(
                new ShuttleRepository(DbContext),
                new CreateShuttleRequestValidator(),
                new UpdateShuttleRequestValidator());
        }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
