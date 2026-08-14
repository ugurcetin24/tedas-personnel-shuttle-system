using Tedas.Shuttle.Application.DTOs.Routing;

namespace Tedas.Shuttle.Application.Interfaces;

public interface IRoutingService
{
    Task<CalculatedRouteDto?> CalculateAsync(
        IReadOnlyList<RouteCoordinateDto> coordinates,
        CancellationToken cancellationToken);
}

