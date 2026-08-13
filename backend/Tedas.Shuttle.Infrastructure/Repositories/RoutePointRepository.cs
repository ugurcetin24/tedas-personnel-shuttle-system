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

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}

