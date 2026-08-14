using Microsoft.EntityFrameworkCore;
using Tedas.Shuttle.Application.Interfaces;
using Tedas.Shuttle.Domain.Entities;
using Tedas.Shuttle.Infrastructure.Persistence;

namespace Tedas.Shuttle.Infrastructure.Repositories;

public sealed class RoutePointRepository(AppDbContext dbContext) : IRoutePointRepository
{
    public async Task<IReadOnlyList<RoutePoint>> ListByShiftAsync(
        Guid shuttleShiftId,
        CancellationToken cancellationToken)
    {
        return await dbContext.RoutePoints
            .Include(routePoint => routePoint.ShuttleShift)
            .ThenInclude(shift => shift!.PhysicalShuttle)
            .Where(routePoint => routePoint.ShuttleShiftId == shuttleShiftId)
            .OrderBy(routePoint => routePoint.Order)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RoutePoint>> ListByShiftIdsAsync(
        IReadOnlyCollection<Guid> shuttleShiftIds,
        CancellationToken cancellationToken)
    {
        if (shuttleShiftIds.Count == 0)
        {
            return [];
        }

        return await dbContext.RoutePoints
            .Include(routePoint => routePoint.ShuttleShift)
            .ThenInclude(shift => shift!.PhysicalShuttle)
            .Where(routePoint => shuttleShiftIds.Contains(routePoint.ShuttleShiftId))
            .OrderBy(routePoint => routePoint.ShuttleShift!.PhysicalShuttle!.Code)
            .ThenBy(routePoint => routePoint.ShuttleShift!.Name)
            .ThenBy(routePoint => routePoint.Order)
            .ToArrayAsync(cancellationToken);
    }

    public Task<RoutePoint?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.RoutePoints
            .Include(routePoint => routePoint.ShuttleShift)
            .ThenInclude(shift => shift!.PhysicalShuttle)
            .FirstOrDefaultAsync(routePoint => routePoint.Id == id, cancellationToken);
    }

    public Task<ShuttleShift?> GetShiftByIdAsync(Guid shiftId, CancellationToken cancellationToken)
    {
        return dbContext.ShuttleShifts
            .Include(shift => shift.PhysicalShuttle)
            .FirstOrDefaultAsync(shift => shift.Id == shiftId, cancellationToken);
    }

    public async Task<int> GetNextOrderAsync(Guid shuttleShiftId, CancellationToken cancellationToken)
    {
        var maxOrder = await dbContext.RoutePoints
            .Where(routePoint => routePoint.ShuttleShiftId == shuttleShiftId)
            .Select(routePoint => (int?)routePoint.Order)
            .MaxAsync(cancellationToken);

        return (maxOrder ?? 0) + 1;
    }

    public async Task AddAsync(RoutePoint routePoint, CancellationToken cancellationToken)
    {
        await dbContext.RoutePoints.AddAsync(routePoint, cancellationToken);
    }

    public async Task AddRangeAsync(IReadOnlyCollection<RoutePoint> routePoints, CancellationToken cancellationToken)
    {
        await dbContext.RoutePoints.AddRangeAsync(routePoints, cancellationToken);
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
