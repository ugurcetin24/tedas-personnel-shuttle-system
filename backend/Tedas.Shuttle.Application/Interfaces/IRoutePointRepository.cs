using Tedas.Shuttle.Domain.Entities;

namespace Tedas.Shuttle.Application.Interfaces;

public interface IRoutePointRepository
{
    Task<IReadOnlyList<RoutePoint>> ListByShiftAsync(Guid shuttleShiftId, CancellationToken cancellationToken);

    Task<RoutePoint?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<ShuttleShift?> GetShiftByIdAsync(Guid shiftId, CancellationToken cancellationToken);

    Task<int> GetNextOrderAsync(Guid shuttleShiftId, CancellationToken cancellationToken);

    Task AddAsync(RoutePoint routePoint, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

