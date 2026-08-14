using Microsoft.EntityFrameworkCore;
using Tedas.Shuttle.Application.Interfaces;
using Tedas.Shuttle.Domain.Entities;
using Tedas.Shuttle.Infrastructure.Persistence;

namespace Tedas.Shuttle.Infrastructure.Repositories;

public sealed class SavedRouteRepository(AppDbContext dbContext) : ISavedRouteRepository
{
    public async Task<IReadOnlyList<SavedRoute>> ListByShiftAsync(
        Guid shuttleShiftId,
        CancellationToken cancellationToken)
    {
        return await dbContext.SavedRoutes
            .Include(route => route.ShuttleShift)
            .ThenInclude(shift => shift!.PhysicalShuttle)
            .Where(route => route.ShuttleShiftId == shuttleShiftId)
            .OrderByDescending(route => route.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public Task<ShuttleShift?> GetShiftByIdAsync(Guid shiftId, CancellationToken cancellationToken)
    {
        return dbContext.ShuttleShifts
            .Include(shift => shift.PhysicalShuttle)
            .FirstOrDefaultAsync(shift => shift.Id == shiftId, cancellationToken);
    }

    public async Task<IReadOnlyList<RoutePoint>> ListActiveRoutePointsByShiftAsync(
        Guid shiftId,
        CancellationToken cancellationToken)
    {
        return await dbContext.RoutePoints
            .AsNoTracking()
            .Where(routePoint => routePoint.ShuttleShiftId == shiftId && routePoint.IsActive)
            .OrderBy(routePoint => routePoint.Order)
            .ToArrayAsync(cancellationToken);
    }

    public async Task AddAsync(SavedRoute savedRoute, CancellationToken cancellationToken)
    {
        await dbContext.SavedRoutes.AddAsync(savedRoute, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}

