using Tedas.Shuttle.Domain.Entities;

namespace Tedas.Shuttle.Application.Interfaces;

public interface ISavedRouteRepository
{
    Task<IReadOnlyList<SavedRoute>> ListByShiftAsync(Guid shuttleShiftId, CancellationToken cancellationToken);

    Task<ShuttleShift?> GetShiftByIdAsync(Guid shiftId, CancellationToken cancellationToken);

    Task<IReadOnlyList<RoutePoint>> ListActiveRoutePointsByShiftAsync(Guid shiftId, CancellationToken cancellationToken);

    Task AddAsync(SavedRoute savedRoute, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

