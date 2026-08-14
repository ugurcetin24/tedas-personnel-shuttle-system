namespace Tedas.Shuttle.Application.DTOs.Routing;

public sealed record SavedRouteDto(
    Guid Id,
    Guid ShuttleShiftId,
    string PhysicalShuttleCode,
    string ShiftName,
    string Name,
    double DistanceMeters,
    double DurationSeconds,
    string Geometry,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

