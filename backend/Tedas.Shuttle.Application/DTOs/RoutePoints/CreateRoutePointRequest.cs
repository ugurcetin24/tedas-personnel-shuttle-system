namespace Tedas.Shuttle.Application.DTOs.RoutePoints;

public sealed record CreateRoutePointRequest(
    string Name,
    string? Address,
    decimal Latitude,
    decimal Longitude);

