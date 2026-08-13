using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Domain.Entities;

namespace Tedas.Shuttle.Application.Interfaces;

public interface IDriverRepository
{
    Task<PaginatedList<Driver>> SearchAsync(
        int page,
        int pageSize,
        string? search,
        bool? isActive,
        CancellationToken cancellationToken);

    Task<Driver?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> LicenseNumberExistsAsync(
        string licenseNumber,
        Guid? excludedDriverId,
        CancellationToken cancellationToken);

    Task<ShuttleShift?> GetShiftByIdAsync(Guid shiftId, CancellationToken cancellationToken);

    Task<bool> ShiftHasAssignedDriverAsync(
        Guid shiftId,
        Guid? excludedDriverId,
        CancellationToken cancellationToken);

    Task AddAsync(Driver driver, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

