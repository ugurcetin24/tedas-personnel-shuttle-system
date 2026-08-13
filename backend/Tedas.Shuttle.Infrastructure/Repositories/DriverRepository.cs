using Microsoft.EntityFrameworkCore;
using Tedas.Shuttle.Application.Common;
using Tedas.Shuttle.Application.Interfaces;
using Tedas.Shuttle.Domain.Entities;
using Tedas.Shuttle.Infrastructure.Persistence;

namespace Tedas.Shuttle.Infrastructure.Repositories;

public sealed class DriverRepository(AppDbContext dbContext) : IDriverRepository
{
    public async Task<PaginatedList<Driver>> SearchAsync(
        int page,
        int pageSize,
        string? search,
        bool? isActive,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Drivers
            .AsNoTracking()
            .Include(driver => driver.ShuttleShift)
            .ThenInclude(shift => shift!.PhysicalShuttle)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            query = query.Where(driver =>
                EF.Functions.Like(driver.FirstName, $"%{normalizedSearch}%")
                || EF.Functions.Like(driver.LastName, $"%{normalizedSearch}%")
                || EF.Functions.Like(driver.Phone ?? string.Empty, $"%{normalizedSearch}%")
                || EF.Functions.Like(driver.LicenseNumber, $"%{normalizedSearch}%"));
        }

        if (isActive.HasValue)
        {
            query = query.Where(driver => driver.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(driver => driver.FirstName)
            .ThenBy(driver => driver.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

        return new PaginatedList<Driver>(items, page, pageSize, totalCount);
    }

    public Task<Driver?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Drivers
            .Include(driver => driver.ShuttleShift)
            .ThenInclude(shift => shift!.PhysicalShuttle)
            .FirstOrDefaultAsync(driver => driver.Id == id, cancellationToken);
    }

    public Task<bool> LicenseNumberExistsAsync(
        string licenseNumber,
        Guid? excludedDriverId,
        CancellationToken cancellationToken)
    {
        return dbContext.Drivers.AnyAsync(
            driver =>
                driver.LicenseNumber == licenseNumber
                && (!excludedDriverId.HasValue || driver.Id != excludedDriverId.Value),
            cancellationToken);
    }

    public Task<ShuttleShift?> GetShiftByIdAsync(Guid shiftId, CancellationToken cancellationToken)
    {
        return dbContext.ShuttleShifts
            .Include(shift => shift.PhysicalShuttle)
            .FirstOrDefaultAsync(shift => shift.Id == shiftId, cancellationToken);
    }

    public Task<bool> ShiftHasAssignedDriverAsync(
        Guid shiftId,
        Guid? excludedDriverId,
        CancellationToken cancellationToken)
    {
        return dbContext.Drivers.AnyAsync(
            driver =>
                driver.ShuttleShiftId == shiftId
                && (!excludedDriverId.HasValue || driver.Id != excludedDriverId.Value),
            cancellationToken);
    }

    public async Task AddAsync(Driver driver, CancellationToken cancellationToken)
    {
        await dbContext.Drivers.AddAsync(driver, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}

