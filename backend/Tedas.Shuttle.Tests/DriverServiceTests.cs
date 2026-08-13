using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Application.DTOs.Drivers;
using Tedas.Shuttle.Application.DTOs.Shifts;
using Tedas.Shuttle.Application.DTOs.Shuttles;
using Tedas.Shuttle.Application.Services;
using Tedas.Shuttle.Application.Validators;
using Tedas.Shuttle.Domain.Enums;
using Tedas.Shuttle.Infrastructure.Persistence;
using Tedas.Shuttle.Infrastructure.Repositories;

namespace Tedas.Shuttle.Tests;

public sealed class DriverServiceTests
{
    [Fact]
    public async Task CreateAsync_WithValidRequest_CreatesActiveDriver()
    {
        await using var fixture = await DriverServiceFixture.CreateAsync();
        var service = fixture.CreateDriverService();

        var driver = await service.CreateAsync(
            CreateDriverRequest("Ali", "Yilmaz", "0555 000 0000", "B-123"),
            CancellationToken.None);

        Assert.Equal("Ali Yilmaz", driver.FullName);
        Assert.Equal("B-123", driver.LicenseNumber);
        Assert.True(driver.IsActive);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateLicenseNumber_ThrowsConflict()
    {
        await using var fixture = await DriverServiceFixture.CreateAsync();
        var service = fixture.CreateDriverService();
        await service.CreateAsync(CreateDriverRequest("Ali", "Yilmaz", null, "B-123"), CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BusinessConflictException>(() =>
            service.CreateAsync(CreateDriverRequest("Veli", "Demir", null, " b-123 "), CancellationToken.None));

        Assert.Equal("DRIVER_LICENSE_DUPLICATE", exception.Code);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingDriver_UpdatesEditableFields()
    {
        await using var fixture = await DriverServiceFixture.CreateAsync();
        var service = fixture.CreateDriverService();
        var driver = await service.CreateAsync(CreateDriverRequest("Ali", "Yilmaz", null, "B-123"), CancellationToken.None);

        var updated = await service.UpdateAsync(
            driver.Id,
            new UpdateDriverRequest("Ayse", "Kaya", "0555 111 1111", "C-456"),
            CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal("Ayse Kaya", updated.FullName);
        Assert.Equal("C-456", updated.LicenseNumber);
        Assert.Equal("0555 111 1111", updated.Phone);
    }

    [Fact]
    public async Task UpdateStatusAsync_WithExistingDriver_ChangesActiveStatus()
    {
        await using var fixture = await DriverServiceFixture.CreateAsync();
        var service = fixture.CreateDriverService();
        var driver = await service.CreateAsync(CreateDriverRequest("Ali", "Yilmaz", null, "B-123"), CancellationToken.None);

        var updated = await service.UpdateStatusAsync(
            driver.Id,
            new UpdateDriverStatusRequest(IsActive: false),
            CancellationToken.None);

        Assert.NotNull(updated);
        Assert.False(updated.IsActive);
    }

    [Fact]
    public async Task UpdateShiftAssignmentAsync_WithActiveShift_AssignsDriver()
    {
        await using var fixture = await DriverServiceFixture.CreateAsync();
        var driverService = fixture.CreateDriverService();
        var shift = await fixture.CreateShiftAsync();
        var driver = await driverService.CreateAsync(CreateDriverRequest("Ali", "Yilmaz", null, "B-123"), CancellationToken.None);

        var updated = await driverService.UpdateShiftAssignmentAsync(
            driver.Id,
            new UpdateDriverShiftAssignmentRequest(shift.Id),
            CancellationToken.None);

        Assert.NotNull(updated);
        Assert.NotNull(updated.AssignedShift);
        Assert.Equal(shift.Id, updated.AssignedShift.ShuttleShiftId);
    }

    [Fact]
    public async Task UpdateShiftAssignmentAsync_WhenShiftAlreadyHasDriver_ThrowsConflict()
    {
        await using var fixture = await DriverServiceFixture.CreateAsync();
        var driverService = fixture.CreateDriverService();
        var shift = await fixture.CreateShiftAsync();
        var firstDriver = await driverService.CreateAsync(CreateDriverRequest("Ali", "Yilmaz", null, "B-123"), CancellationToken.None);
        var secondDriver = await driverService.CreateAsync(CreateDriverRequest("Veli", "Demir", null, "C-456"), CancellationToken.None);
        await driverService.UpdateShiftAssignmentAsync(
            firstDriver.Id,
            new UpdateDriverShiftAssignmentRequest(shift.Id),
            CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BusinessConflictException>(() =>
            driverService.UpdateShiftAssignmentAsync(
                secondDriver.Id,
                new UpdateDriverShiftAssignmentRequest(shift.Id),
                CancellationToken.None));

        Assert.Equal("SHIFT_DRIVER_ALREADY_ASSIGNED", exception.Code);
    }

    [Fact]
    public async Task UpdateShiftAssignmentAsync_WithNullShift_RemovesAssignment()
    {
        await using var fixture = await DriverServiceFixture.CreateAsync();
        var driverService = fixture.CreateDriverService();
        var shift = await fixture.CreateShiftAsync();
        var driver = await driverService.CreateAsync(CreateDriverRequest("Ali", "Yilmaz", null, "B-123"), CancellationToken.None);
        await driverService.UpdateShiftAssignmentAsync(
            driver.Id,
            new UpdateDriverShiftAssignmentRequest(shift.Id),
            CancellationToken.None);

        var updated = await driverService.UpdateShiftAssignmentAsync(
            driver.Id,
            new UpdateDriverShiftAssignmentRequest(null),
            CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Null(updated.AssignedShift);
    }

    private static CreateDriverRequest CreateDriverRequest(
        string firstName,
        string lastName,
        string? phone,
        string licenseNumber)
    {
        return new CreateDriverRequest(firstName, lastName, phone, licenseNumber);
    }

    private sealed class DriverServiceFixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        public AppDbContext DbContext { get; }

        private DriverServiceFixture(SqliteConnection connection, AppDbContext dbContext)
        {
            _connection = connection;
            DbContext = dbContext;
        }

        public static async Task<DriverServiceFixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            var dbContext = new AppDbContext(options);
            await dbContext.Database.EnsureCreatedAsync();

            return new DriverServiceFixture(connection, dbContext);
        }

        public DriverService CreateDriverService()
        {
            return new DriverService(
                new DriverRepository(DbContext),
                new CreateDriverRequestValidator(),
                new UpdateDriverRequestValidator());
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
                new CreateShiftRequest(
                    "Sabah",
                    ShiftType.Morning,
                    20,
                    new TimeOnly(7, 30),
                    new TimeOnly(9, 0)),
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

