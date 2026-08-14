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

    public async Task<IReadOnlyList<ShuttleShift>> ListByShuttleCodesAsync(
        IReadOnlyCollection<string> shuttleCodes,
        CancellationToken cancellationToken)
    {
        if (shuttleCodes.Count == 0)
        {
            return [];
        }

        return await dbContext.ShuttleShifts
            .Include(shift => shift.PhysicalShuttle)
            .Where(shift => shuttleCodes.Contains(shift.PhysicalShuttle!.Code))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<string, PhysicalShuttle>> GetShuttlesByCodesAsync(
        IReadOnlyCollection<string> shuttleCodes,
        CancellationToken cancellationToken)
    {
        if (shuttleCodes.Count == 0)
        {
            return new Dictionary<string, PhysicalShuttle>(StringComparer.OrdinalIgnoreCase);
        }

        var shuttles = await dbContext.PhysicalShuttles
            .Where(shuttle => shuttleCodes.Contains(shuttle.Code))
            .ToArrayAsync(cancellationToken);

        return shuttles.ToDictionary(
            shuttle => shuttle.Code,
            StringComparer.OrdinalIgnoreCase);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetActiveAssignmentCountsAsync(
        IReadOnlyCollection<Guid> shiftIds,
        CancellationToken cancellationToken)
    {
        if (shiftIds.Count == 0)
        {
            return new Dictionary<Guid, int>();
        }

        return await dbContext.PersonnelAssignments
            .Where(assignment => shiftIds.Contains(assignment.ShuttleShiftId) && assignment.IsActive)
            .GroupBy(assignment => assignment.ShuttleShiftId)
            .Select(group => new { ShiftId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.ShiftId, item => item.Count, cancellationToken);
    }

    public Task<bool> ShuttleExistsAsync(Guid physicalShuttleId, CancellationToken cancellationToken)
    {
        return dbContext.PhysicalShuttles.AnyAsync(
            shuttle => shuttle.Id == physicalShuttleId,
            cancellationToken);
    }

    public Task<int> GetActiveAssignmentCountAsync(Guid shiftId, CancellationToken cancellationToken)
    {
        return dbContext.PersonnelAssignments.CountAsync(
            assignment => assignment.ShuttleShiftId == shiftId && assignment.IsActive,
            cancellationToken);
    }

    public async Task AddAsync(ShuttleShift shift, CancellationToken cancellationToken)
    {
        await dbContext.ShuttleShifts.AddAsync(shift, cancellationToken);
    }

    public async Task AddRangeAsync(IReadOnlyCollection<ShuttleShift> shifts, CancellationToken cancellationToken)
    {
        await dbContext.ShuttleShifts.AddRangeAsync(shifts, cancellationToken);
    }

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await operation(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
