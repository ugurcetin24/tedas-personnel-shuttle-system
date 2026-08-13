using Microsoft.EntityFrameworkCore;
using Tedas.Shuttle.Application.Interfaces;
using Tedas.Shuttle.Domain.Entities;
using Tedas.Shuttle.Infrastructure.Persistence;

namespace Tedas.Shuttle.Infrastructure.Repositories;

public sealed class ShiftRepository(AppDbContext dbContext) : IShiftRepository
{
    public async Task<IReadOnlyList<ShuttleShift>> ListAsync(
        bool? isActive,
        CancellationToken cancellationToken)
    {
        var query = dbContext.ShuttleShifts
            .AsNoTracking()
            .Include(shift => shift.PhysicalShuttle)
            .AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(shift => shift.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(shift => shift.PhysicalShuttle!.Code)
            .ThenBy(shift => shift.StartTime)
            .ThenBy(shift => shift.Name)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ShuttleShift>> ListByShuttleAsync(
        Guid physicalShuttleId,
        CancellationToken cancellationToken)
    {
        return await dbContext.ShuttleShifts
            .AsNoTracking()
            .Include(shift => shift.PhysicalShuttle)
            .Where(shift => shift.PhysicalShuttleId == physicalShuttleId)
            .OrderBy(shift => shift.StartTime)
            .ThenBy(shift => shift.Name)
            .ToArrayAsync(cancellationToken);
    }

    public Task<ShuttleShift?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.ShuttleShifts
            .Include(shift => shift.PhysicalShuttle)
            .FirstOrDefaultAsync(shift => shift.Id == id, cancellationToken);
    }

    public Task<bool> ShuttleExistsAsync(Guid physicalShuttleId, CancellationToken cancellationToken)
    {
        return dbContext.PhysicalShuttles.AnyAsync(
            shuttle => shuttle.Id == physicalShuttleId,
            cancellationToken);
    }

    public Task<int> GetActiveAssignmentCountAsync(Guid shiftId, CancellationToken cancellationToken)
    {
        return Task.FromResult(0);
    }

    public async Task AddAsync(ShuttleShift shift, CancellationToken cancellationToken)
    {
        await dbContext.ShuttleShifts.AddAsync(shift, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
