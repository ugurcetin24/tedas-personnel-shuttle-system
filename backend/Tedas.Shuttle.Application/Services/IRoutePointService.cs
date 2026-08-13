using Tedas.Shuttle.Application.DTOs.RoutePoints;

namespace Tedas.Shuttle.Application.Services;

public interface IRoutePointService
{
    Task<IReadOnlyList<RoutePointListItemDto>?> ListByShiftAsync(Guid shuttleShiftId, CancellationToken cancellationToken);

    Task<RoutePointDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<RoutePointDto?> CreateAsync(Guid shuttleShiftId, CreateRoutePointRequest request, CancellationToken cancellationToken);

    Task<RoutePointDto?> UpdateAsync(Guid id, UpdateRoutePointRequest request, CancellationToken cancellationToken);

    Task<RoutePointDto?> UpdateStatusAsync(Guid id, UpdateRoutePointStatusRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<RoutePointListItemDto>?> ReorderAsync(Guid shuttleShiftId, ReorderRoutePointsRequest request, CancellationToken cancellationToken);
}

