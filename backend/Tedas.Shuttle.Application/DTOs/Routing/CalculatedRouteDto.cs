namespace Tedas.Shuttle.Application.DTOs.Routing;

public sealed record CalculatedRouteDto(
    double DistanceMeters,
    double DurationSeconds,
    string Geometry,
    IReadOnlyList<RouteCoordinateDto> Coordinates);

