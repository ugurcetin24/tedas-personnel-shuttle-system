using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Application.DTOs.RoutePoints;
using Tedas.Shuttle.Application.DTOs.Shifts;
using Tedas.Shuttle.Application.DTOs.Shuttles;
using Tedas.Shuttle.Application.Services;
using Tedas.Shuttle.Application.Validators;
using Tedas.Shuttle.Domain.Enums;
using Tedas.Shuttle.Infrastructure.Persistence;
using Tedas.Shuttle.Infrastructure.Repositories;

namespace Tedas.Shuttle.Tests;

public sealed class RoutePointServiceTests
{
    [Fact]
    public async Task CreateAsync_WithExistingShift_AppendsOrder()
    {
        await using var fixture = await RoutePointServiceFixture.CreateAsync();
        var shift = await fixture.CreateShiftAsync();
        var service = fixture.CreateRoutePointService();

        var first = await service.CreateAsync(shift.Id, CreateRequest("TEDAS", 39.920m, 32.850m), CancellationToken.None);
        var second = await service.CreateAsync(shift.Id, CreateRequest("Kizilay", 39.921m, 32.854m), CancellationToken.None);

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal(1, first.Order);
        Assert.Equal(2, second.Order);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingRoutePoint_UpdatesEditableFields()
    {
        await using var fixture = await RoutePointServiceFixture.CreateAsync();
        var shift = await fixture.CreateShiftAsync();
        var service = fixture.CreateRoutePointService();
        var routePoint = await service.CreateAsync(shift.Id, CreateRequest("TEDAS", 39.920m, 32.850m), CancellationToken.None);

        var updated = await service.UpdateAsync(
            routePoint!.Id,
            new UpdateRoutePointRequest("Genel Mudurluk", "Adres", 39.930m, 32.860m),
            CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal("Genel Mudurluk", updated.Name);
        Assert.Equal("Adres", updated.Address);
        Assert.Equal(39.930m, updated.Latitude);
    }

    [Fact]
    public async Task UpdateStatusAsync_WithExistingRoutePoint_ChangesActiveStatus()
    {
        await using var fixture = await RoutePointServiceFixture.CreateAsync();
        var shift = await fixture.CreateShiftAsync();
        var service = fixture.CreateRoutePointService();
        var routePoint = await service.CreateAsync(shift.Id, CreateRequest("TEDAS", 39.920m, 32.850m), CancellationToken.None);

        var updated = await service.UpdateStatusAsync(
            routePoint!.Id,
            new UpdateRoutePointStatusRequest(IsActive: false),
            CancellationToken.None);

        Assert.NotNull(updated);
        Assert.False(updated.IsActive);
    }

    [Fact]
    public async Task ReorderAsync_WithCompleteList_UpdatesOrder()
    {
        await using var fixture = await RoutePointServiceFixture.CreateAsync();
        var shift = await fixture.CreateShiftAsync();
        var service = fixture.CreateRoutePointService();
        var first = await service.CreateAsync(shift.Id, CreateRequest("TEDAS", 39.920m, 32.850m), CancellationToken.None);
        var second = await service.CreateAsync(shift.Id, CreateRequest("Kizilay", 39.921m, 32.854m), CancellationToken.None);

        var reordered = await service.ReorderAsync(
            shift.Id,
            new ReorderRoutePointsRequest([second!.Id, first!.Id]),
            CancellationToken.None);

        Assert.NotNull(reordered);
        Assert.Equal(second.Id, reordered[0].Id);
        Assert.Equal(1, reordered[0].Order);
        Assert.Equal(first.Id, reordered[1].Id);
        Assert.Equal(2, reordered[1].Order);
    }

    [Fact]
    public async Task ReorderAsync_WithMissingRoutePoint_ThrowsConflict()
    {
        await using var fixture = await RoutePointServiceFixture.CreateAsync();
        var shift = await fixture.CreateShiftAsync();
        var service = fixture.CreateRoutePointService();
        var routePoint = await service.CreateAsync(shift.Id, CreateRequest("TEDAS", 39.920m, 32.850m), CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BusinessConflictException>(() =>
            service.ReorderAsync(
                shift.Id,
                new ReorderRoutePointsRequest([routePoint!.Id, Guid.NewGuid()]),
                CancellationToken.None));

        Assert.Equal("ROUTE_POINT_ORDER_INVALID", exception.Code);
    }

    private static CreateRoutePointRequest CreateRequest(string name, decimal latitude, decimal longitude)
    {
        return new CreateRoutePointRequest(name, null, latitude, longitude);
    }

    private sealed class RoutePointServiceFixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        public AppDbContext DbContext { get; }

        private RoutePointServiceFixture(SqliteConnection connection, AppDbContext dbContext)
        {
            _connection = connection;
            DbContext = dbContext;
        }

        public static async Task<RoutePointServiceFixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            var dbContext = new AppDbContext(options);
            await dbContext.Database.EnsureCreatedAsync();

            return new RoutePointServiceFixture(connection, dbContext);
        }

        public RoutePointService CreateRoutePointService()
        {
            return new RoutePointService(
                new RoutePointRepository(DbContext),
                new CreateRoutePointRequestValidator(),
                new UpdateRoutePointRequestValidator());
        }

        public async Task<ShiftDto> CreateShiftAsync()
        {
            var shuttleService = new ShuttleService(
                new ShuttleRepository(DbContext),
                new CreateShuttleRequestValidator(),
                new UpdateShuttleRequestValidator());

            var shiftService = new ShiftService(
                new ShiftRepository(DbContext),
                new CreateShiftRequestValidator(),
                new UpdateShiftRequestValidator());

            var shuttle = await shuttleService.CreateAsync(
                new CreateShuttleRequest("SERVIS-01", "06 TEST 01", "Test servis"),
                CancellationToken.None);

            var shift = await shiftService.CreateAsync(
                shuttle.Id,
                new CreateShiftRequest("Sabah", ShiftType.Morning, 20, new TimeOnly(7, 30), new TimeOnly(9, 0)),
                CancellationToken.None);

            return shift!;
        }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}

