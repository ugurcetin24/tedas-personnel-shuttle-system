using Tedas.Shuttle.Domain.Entities;

namespace Tedas.Shuttle.Application.Interfaces;

public interface IRoutePointRepository
{
    Task<IReadOnlyList<RoutePoint>> ListByShiftAsync(Guid shuttleShiftId, CancellationToken cancellationToken);

    Task<IReadOnlyList<RoutePoint>> ListByShiftIdsAsync(
        IReadOnlyCollection<Guid> shuttleShiftIds,
        CancellationToken cancellationToken);

    Task<RoutePoint?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<ShuttleShift?> GetShiftByIdAsync(Guid shiftId, CancellationToken cancellationToken);

    Task<int> GetNextOrderAsync(Guid shuttleShiftId, CancellationToken cancellationToken);

    Task AddAsync(RoutePoint routePoint, CancellationToken cancellationToken);

    Task AddRangeAsync(IReadOnlyCollection<RoutePoint> routePoints, CancellationToken cancellationToken);

    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
