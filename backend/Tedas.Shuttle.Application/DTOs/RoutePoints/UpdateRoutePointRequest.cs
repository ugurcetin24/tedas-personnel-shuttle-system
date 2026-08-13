namespace Tedas.Shuttle.Application.DTOs.RoutePoints;

public sealed record UpdateRoutePointRequest(
    string Name,
    string? Address,
    decimal Latitude,
    decimal Longitude);

