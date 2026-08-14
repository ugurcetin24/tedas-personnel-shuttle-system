using FluentValidation;
using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Application.DTOs.Shifts;
using Tedas.Shuttle.Application.Interfaces;
using Tedas.Shuttle.Application.Services;
using Tedas.Shuttle.Application.Validators;
using Tedas.Shuttle.Domain.Entities;
using Tedas.Shuttle.Domain.Enums;

namespace Tedas.Shuttle.Tests;

public sealed class ShiftServiceTests
{
    [Fact]
    public async Task CreateAsync_WithExistingShuttle_CreatesActiveShift()
    {
        var repository = new FakeShiftRepository();
        var shuttle = repository.AddShuttle("SERVIS-01");
        var service = CreateService(repository);

        var shift = await service.CreateAsync(
            shuttle.Id,
            CreateRequest(capacity: 20),
            CancellationToken.None);

        Assert.NotNull(shift);
        Assert.Equal("Sabah", shift.Name);
        Assert.Equal(20, shift.Capacity);
        Assert.Equal(0, shift.Occupancy);
        Assert.Equal(20, shift.AvailableSeats);
        Assert.True(shift.IsActive);
    }

    [Fact]
    public async Task CreateAsync_WithMissingShuttle_ReturnsNull()
    {
        var service = CreateService(new FakeShiftRepository());

        var shift = await service.CreateAsync(
            Guid.NewGuid(),
            CreateRequest(capacity: 20),
            CancellationToken.None);

        Assert.Null(shift);
    }

    [Fact]
    public async Task UpdateAsync_CapacityCanIncrease()
    {
        var repository = new FakeShiftRepository();
        var shuttle = repository.AddShuttle("SERVIS-01");
        var service = CreateService(repository);
        var shift = await service.CreateAsync(shuttle.Id, CreateRequest(capacity: 20), CancellationToken.None);

        var updated = await service.UpdateAsync(
            shift!.Id,
            new UpdateShiftRequest("Sabah", ShiftType.Morning, 25, new TimeOnly(7, 30), new TimeOnly(9, 0)),
            CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal(25, updated.Capacity);
    }

    [Fact]
    public async Task UpdateAsync_CapacityCannotFallBelowOccupancy()
    {
        var repository = new FakeShiftRepository();
        var shuttle = repository.AddShuttle("SERVIS-01");
        var service = CreateService(repository);
        var shift = await service.CreateAsync(shuttle.Id, CreateRequest(capacity: 20), CancellationToken.None);
        repository.SetOccupancy(shift!.Id, 14);

        var exception = await Assert.ThrowsAsync<BusinessConflictException>(() =>
            service.UpdateAsync(
                shift.Id,
                new UpdateShiftRequest("Sabah", ShiftType.Morning, 13, new TimeOnly(7, 30), new TimeOnly(9, 0)),
                CancellationToken.None));

        Assert.Equal("SHIFT_CAPACITY_BELOW_OCCUPANCY", exception.Code);
    }

    [Fact]
    public async Task UpdateStatusAsync_WithExistingShift_ChangesActiveStatus()
    {
        var repository = new FakeShiftRepository();
        var shuttle = repository.AddShuttle("SERVIS-01");
        var service = CreateService(repository);
        var shift = await service.CreateAsync(shuttle.Id, CreateRequest(capacity: 20), CancellationToken.None);

        var updated = await service.UpdateStatusAsync(
            shift!.Id,
            new UpdateShiftStatusRequest(IsActive: false),
            CancellationToken.None);

        Assert.NotNull(updated);
        Assert.False(updated.IsActive);
    }

    private static ShiftService CreateService(IShiftRepository repository)
    {
        return new ShiftService(
            repository,
            new CreateShiftRequestValidator(),
            new UpdateShiftRequestValidator());
    }

    private static CreateShiftRequest CreateRequest(int capacity)
    {
        return new CreateShiftRequest(
            "Sabah",
            ShiftType.Morning,
            capacity,
            new TimeOnly(7, 30),
            new TimeOnly(9, 0));
    }

    private sealed class FakeShiftRepository : IShiftRepository
    {
        private readonly List<PhysicalShuttle> _shuttles = [];
        private readonly List<ShuttleShift> _shifts = [];
        private readonly Dictionary<Guid, int> _occupancyByShiftId = [];

        public PhysicalShuttle AddShuttle(string code)
        {
            var shuttle = new PhysicalShuttle(code, "06 TEST 01", "Test servis", DateTimeOffset.UtcNow);
            _shuttles.Add(shuttle);

            return shuttle;
        }

        public void SetOccupancy(Guid shiftId, int occupancy)
        {
            _occupancyByShiftId[shiftId] = occupancy;
        }

        public Task<IReadOnlyList<ShuttleShift>> ListAsync(
            bool? isActive,
            CancellationToken cancellationToken)
        {
            var shifts = _shifts
                .Where(shift => !isActive.HasValue || shift.IsActive == isActive.Value)
                .ToArray();

            return Task.FromResult<IReadOnlyList<ShuttleShift>>(shifts);
        }

        public Task<IReadOnlyList<ShuttleShift>> ListByShuttleAsync(
            Guid physicalShuttleId,
            CancellationToken cancellationToken)
        {
            var shifts = _shifts
                .Where(shift => shift.PhysicalShuttleId == physicalShuttleId)
                .ToArray();

            return Task.FromResult<IReadOnlyList<ShuttleShift>>(shifts);
        }

        public Task<IReadOnlyList<ShuttleShift>> ListByShuttleCodesAsync(
            IReadOnlyCollection<string> shuttleCodes,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<ShuttleShift>>([]);
        }

        public Task<IReadOnlyDictionary<string, PhysicalShuttle>> GetShuttlesByCodesAsync(
            IReadOnlyCollection<string> shuttleCodes,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyDictionary<string, PhysicalShuttle>>(
                new Dictionary<string, PhysicalShuttle>(StringComparer.OrdinalIgnoreCase));
        }

        public Task<IReadOnlyDictionary<Guid, int>> GetActiveAssignmentCountsAsync(
            IReadOnlyCollection<Guid> shiftIds,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyDictionary<Guid, int>>(
                _occupancyByShiftId
                    .Where(pair => shiftIds.Contains(pair.Key))
                    .ToDictionary(pair => pair.Key, pair => pair.Value));
        }

        public Task<ShuttleShift?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_shifts.FirstOrDefault(shift => shift.Id == id));
        }

        public Task<bool> ShuttleExistsAsync(Guid physicalShuttleId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_shuttles.Any(shuttle => shuttle.Id == physicalShuttleId));
        }

        public Task<int> GetActiveAssignmentCountAsync(Guid shiftId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_occupancyByShiftId.GetValueOrDefault(shiftId));
        }

        public Task AddAsync(ShuttleShift shift, CancellationToken cancellationToken)
        {
            _shifts.Add(shift);

            return Task.CompletedTask;
        }

        public Task AddRangeAsync(IReadOnlyCollection<ShuttleShift> shifts, CancellationToken cancellationToken)
        {
            _shifts.AddRange(shifts);

            return Task.CompletedTask;
        }

        public async Task ExecuteInTransactionAsync(
            Func<CancellationToken, Task> operation,
            CancellationToken cancellationToken)
        {
            await operation(cancellationToken);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
