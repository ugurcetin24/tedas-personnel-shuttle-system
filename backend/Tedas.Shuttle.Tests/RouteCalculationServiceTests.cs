using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Application.DTOs.Routing;
using Tedas.Shuttle.Application.DTOs.Shifts;
using Tedas.Shuttle.Application.DTOs.Shuttles;
using Tedas.Shuttle.Application.Interfaces;
using Tedas.Shuttle.Application.Services;
using Tedas.Shuttle.Application.Validators;
using Tedas.Shuttle.Domain.Enums;
using Tedas.Shuttle.Infrastructure.Persistence;
using Tedas.Shuttle.Infrastructure.Repositories;

namespace Tedas.Shuttle.Tests;

public sealed class RouteCalculationServiceTests
{
    [Fact]
    public async Task CalculateByShiftAsync_WithLessThanTwoActivePoints_ThrowsConflict()
    {
        await using var fixture = await RouteCalculationServiceFixture.CreateAsync();
        var shift = await fixture.CreateShiftAsync();
        await fixture.AddRoutePointAsync(shift.Id, "TEDAS", 39.920m, 32.850m);
        var service = fixture.CreateService(new StubRoutingService(FixedRoute()));

        var exception = await Assert.ThrowsAsync<BusinessConflictException>(() =>
            service.CalculateByShiftAsync(shift.Id, CancellationToken.None));

        Assert.Equal("ROUTE_POINT_COUNT_INSUFFICIENT", exception.Code);
    }

    [Fact]
    public async Task SaveCalculatedRouteAsync_WithCalculatedRoute_PersistsSavedRoute()
    {
        await using var fixture = await RouteCalculationServiceFixture.CreateAsync();
        var shift = await fixture.CreateShiftAsync();
        await fixture.AddRoutePointAsync(shift.Id, "TEDAS", 39.920m, 32.850m);
        await fixture.AddRoutePointAsync(shift.Id, "Kizilay", 39.921m, 32.854m);
        var service = fixture.CreateService(new StubRoutingService(FixedRoute()));

        var saved = await service.SaveCalculatedRouteAsync(
            shift.Id,
            new SaveRouteRequest("  Sabah Rotasi  "),
            CancellationToken.None);
        var savedRoutes = await service.ListSavedByShiftAsync(shift.Id, CancellationToken.None);

        Assert.NotNull(saved);
        Assert.Equal("Sabah Rotasi", saved.Name);
        Assert.Equal(1234.5, saved.DistanceMeters);
        Assert.Single(savedRoutes!);
        Assert.Equal(saved.Id, savedRoutes![0].Id);
    }

    [Fact]
    public async Task SaveCalculatedRouteAsync_WhenRoutingFails_ThrowsConflict()
    {
        await using var fixture = await RouteCalculationServiceFixture.CreateAsync();
        var shift = await fixture.CreateShiftAsync();
        await fixture.AddRoutePointAsync(shift.Id, "TEDAS", 39.920m, 32.850m);
        await fixture.AddRoutePointAsync(shift.Id, "Kizilay", 39.921m, 32.854m);
        var service = fixture.CreateService(new StubRoutingService(null));

        var exception = await Assert.ThrowsAsync<BusinessConflictException>(() =>
            service.SaveCalculatedRouteAsync(
                shift.Id,
                new SaveRouteRequest("Sabah Rotasi"),
                CancellationToken.None));

        Assert.Equal("ROUTE_CALCULATION_FAILED", exception.Code);
    }

    private static CalculatedRouteDto FixedRoute()
    {
        return new CalculatedRouteDto(
            1234.5,
            678.9,
            """{"type":"LineString","coordinates":[[32.85,39.92],[32.854,39.921]]}""",
            [
                new RouteCoordinateDto(39.920m, 32.850m),
                new RouteCoordinateDto(39.921m, 32.854m)
            ]);
    }

    private sealed class StubRoutingService(CalculatedRouteDto? route) : IRoutingService
    {
        public Task<CalculatedRouteDto?> CalculateAsync(
            IReadOnlyList<RouteCoordinateDto> coordinates,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(route);
        }
    }

    private sealed class RouteCalculationServiceFixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private AppDbContext DbContext { get; }

        private RouteCalculationServiceFixture(SqliteConnection connection, AppDbContext dbContext)
        {
            _connection = connection;
            DbContext = dbContext;
        }

        public static async Task<RouteCalculationServiceFixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            var dbContext = new AppDbContext(options);
            await dbContext.Database.EnsureCreatedAsync();

            return new RouteCalculationServiceFixture(connection, dbContext);
        }

        public RouteCalculationService CreateService(IRoutingService routingService)
        {
            return new RouteCalculationService(
                new SavedRouteRepository(DbContext),
                routingService);
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

            return (await shiftService.CreateAsync(
                shuttle.Id,
                new CreateShiftRequest("Sabah", ShiftType.Morning, 20, new TimeOnly(7, 30), new TimeOnly(9, 0)),
                CancellationToken.None))!;
        }

        public async Task AddRoutePointAsync(Guid shiftId, string name, decimal latitude, decimal longitude)
        {
            var routePointService = new RoutePointService(
                new RoutePointRepository(DbContext),
                new CreateRoutePointRequestValidator(),
                new UpdateRoutePointRequestValidator());

            await routePointService.CreateAsync(
                shiftId,
                new Application.DTOs.RoutePoints.CreateRoutePointRequest(name, null, latitude, longitude),
                CancellationToken.None);
        }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
