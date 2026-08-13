using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Application.DTOs.Assignments;
using Tedas.Shuttle.Application.DTOs.Personnel;
using Tedas.Shuttle.Application.DTOs.Shifts;
using Tedas.Shuttle.Application.DTOs.Shuttles;
using Tedas.Shuttle.Application.Services;
using Tedas.Shuttle.Application.Validators;
using Tedas.Shuttle.Domain.Enums;
using Tedas.Shuttle.Infrastructure.Persistence;
using Tedas.Shuttle.Infrastructure.Repositories;

namespace Tedas.Shuttle.Tests;

public sealed class AssignmentServiceTests
{
    [Fact]
    public async Task CreateAsync_WithActivePersonnelAndShift_CreatesAssignment()
    {
        await using var fixture = await AssignmentServiceFixture.CreateAsync();
        var personnel = await fixture.CreatePersonnelAsync("TEST-1001");
        var shift = await fixture.CreateShiftAsync(capacity: 2);
        var service = fixture.CreateAssignmentService();

        var assignment = await service.CreateAsync(
            new CreateAssignmentRequest(personnel.Id, shift.Id, null),
            CancellationToken.None);

        Assert.Equal(personnel.Id, assignment.PersonnelId);
        Assert.Equal(shift.Id, assignment.ShuttleShiftId);
        Assert.True(assignment.IsActive);
        Assert.Equal(1, assignment.Occupancy);
        Assert.Equal(1, assignment.AvailableSeats);
    }

    [Fact]
    public async Task CreateAsync_WithInactivePersonnel_ThrowsConflict()
    {
        await using var fixture = await AssignmentServiceFixture.CreateAsync();
        var personnel = await fixture.CreatePersonnelAsync("TEST-1001");
        await fixture.CreatePersonnelService().UpdateStatusAsync(
            personnel.Id,
            new UpdatePersonnelStatusRequest(IsActive: false),
            CancellationToken.None);
        var shift = await fixture.CreateShiftAsync(capacity: 2);
        var service = fixture.CreateAssignmentService();

        var exception = await Assert.ThrowsAsync<BusinessConflictException>(() =>
            service.CreateAsync(new CreateAssignmentRequest(personnel.Id, shift.Id, null), CancellationToken.None));

        Assert.Equal("PERSONNEL_INACTIVE", exception.Code);
    }

    [Fact]
    public async Task CreateAsync_WithInactiveShift_ThrowsConflict()
    {
        await using var fixture = await AssignmentServiceFixture.CreateAsync();
        var personnel = await fixture.CreatePersonnelAsync("TEST-1001");
        var shift = await fixture.CreateShiftAsync(capacity: 2);
        await fixture.CreateShiftService().UpdateStatusAsync(
            shift.Id,
            new UpdateShiftStatusRequest(IsActive: false),
            CancellationToken.None);
        var service = fixture.CreateAssignmentService();

        var exception = await Assert.ThrowsAsync<BusinessConflictException>(() =>
            service.CreateAsync(new CreateAssignmentRequest(personnel.Id, shift.Id, null), CancellationToken.None));

        Assert.Equal("SHIFT_INACTIVE", exception.Code);
    }

    [Fact]
    public async Task CreateAsync_WhenShiftIsFull_ThrowsConflict()
    {
        await using var fixture = await AssignmentServiceFixture.CreateAsync();
        var firstPersonnel = await fixture.CreatePersonnelAsync("TEST-1001");
        var secondPersonnel = await fixture.CreatePersonnelAsync("TEST-1002");
        var shift = await fixture.CreateShiftAsync(capacity: 1);
        var service = fixture.CreateAssignmentService();
        await service.CreateAsync(
            new CreateAssignmentRequest(firstPersonnel.Id, shift.Id, null),
            CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BusinessConflictException>(() =>
            service.CreateAsync(new CreateAssignmentRequest(secondPersonnel.Id, shift.Id, null), CancellationToken.None));

        Assert.Equal("SHUTTLE_CAPACITY_FULL", exception.Code);
    }

    [Fact]
    public async Task CreateAsync_WhenPersonnelAlreadyAssigned_ThrowsConflict()
    {
        await using var fixture = await AssignmentServiceFixture.CreateAsync();
        var personnel = await fixture.CreatePersonnelAsync("TEST-1001");
        var firstShift = await fixture.CreateShiftAsync("SERVIS-01", capacity: 2);
        var secondShift = await fixture.CreateShiftAsync("SERVIS-02", capacity: 2);
        var service = fixture.CreateAssignmentService();
        await service.CreateAsync(
            new CreateAssignmentRequest(personnel.Id, firstShift.Id, null),
            CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BusinessConflictException>(() =>
            service.CreateAsync(new CreateAssignmentRequest(personnel.Id, secondShift.Id, null), CancellationToken.None));

        Assert.Equal("PERSONNEL_ASSIGNMENT_DUPLICATE", exception.Code);
    }

    [Fact]
    public async Task DeactivateAsync_WithExistingAssignment_AllowsNewAssignment()
    {
        await using var fixture = await AssignmentServiceFixture.CreateAsync();
        var personnel = await fixture.CreatePersonnelAsync("TEST-1001");
        var firstShift = await fixture.CreateShiftAsync("SERVIS-01", capacity: 2);
        var secondShift = await fixture.CreateShiftAsync("SERVIS-02", capacity: 2);
        var service = fixture.CreateAssignmentService();
        var assignment = await service.CreateAsync(
            new CreateAssignmentRequest(personnel.Id, firstShift.Id, null),
            CancellationToken.None);

        var deactivated = await service.DeactivateAsync(assignment.Id, CancellationToken.None);
        var nextAssignment = await service.CreateAsync(
            new CreateAssignmentRequest(personnel.Id, secondShift.Id, null),
            CancellationToken.None);

        Assert.NotNull(deactivated);
        Assert.False(deactivated.IsActive);
        Assert.Equal(secondShift.Id, nextAssignment.ShuttleShiftId);
    }

    private sealed class AssignmentServiceFixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        public AppDbContext DbContext { get; }

        private AssignmentServiceFixture(SqliteConnection connection, AppDbContext dbContext)
        {
            _connection = connection;
            DbContext = dbContext;
        }

        public static async Task<AssignmentServiceFixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            var dbContext = new AppDbContext(options);
            await dbContext.Database.EnsureCreatedAsync();

            return new AssignmentServiceFixture(connection, dbContext);
        }

        public PersonnelService CreatePersonnelService()
        {
            return new PersonnelService(
                new PersonnelRepository(DbContext),
                new CreatePersonnelRequestValidator(),
                new UpdatePersonnelRequestValidator());
        }

        public ShiftService CreateShiftService()
        {
            return new ShiftService(
                new ShiftRepository(DbContext),
                new CreateShiftRequestValidator(),
                new UpdateShiftRequestValidator());
        }

        public AssignmentService CreateAssignmentService()
        {
            return new AssignmentService(
                new AssignmentRepository(DbContext),
                new CreateAssignmentRequestValidator());
        }

        public Task<PersonnelDto> CreatePersonnelAsync(string registrationNumber)
        {
            return CreatePersonnelService().CreateAsync(
                new CreatePersonnelRequest(
                    registrationNumber,
                    "Test",
                    registrationNumber[^4..],
                    "Bilgi Teknolojileri",
                    "Uzman",
                    null,
                    null,
                    null,
                    null,
                    null),
                CancellationToken.None);
        }

        public async Task<ShiftDto> CreateShiftAsync(int capacity)
        {
            return await CreateShiftAsync("SERVIS-01", capacity);
        }

        public async Task<ShiftDto> CreateShiftAsync(string shuttleCode, int capacity)
        {
            var shuttleService = new ShuttleService(
                new ShuttleRepository(DbContext),
                new CreateShuttleRequestValidator(),
                new UpdateShuttleRequestValidator());

            var shuttle = await shuttleService.CreateAsync(
                new CreateShuttleRequest(shuttleCode, $"06 {shuttleCode[^2..]} 01", "Test servis"),
                CancellationToken.None);

            var shift = await CreateShiftService().CreateAsync(
                shuttle.Id,
                new CreateShiftRequest(
                    "Sabah",
                    ShiftType.Morning,
                    capacity,
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

